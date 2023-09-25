param name string
param location string
//param appSettings object = {}
var webSiteName = toLower('wapp-${name}')
param appServicePlanId string

module web '../core/host/appservice.bicep' = {
    name: webSiteName
    params: {
        name: webSiteName
        location: location
        appServicePlanId: appServicePlanId
       // appSettings: appSettings
        nodeVersion: '18-lts'
    }
}

output SERVICE_WEB_NAME string = web.outputs.name
output SERVICE_WEB_URI string = web.outputs.uri
