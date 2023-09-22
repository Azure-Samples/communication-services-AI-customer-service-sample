﻿ 
 
 try {
    # Check if the user is logged in
    az account show > $null 2>&1

    Write-Output "Already logged in."
} catch {
    # If not logged in, this block will be executed
    az login
    Write-Output "Logged in successfully."
}

$subscriptionName = Read-Host "Please enter the subscription name"
$location = Read-Host "Please enter the region for azure resources creation"
$environmentName = Read-Host "Please enter the environment name"

az account set --subscription $subscriptionName
$subscriptionId = (az account show | ConvertFrom-Json).id

Write-Output "Deploying to subscription id $subscriptionId"

$mainBicepPath = ".\infra"
$scriptsPath =".\scripts"
$appPath =".\app"

# deploy resources
$result = az deployment sub create --location $location  --name bicepDeployment --template-file $mainBicepPath\main.bicep --parameters $mainBicepPath\main.parameters.json `
--parameters environmentName=$environmentName location=$location  | ConvertFrom-Json 

$outputs = $result.properties.outputs

Write-Output "Created resources to resourcegroup $($outputs.AZURE_RESOURCE_GROUP.Value)"

[Environment]::SetEnvironmentVariable('AZURE_STORAGE_CONTAINER', $outputs.AZURE_STORAGE_CONTAINER.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('AZURE_STORAGE_ACCOUNT_CONNECTIONSTRING', $outputs.AZURE_STORAGE_ACCOUNT_CONNECTIONSTRING.Value, [System.EnvironmentVariableTarget]::User)

[Environment]::SetEnvironmentVariable('AZURE_SEARCH_SERVICE_KEY', $outputs.AZURE_SEARCH_SERVICE_KEY.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('AZURE_SEARCH_INDEX', $outputs.AZURE_SEARCH_INDEX.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('AZURE_SEARCH_SERVICE_ENDPOINT', $outputs.AZURE_SEARCH_SERVICE_ENDPOINT.Value, [System.EnvironmentVariableTarget]::User)

[Environment]::SetEnvironmentVariable('AZURE_AI_SERVICE_ENDPOINT', $outputs.AZURE_AI_SERVICE_ENDPOINT.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('AZURE_AI_SERVICE_KEY', $outputs.AZURE_AI_SERVICE_KEY.Value, [System.EnvironmentVariableTarget]::User)


[Environment]::SetEnvironmentVariable('ACS_CONNECTIONSTRING', $outputs.ACS_CONNECTIONSTRING.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('ACS_ENDPOINT', $outputs.ACS_ENDPOINT.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('ACS_SERVICE_NAME', $outputs.ACS_SERVICE_NAME.Value, [System.EnvironmentVariableTarget]::User)

[Environment]::SetEnvironmentVariable('API_ENDPOINT', $outputs.API_ENDPOINT.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('API_SERVICE_NAME', $outputs.API_SERVICE_NAME.Value, [System.EnvironmentVariableTarget]::User)

[Environment]::SetEnvironmentVariable('WEB_ENDPOINT', $outputs.WEB_ENDPOINT.Value, [System.EnvironmentVariableTarget]::User)
[Environment]::SetEnvironmentVariable('WEB_SERVICE_NAME', $outputs.WEB_SERVICE_NAME.Value, [System.EnvironmentVariableTarget]::User)

[Environment]::SetEnvironmentVariable('AZURE_RESOURCE_GROUP', $outputs.AZURE_RESOURCE_GROUP.Value, [System.EnvironmentVariableTarget]::User)


#Run predocs
./scripts/prepdocs.ps1 -AZURE_STORAGE_BLOB_CONNSTRING $outputs.AZURE_STORAGE_ACCOUNT_CONNECTIONSTRING.Value`
-AZURE_STORAGE_BLOB_CONTAINER $outputs.AZURE_STORAGE_CONTAINER.Value`
-AZURE_SEARCH_SERVICE_ENDPOINT $outputs.AZURE_SEARCH_SERVICE_ENDPOINT.Value`
-AZURE_SEARCH_SERVICE_ENDPOINT_KEY $outputs.AZURE_SEARCH_SERVICE_KEY.Value`
-AZURE_SEARCH_INDEX $outputs.AZURE_SEARCH_INDEX.Value`
-AZURE_AI_SERVICE_ENDPOINT $outputs.AZURE_AI_SERVICE_ENDPOINT.Value`
-AZURE_AI_SERVICE_KEY $outputs.AZURE_AI_SERVICE_KEY.Value


$resourceGroupName =$outputs.AZURE_RESOURCE_GROUP.Value
$webAppServiceName = $outputs.WEB_SERVICE_NAME.Value
$apiAppServiceName = $outputs.API_SERVICE_NAME.Value
$acsServiceName = $outputs.ACS_SERVICE_NAME.Value
$backendHostUrl = $outputs.API_ENDPOINT.Value

<#----------deploy frontend application to azure appservice------------#>
.\scripts\deploy-frontend.ps1 -resourceGroupName $resourceGroupName  -appServiceName $webAppServiceName -backendHostUrl $backendHostUrl


<#----------deploy backend application to azure appservice------------#>
.\scripts\deploy-backend.ps1 -resourceGroupName $resourceGroupName -appServiceName $apiAppServiceName


<#----------create event subscription------------#>
.\scripts\event-subscription-creation.ps1 -resourceGroupName $resourceGroupName -subscriptionId $subscriptionId -communicationServiceName $acsServiceName -hostUrl $backendHostUrl


Write-Output "You can view the resources created under the resource group $($outputs.AZURE_RESOURCE_GROUP.Value) in Azure Portal: https:/portal.azure.com/#@/resource/subscriptions/$subscriptionId/resourceGroups/$($outputs.AZURE_RESOURCE_GROUP.Value)/overview"


Write-Output "Backend endpoint: $($outputs.API_ENDPOINT.Value)/swagger"
Write-Output "Frontend endpoint: $($outputs.WEB_ENDPOINT.Value)"