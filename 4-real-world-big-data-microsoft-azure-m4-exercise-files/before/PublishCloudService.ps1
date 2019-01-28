# http://azure.microsoft.com/en-us/documentation/articles/cloud-services-dotnet-continuous-delivery/

param(
	[Parameter(Mandatory=$true)]
	$SelectedSubscription,
	
	[Parameter(Mandatory=$true)]
	$ServiceName,
	
	[Parameter(Mandatory=$true)]
	$StorageAccountName,
	
	[Parameter(Mandatory=$true)]
	$PackageLocation,
	
	[Parameter(Mandatory=$true)]
	$CloudConfigLocation,
	
	$Environment = "Staging",
	
	$DeploymentLabel = "ContinuousDeploy to $servicename",
	
	$TimeStampFormat = "g",
	
	$AlwaysDeleteExistingDeployments = 1,
	
	$EnableDeploymentUpgrade = 1,
	
	$AzureModulePath = (Join-Path -Path "${env:ProgramFiles(x86)}" -ChildPath "Microsoft SDKs\Azure\PowerShell\ServiceManagement\Azure\Azure.psd1")
)

function Publish()
{
    $deployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Environment -ErrorVariable a -ErrorAction silentlycontinue 
    if ($a[0] -ne $null)
    {
        Write-Output "$(Get-Date -f $timeStampFormat) - No deployment is detected. Creating a new deployment. "
    }
	
    #check for existing deployment and then either upgrade, delete + deploy, or cancel according to $alwaysDeleteExistingDeployments and $enableDeploymentUpgrade boolean variables
    if ($deployment.Name -ne $null)
    {
        switch ($AlwaysDeleteExistingDeployments)
        {
            1
            {
                switch ($EnableDeploymentUpgrade)
                {
                    1  #Update deployment inplace (usually faster, cheaper, won't destroy VIP)
                    {
                        Write-Output "$(Get-Date -f $timeStampFormat) - Deployment exists in $servicename.  Upgrading deployment."
                        UpgradeDeployment
                    }
                    0  #Delete then create new deployment
                    {
                        Write-Output "$(Get-Date -f $timeStampFormat) - Deployment exists in $servicename.  Deleting deployment."
                        DeleteDeployment
                        CreateNewDeployment
                        
                    }
                } # switch ($enableDeploymentUpgrade)
            }
            0
            {
                Write-Output "$(Get-Date -f $timeStampFormat) - ERROR: Deployment exists in $servicename.  Script execution cancelled."
                exit
            }
        } #switch ($alwaysDeleteExistingDeployments)
    } else {
            CreateNewDeployment
    }
}

function CreateNewDeployment()
{
    Write-Progress -Id 3 -Activity "Creating New Deployment" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: In progress"

    $opstat = New-AzureDeployment -Slot $Environment -Package $PackageLocation -Configuration $CloudConfigLocation -label $DeploymentLabel -ServiceName $ServiceName
        
    $completeDeployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Environment
    $completeDeploymentID = $completeDeployment.deploymentid

    Write-Progress -Id 3 -Activity "Creating New Deployment" -Completed -Status "Complete"
    Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"
    
    StartInstances
}

function UpgradeDeployment()
{
    Write-Progress -Id 3 -Activity "Upgrading Deployment" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: In progress"

    # perform Update-Deployment
    $setdeployment = Set-AzureDeployment -Upgrade -Slot $Environment -Package $PackageLocation -Configuration $CloudConfigLocation -label $DeploymentLabel -ServiceName $ServiceName -Force
    
    $completeDeployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Environment
    $completeDeploymentID = $completeDeployment.deploymentid
    
    Write-Progress -Id 3 -Activity "Upgrading Deployment" -Completed -Status "Complete"
    Write-Output "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: Complete, Deployment ID: $completeDeploymentID"
}

