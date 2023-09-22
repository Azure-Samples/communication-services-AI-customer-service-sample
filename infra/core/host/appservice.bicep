metadata description = 'Creates an Azure App Service in an existing Azure App Service plan.'
param name string
param location string = resourceGroup().location
param tags object = {}

// Reference Properties
param appServicePlanId string

param netFrameworkVersion string = 'v7.0'
param nodeVersion string = '18-lts'

//@secure()
//param appSettings object = {}


resource appService 'Microsoft.Web/sites@2020-06-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
        siteConfig: {
            netFrameworkVersion: netFrameworkVersion
            nodeVersion: nodeVersion
      
    }
  }
}

//module config 'appservice-appsettings.bicep' = if (!empty(appSettings)) {
//  name: '${name}-appSettings'
//  params: {
//    name: appService.name
//    appSettings: appSettings
//  }
//}

output name string = appService.name
output uri string = 'https://${appService.properties.defaultHostName}'
