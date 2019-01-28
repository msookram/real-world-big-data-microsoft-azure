
# Create a SQL Azure server & database and deploy specified DACPAC

[CmdletBinding(PositionalBinding=$True)]
Param
(
    [Parameter(Mandatory = $true)]    
    [String]$PublicIpAddress,  
    [Parameter(Mandatory = $true)]    
    [String]$AdminUser,
    [Parameter(Mandatory = $true)]    
    [String]$AdminPassword,
    [Parameter(Mandatory = $true)]    
    [String]$DacPacPath
)

#create the new server:
$databaseServer = New-AzureSqlDatabaseServer -AdministratorLogin $AdminUser `
                                             -AdministratorLoginPassword $AdminPassword `
                                             -Location 'North Europe'

#allow access from *this* IP so we can create the DB, and all azure services:
New-AzureSqlDatabaseServerFirewallRule -ServerName $databaseServer.ServerName `
                                       -RuleName 'local' `
                                       -StartIpAddress $PublicIpAddress –EndIpAddress $PublicIpAddress 

New-AzureSqlDatabaseServerFirewallRule -ServerName $databaseServer.ServerName `
                                       -AllowAllAzureServices -RuleName "Azure"

#and a context object to access it:
$securePassword = ConvertTo-SecureString $AdminPassword -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($AdminUser, $securePassword)  
$context = New-AzureSqlDatabaseServerContext -ServerName $databaseServer.ServerName `
                                             -Credential $credential

#now create the database:
New-AzureSqlDatabase -DatabaseName 'eventsdb-prd' -Context $context

#and publish using SqlPackage and the DACPAC:
$connectionString = "Server=tcp:$($databaseServer.ServerName).database.windows.net,1433;Database=eventsdb-prd;User ID=$AdminUser;Password=$AdminPassword;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"

& "C:\Program Files (x86)\Microsoft SQL Server\110\DAC\bin\SqlPackage.exe" `
    /SourceFile:"$DacPacPath" /TargetConnectionString:"$connectionString" /Action:Publish
