#!/usr/bin/env bash

# Using the existing variables

echo "Resource Group Name: $AZURE_RESOURCE_GROUP"
echo "App Service Name: $API_SERVICE_NAME"

# Change directory to your .NET project's folder
apppath="./app/backend"
cd "$apppath" || exit

# Publish the .NET project
publishfolder="publish"
dotnet publish "./CustomerSupportServiceSample.csproj" -o "$publishfolder"

# Zip the published files
(cd "$publishfolder" && zip -r ../app.zip .)

# Deploy the ZIP archive to the Azure Web App using Azure CLI
az webapp deployment source config-zip --resource-group $AZURE_RESOURCE_GROUP --name $API_SERVICE_NAME --src "app.zip"

# Clean up the ZIP archive
rm -f "app.zip"

# Return to the previous directory
cd ..

# Optional: Restart the Azure Web App
az webapp restart --name $API_SERVICE_NAME --resource-group $AZURE_RESOURCE_GROUP
