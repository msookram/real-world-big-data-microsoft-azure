#build secure credential for RDP access
$securePassword = ConvertTo-SecureString 'P!ural5ighT' -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential('admin',$securePassword)  

New-AzureHDInsightCluster `
  -Name 'telemetryprdstorm' `
  -Credential $credential `
  -Location 'North Europe' `
  -DefaultStorageAccountName 'devicetelemetryprd.blob.core.windows.net' `
  -DefaultStorageAccountKey 'LA7fhZFfRpS5FYkwDUPwIm7RRLHUF2Q0StkVfBMRYNnA5qZn3FMnksL8SOgKD3AaSFPHP/fZEC8WdDTTXPaYTg==' `
  -DefaultStorageContainerName 'telemetryprdstorm' `
  -ClusterSizeInNodes 1 `
  -ClusterType Storm