
# Create a Service Bus namespace, Event Hub & access policy
# Adapted from http://blogs.msdn.com/b/paolos/archive/2014/12/01/how-to-create-a-service-bus-namespace-and-an-event-hub-using-a-powershell-script.aspx

[CmdletBinding(PositionalBinding=$True)]
Param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern("^[a-z0-9\-]*$")]
    [String]$Path,                                  # required    needs to be alphanumeric (dashes allowed)    
    [Int]$PartitionCount = 16,                      # optional    default to 16
    [Int]$MessageRetentionInDays = 7,               # optional    default to 7
    [Parameter(Mandatory = $true)]
    [ValidatePattern("^[a-z0-9]*$")]
    [String]$Namespace,                             # required    needs to be alphanumeric
    [String]$Location = "North Europe"              # optional    default to "North Europe"
    )

# Set the output level to verbose and make the script stop on error
$VerbosePreference = "Continue"
$ErrorActionPreference = "Stop"

# Load the ServiceBus assembly, relative to the script - be sure to
# build the solution first to run the NuGet package restore
$ThisPath = Split-Path -parent $MyInvocation.MyCommand.Definition
Add-Type -Path "$ThisPath\..\..\packages\WindowsAzure.ServiceBus.2.6.1\lib\net40-full\Microsoft.ServiceBus.dll"
Write-Output "The [Microsoft.ServiceBus.dll] assembly has been successfully added to the script."

# Mark the start time of the script execution
$startTime = Get-Date

# Create Azure Service Bus namespace
$CurrentNamespace = Get-AzureSBNamespace -Name $Namespace

# Check if the namespace already exists or needs to be created
if ($CurrentNamespace)
{
    Write-Output "The namespace [$Namespace] already exists in the [$($CurrentNamespace.Region)] region." 
}
else
{
    Write-Host "The [$Namespace] namespace does not exist."
    Write-Output "Creating the [$Namespace] namespace in the [$Location] region..."
    New-AzureSBNamespace -Name $Namespace -Location $Location -CreateACSNamespace $False -NamespaceType Messaging
    $CurrentNamespace = Get-AzureSBNamespace -Name $Namespace
    Write-Host "The [$Namespace] namespace in the [$Location] region has been successfully created."
}

# Create the NamespaceManager object to create the event hub
Write-Host "Creating a NamespaceManager object for the [$Namespace] namespace..."
$NamespaceManager = [Microsoft.ServiceBus.NamespaceManager]::CreateFromConnectionString($CurrentNamespace.ConnectionString);
Write-Host "Connected to [$Namespace]; connection string: [$($CurrentNamespace.ConnectionString)]"

# Check if the event hub already exists
if ($NamespaceManager.EventHubExists($Path))
{
    Write-Output "The [$Path] event hub already exists in the [$Namespace] namespace." 
    $EventHubDescription = $NamespaceManager.GetEventHub($Path)
}
else
{
    Write-Output "Creating the [$Path] event hub in the [$Namespace] namespace: PartitionCount=[$PartitionCount] MessageRetentionInDays=[$MessageRetentionInDays]..."
    $EventHubDescription = New-Object -TypeName Microsoft.ServiceBus.Messaging.EventHubDescription -ArgumentList $Path
    $EventHubDescription.PartitionCount = $PartitionCount
    $EventHubDescription.MessageRetentionInDays = $MessageRetentionInDays    
    $NamespaceManager.CreateEventHub($EventHubDescription);
    Write-Host "The [$Path] event hub in the [$Namespace] namespace has been successfully created."
}


# Mark the finish time of the script execution
$finishTime = Get-Date

# Output the time consumed in seconds
$TotalTime = ($finishTime - $startTime).TotalSeconds
Write-Output "The script completed in $TotalTime seconds."
