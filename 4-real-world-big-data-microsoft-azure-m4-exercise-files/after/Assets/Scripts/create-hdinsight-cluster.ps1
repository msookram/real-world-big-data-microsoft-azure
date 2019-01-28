#build secure credential for RDP access
$securePassword = ConvertTo-SecureString 'P!ural5ighT' -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential('admin',$securePassword)  

New-AzureHDInsightCluster `
  -Name 'devicetelemetryprd2' `
  -Credential $credential `
  -Location 'North Europe' `
  -DefaultStorageAccountName 'devicetelemetryprd.blob.core.windows.net' `
  -DefaultStorageAccountKey 'LA7fhZFfRpS5FYkwDUPwIm7RRLHUF2Q0StkVfBMRYNnA5qZn3FMnksL8SOgKD3AaSFPHP/fZEC8WdDTTXPaYTg==' `
  -DefaultStorageContainerName 'devicetelemetryprd2' `
  -ClusterSizeInNodes 4 `
  -ClusterType Hadoop