function DeleteDeployment()
{
    Write-Progress -Id 2 -Activity "Deleting Deployment" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Deleting Deployment: In progress"

    #WARNING - always deletes with force
    $removeDeployment = Remove-AzureDeployment -Slot $Environment -ServiceName $ServiceName -Force

    Write-Progress -Id 2 -Activity "Deleting Deployment: Complete" -Completed -Status $removeDeployment
    Write-Output "$(Get-Date -f $timeStampFormat) - Deleting Deployment: Complete"
}

function StartInstances()
{
    Write-Progress -Id 4 -Activity "Starting Instances" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instances: In progress"

    $deployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Environment
    $runstatus = $deployment.Status

    if ($runstatus -ne 'Running') 
    {
        $run = Set-AzureDeployment -Slot $Environment -ServiceName $ServiceName -Status Running
    }
	
    $deployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Environment
    $oldStatusStr = @("") * $deployment.RoleInstanceList.Count
    
    while (-not(AllInstancesRunning($deployment.RoleInstanceList)))
    {
        $i = 1
        foreach ($roleInstance in $deployment.RoleInstanceList)
        {
            $instanceName = $roleInstance.InstanceName
            $instanceStatus = $roleInstance.InstanceStatus

            if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
            {
                $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
                Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
            }

            Write-Progress -Id (4 + $i) -Activity "Starting Instance '$instanceName'" -Status "$instanceStatus"
            $i = $i + 1
        }

        Sleep -Seconds 1

        $deployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Environment
    }

    $i = 1
    foreach ($roleInstance in $deployment.RoleInstanceList)
    {
        $instanceName = $roleInstance.InstanceName
        $instanceStatus = $roleInstance.InstanceStatus

        if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
        {
            $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
            Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
        }

        $i = $i + 1
    }
    
    $deployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Environment
    $opstat = $deployment.Status 
    
    Write-Progress -Id 4 -Activity "Starting Instances" -Completed -status $opstat
    Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instances: $opstat"
}

function AllInstancesRunning($roleInstanceList)
{
    foreach ($roleInstance in $roleInstanceList)
    {
        if ($roleInstance.InstanceStatus -ne "ReadyRole")
        {
            return $false
        }
    }
    
    return $true
}

# Stop execution on first error
$ErrorActionPreference = "Stop"

Import-Module $AzureModulePath

#configure powershell with publishsettings for your subscription

# Write-Output "$(Get-Date -f $timeStampFormat) - Removing old subscriptions"
# Get-AzureSubscription | ForEach-Object { Remove-AzureSubscription $_.SubscriptionName -Force }

# Write-Output "$(Get-Date -f $timeStampFormat) - Importing Publish Settings file $SubscriptionDataFile"
# Import-AzurePublishSettingsFile $SubscriptionDataFile

Write-Output "$(Get-Date -f $timeStampFormat) - Selecting subscription $SelectedSubscription and storage $StorageAccountName"
Select-AzureSubscription $SelectedSubscription
Set-AzureSubscription $SelectedSubscription -CurrentStorageAccountName $StorageAccountName

#set remaining environment variables for Azure cmdlets
$subscription = Get-AzureSubscription $SelectedSubscription
$subscriptionname = $subscription.subscriptionname
$subscriptionid = $subscription.subscriptionid
$currentStorageAccountName = $subscription.CurrentStorageAccountName

#main driver - publish & write progress to activity log
Write-Output "$(Get-Date -f $timeStampFormat) - Azure Cloud Service deploy script started."
Write-Output "$(Get-Date -f $timeStampFormat) - Preparing deployment of $deploymentLabel for $subscriptionname with Subscription ID $subscriptionid, storage: $currentStorageAccountName"

Publish

$deployment = Get-AzureDeployment -slot $Environment -serviceName $ServiceName
$deploymentUrl = $deployment.Url

Write-Output "$(Get-Date -f $timeStampFormat) - Created Cloud Service with URL $deploymentUrl."
Write-Output "$(Get-Date -f $timeStampFormat) - Azure Cloud Service deploy script finished."
