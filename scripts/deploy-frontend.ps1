param(
[string] $resourceGroupName,
[string] $appServiceName,
[string] $backendHostUrl
)

$appPath = ".\app"

# Change directory to your clientapp's folder
Push-Location $appPath\frontend


<# Update the frontend appsettings url #>
# Path to the JSON file
$file = ".\src\appsettings.json"

# Read the JSON content from the file and convert to a PowerShell object
$content = Get-Content -Path $file -Raw | ConvertFrom-Json

# Update the specific values in the object
#$content.HostUrl = $devTunnelUrl
$content.baseUrl = $backendHostUrl
# Convert the updated object back to JSON format and overwrite the original file
$content | ConvertTo-Json -Depth 5 | Set-Content -Path $file

# Install dependencies and build the clientapp
npm install
npm run build

# Create a ZIP archive of the build folder
Compress-Archive -Path ".\build\*" -DestinationPath "app.zip" -Force


# Deploy the ZIP archive to the Azure Web App
az webapp deployment source config-zip --resource-group $resourceGroupName --name $appServiceName --src "app.zip" --v
 Write-Output "Fronend deployment completed"

# Clean up the ZIP archive
Remove-Item "app.zip" -Force

Pop-Location

# Optional: Restart the Azure Web App to apply changes
az webapp restart --name $appServiceName --resource-group $resourceGroupName --v