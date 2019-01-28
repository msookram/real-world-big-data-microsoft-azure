
# Create an Azure Web App & deploy specified package

$AppName = 'telemetry-dashboard'
$WebDeployPackagePath = 'deployment-packages\Telemetry.RealTime.Dashboard.zip'

$WebApp = Get-AzureWebsite -Name $AppName

if ($WebApp)
{
    Write-Output "Web App: $AppName already exists" 
}
else
{
    New-AzureWebsite -Name $AppName -Location 'North Europe'
    Write-Output ("Web App: $AppName created")
}

Publish-AzureWebsiteProject -Name $AppName -Package $WebDeployPackagePath
Write-Output ("Web App package: $WebDeployPackagePath deployed")