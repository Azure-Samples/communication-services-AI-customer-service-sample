targetScope = 'subscription'

@description('Name of the environment used to generate a short unique hash for resources.')
@minLength(1)
@maxLength(64)
param environmentName string

@description('Primary location for all resources')
param location string

@description('Name of the resource group')
param resourceGroupName string = ''

/* Azure communication resource details */
@description('Name of the Azure Communication service')
param acsServiceName string = ''

@description('Name of the resource group for the Azure Communication service')
param acsServiceResourceGroupName string = ''

/* Azure AI Multi service resource details */

@description('Location of the resource group for the AI Multi Service')
param aiServiceResourceGroupLocation string = location

@description('Name of the resource group for the AI Multi Service')
param aiServiceResourceGroupName string = ''

@description('Name of the AI Multi Service')
param aiServiceName string = ''

@description('SKU name for the AI Multi Service. Default: S0')
param aiServiceSkuName string = 'S0'

/* Azure search service resource details */

@description('Name of the search index. Default: solar-index')
param searchIndexName string = 'solar-index'

@description('Name of the Azure Cognitive Search service')
param searchServiceName string = ''

@description('Location of the resource group for the Azure Cognitive Search service')
param searchServiceResourceGroupLocation string = location

@description('Name of the resource group for the Azure Cognitive Search service')
param searchServiceResourceGroupName string = ''

@description('SKU name for the Azure Cognitive Search service. Default: standard')
param searchServiceSkuName string = 'standard'

/* Azure storage service resource details */

@description('Name of the storage account')
param storageAccountName string = ''

@description('Name of the storage container. Default: content')
param storageContainerName string = 'content'

@description('Location of the resource group for the storage account')
param storageResourceGroupLocation string = location

@description('Name of the resource group for the storage account')
param storageResourceGroupName string = ''

/* Azure openai service resource details */

@description('Capacity of the chat GPT deployment. Default: 30')
param chatGptDeploymentCapacity int = 30

@description('Name of the chat GPT deployment')
param chatGptDeploymentName string = 'chatgpt-deployment'

@description('Name of the chat GPT model. Default: gpt-35-turbo')
param chatGptModelName string = 'gpt-35-turbo'

@description('Location of the resource group for the OpenAI resources')
param openAiResourceGroupLocation string = location

@description('Name of the resource group for the OpenAI resources')
param openAiResourceGroupName string = ''

@description('Name of the OpenAI service')
param openAiServiceName string = ''

@description('SKU name for the OpenAI service. Default: S0')
param openAiSkuName string = 'S0'

/* Azure app service resource details */

@description('Name of the App service plan ')
param appServicePlanName string = ''

@description('Name of the frontend service ')
param frontendServiceName string = ''

@description('Name of the backend service ')
param backendServiceName string = ''

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

// Organize resources in a resource group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
    name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
    location: location
}

resource communicationServiceResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(acsServiceResourceGroupName)) {
    name: !empty(acsServiceResourceGroupName) ? acsServiceResourceGroupName : resourceGroup.name
}

resource cognitiveServiceResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(aiServiceResourceGroupName)) {
    name: !empty(aiServiceResourceGroupName) ? aiServiceResourceGroupName : resourceGroup.name
}

resource searchServiceResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(searchServiceResourceGroupName)) {
    name: !empty(searchServiceResourceGroupName) ? searchServiceResourceGroupName : resourceGroup.name
}

resource storageResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(storageResourceGroupName)) {
    name: !empty(storageResourceGroupName) ? storageResourceGroupName : resourceGroup.name
}

resource openAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(openAiResourceGroupName)) {
    name: !empty(openAiResourceGroupName) ? openAiResourceGroupName : resourceGroup.name
}

module communicationServive 'core/communicationservice/communication-services.bicep' = {
    name: 'communicationservice'
    scope: communicationServiceResourceGroup
    params: {
        communicationServiceName: !empty(acsServiceName) ? acsServiceName : '${abbrs.communicationServiceAccounts}${resourceToken}'
    }
}

module cognitiveMultiService 'core/ai/cognitiveservices.bicep' = {
    name: 'cognitive-service'
    scope: cognitiveServiceResourceGroup
    params: {
        name: !empty(aiServiceName) ? aiServiceName : '${abbrs.cognitiveMultiServiceAccount}${resourceToken}'
        kind: 'CognitiveServices'
        location: aiServiceResourceGroupLocation
        sku: {
            name: aiServiceSkuName
        }
    }
}

module searchService 'core/search/search-services.bicep' = {
    name: 'search-service'
    scope: searchServiceResourceGroup
    params: {
        name: !empty(searchServiceName) ? searchServiceName : 'gptkb-${resourceToken}'
        location: searchServiceResourceGroupLocation
        authOptions: {
            aadOrApiKey: {
                aadAuthFailureMode: 'http401WithBearerChallenge'
            }
        }
        sku: {
            name: searchServiceSkuName
        }
        semanticSearch: 'free'
    }
}

