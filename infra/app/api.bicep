param name string
param location string
//param appSettings object = {}
param appServicePlanId string
var webSiteName = toLower('app-${name}')

module api '../core/host/appservice.bicep' = {
    name: webSiteName
    params: {
        name: webSiteName
        location: location
        appServicePlanId: appServicePlanId
       // appSettings: appSettings
        netFrameworkVersion: '7.0'
    }
}

output SERVICE_API_NAME string = api.outputs.name
output SERVICE_API_URI string = api.outputs.uri