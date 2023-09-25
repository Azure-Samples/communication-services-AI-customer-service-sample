param(
[string] $resourceGroupName,
[string] $appServiceName
)

$appPath = ".\app"

$publishFolder ="publish" # Replace with the name of the publish folder

# Change directory to your .NET project's folder
Push-Location $appPath\backend

# Publish the .NET project
dotnet publish ".\CustomerSupportServiceSample.csproj" -o $publishFolder

# Zip the published files
Compress-Archive -Path ".\publish\*" -DestinationPath "app.zip" -Force


# Deploy the ZIP archive to the Azure Web App
az webapp deployment source config-zip --resource-group $resourceGroupName --name $appServiceName --src "app.zip" --verbose
Write-Output "Backend deployment completed"

# Clean up the ZIP archive
Remove-Item "app.zip" -Force

Pop-Location

# Optional: Restart the Azure Web App to apply changes
az webapp restart --name $appServiceName --resource-group $resourceGroupName --verbose