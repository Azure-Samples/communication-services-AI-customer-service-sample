<#--------- Update backend appsettings----------- #>

# Path to the JSON file
$file = ".\app\backend\appsettings.json"

# Read the JSON content from the file and convert to a PowerShell object
$content = Get-Content -Path $file -Raw | ConvertFrom-Json

$content.AcsConnectionString = [System.Environment]::GetEnvironmentVariable("ACS_CONNECTIONSTRING", "User")
$content.AcsEndpoint =[System.Environment]::GetEnvironmentVariable("ACS_ENDPOINT", "User")

$content.AzureSearchEndpoint = [System.Environment]::GetEnvironmentVariable("AZURE_SEARCH_SERVICE_ENDPOINT", "User")
$content.AzureSearchKey =[System.Environment]::GetEnvironmentVariable("AZURE_SEARCH_SERVICE_KEY", "User")
$content.AzureSearchIndexName = [System.Environment]::GetEnvironmentVariable("AZURE_SEARCH_INDEX", "User")
$content.AzureAIServiceEndpoint = [System.Environment]::GetEnvironmentVariable("AZURE_AI_SERVICE_ENDPOINT", "User")
$content.AzureOpenAIEndpoint = [System.Environment]::GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", "User")
$content.AzureOpenAIKey = [System.Environment]::GetEnvironmentVariable("AZURE_OPENAI_KEY", "User")
$content.AzureOpenAIDeploymentName = [System.Environment]::GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME", "User")

# Convert the updated object back to JSON format and overwrite the original file
$content | ConvertTo-Json -Depth 5 | Set-Content -Path $file

Write-Output "AppSettings file has been updated."
