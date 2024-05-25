#!/usr/bin/env bash

# Using the existing variables to run this script

echo "Resource Group Name: $AZURE_RESOURCE_GROUP"
echo "App Service Name: $WEB_SERVICE_NAME"
echo "Backend Host URL: $API_ENDPOINT"

# Change directory to your clientapp's folder
apppath="./app/frontend"
cd "$apppath" || exit

# Update the frontend appsettings URL (simulating PowerShell's JSON manipulation)
file="./src/appsettings.json"
content=$(cat "$file" | jq '.baseUrl = env.API_ENDPOINT')
echo "$content" > "$file"

# Install dependencies and build the clientapp
npm install
npm run build

# Create a ZIP archive of the build folder
(cd build && zip -r ../app.zip .)

# Deploy the ZIP archive to the Azure Web App using Azure CLI
az webapp deployment source config-zip --resource-group $AZURE_RESOURCE_GROUP --name $WEB_SERVICE_NAME --src "app.zip"

# Clean up the ZIP archive
rm -f "app.zip"

# Return to the previous directory where you before run
cd ..

# Optional: Restart the Azure Web App using Azure CLI
# This restart webapp not needed evetytime when you deployed so you can comment it
az webapp restart --name $WEB_SERVICE_NAME --resource-group $AZURE_RESOURCE_GROUP