module storage 'core/storage/storage-account.bicep' = {
    name: 'storage'
    scope: storageResourceGroup
    params: {
        name: !empty(storageAccountName) ? storageAccountName : '${abbrs.storageStorageAccounts}${resourceToken}'
        location: storageResourceGroupLocation
        publicNetworkAccess: 'Enabled'
        sku: {
            name: 'Standard_ZRS'
        }
    }
}

module openAi 'core/ai/cognitiveservices.bicep' = {
   name: 'openai'
   scope: openAiResourceGroup
    params: {
        name: !empty(openAiServiceName) ? openAiServiceName : '${abbrs.cognitiveServicesAccounts}${resourceToken}'
        location: openAiResourceGroupLocation
        sku: {
            name: openAiSkuName
        }
        deployments: [
            {
                name: chatGptDeploymentName
                model: {
                    format: 'OpenAI'
                    name: chatGptModelName
                    version: '0613'
                }
                sku: {
                    name: 'Standard'
                    capacity: chatGptDeploymentCapacity
                }
            }
        ]
    }
}

// Create an App Service Plan to group applications under the same payment plan and SKU
module appServicePlan 'core/host/appserviceplan.bicep' = {
    name: 'appserviceplan'
    scope: resourceGroup
    params: {
        name: !empty(appServicePlanName) ? appServicePlanName : '${abbrs.webServerFarms}${resourceToken}'
        location: location
        sku: {
            name: 'F1'
            capacity: 1
        }
    }
}

// The application frontend
module frontend 'app/web.bicep' = {
    name: 'web'
    scope: resourceGroup
    params: {
        name: !empty(frontendServiceName) ? frontendServiceName : '${abbrs.webSitesAppService}web-${resourceToken}'
        location: location
        appServicePlanId: appServicePlan.outputs.id
        //appSettings: {
        //    HostUrl: backend.outputs.SERVICE_API_URI
        //}
    }
}

// The application backend
module backend 'app/api.bicep' = {
    name: 'api'
    scope: resourceGroup
    params: {
        name: !empty(backendServiceName) ? backendServiceName : '${abbrs.webSitesAppService}api-${resourceToken}'
        location: location
        appServicePlanId: appServicePlan.outputs.id
        //allowedOrigins: [ web.outputs.SERVICE_WEB_URI ]
        //appSettings: {

        //}
    }
}

module backendAppSettings 'core/host/appservice-appsettings.bicep' = {
    scope: resourceGroup
    name: 'backend-appsettings'
    params: {
        appSettings: {
            AcsConnectionString: communicationServive.outputs.primaryConnectionString
            AcsEndpoint: communicationServive.outputs.endpoint
            AcsPhoneNumber: ''
            EmailSender: ''
            EmailRecipient: ''
            HostUrl: backend.outputs.SERVICE_API_URI
            AzureSearchEndpoint: searchService.outputs.endpoint
            AzureSearchKey: searchService.outputs.key
            AzureSearchIndexName: searchIndexName
            AzureAIServiceEndpoint: cognitiveMultiService.outputs.endpoint
            AzureOpenAIEndpoint: openAi.outputs.endpoint
            AzureOpenAIKey: openAi.outputs.key
            AzureOpenAIDeploymentName: chatGptDeploymentName
        }
        name: backend.outputs.SERVICE_API_NAME
    }
}

module frontendAppSettings 'core/host/appservice-appsettings.bicep' = {
    scope: resourceGroup
    name: 'frontend-appSettings'
    params: {
        appSettings: {
            baseUrl: backend.outputs.SERVICE_API_URI
        }
        name: frontend.outputs.SERVICE_WEB_NAME
    }
}

output AZURE_LOCATION string = location
output AZURE_OPENAI_DEPLOYMENT_NAME string = chatGptDeploymentName
output AZURE_OPENAI_ENDPOINT string = openAi.outputs.endpoint
output AZURE_OPENAI_KEY string = openAi.outputs.key

output AZURE_OPENAI_SERVICE string = openAi.outputs.name
output AZURE_RESOURCE_GROUP string = resourceGroup.name

output AZURE_SEARCH_INDEX string = searchIndexName
output AZURE_SEARCH_SERVICE_ENDPOINT string = searchService.outputs.endpoint
output AZURE_SEARCH_SERVICE_KEY string = searchService.outputs.key

output AZURE_STORAGE_ACCOUNT_CONNECTIONSTRING string = storage.outputs.blobStorageConnectionString
output AZURE_STORAGE_CONTAINER string = storageContainerName

output AZURE_AI_SERVICE_ENDPOINT string = cognitiveMultiService.outputs.endpoint
output AZURE_AI_SERVICE_KEY string = cognitiveMultiService.outputs.key

output ACS_CONNECTIONSTRING string = communicationServive.outputs.primaryConnectionString
output ACS_ENDPOINT string = communicationServive.outputs.endpoint
output ACS_SERVICE_NAME string = communicationServive.outputs.communicationServiceName

output API_ENDPOINT string = backend.outputs.SERVICE_API_URI
output API_SERVICE_NAME string = backend.outputs.SERVICE_API_NAME

output WEB_ENDPOINT string = frontend.outputs.SERVICE_WEB_URI
output WEB_SERVICE_NAME string = frontend.outputs.SERVICE_WEB_NAME
