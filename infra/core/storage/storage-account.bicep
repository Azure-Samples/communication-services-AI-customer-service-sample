param name string
param location string = resourceGroup().location
param tags object = {}
param containerName string ='solar'

@allowed([
    'Cool'
    'Hot'
    'Premium' ])
param accessTier string = 'Hot'
param allowBlobPublicAccess bool = true
param allowCrossTenantReplication bool = true
param allowSharedKeyAccess bool = true
param defaultToOAuthAuthentication bool = false
@allowed([ 'AzureDnsZone', 'Standard' ])
param dnsEndpointType string = 'Standard'
param kind string = 'StorageV2'
param minimumTlsVersion string = 'TLS1_2'
param networkAcls object = {
    bypass: 'AzureServices'
    defaultAction: 'Allow'
}
@allowed([ 'Enabled', 'Disabled' ])
param publicNetworkAccess string = 'Enabled'
param sku object = { name: 'Standard_LRS' }

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
    name: name
    location: location
    tags: tags
    kind: kind
    sku: sku
    properties: {
        accessTier: accessTier
        allowBlobPublicAccess: allowBlobPublicAccess
        allowCrossTenantReplication: allowCrossTenantReplication
        allowSharedKeyAccess: allowSharedKeyAccess
        defaultToOAuthAuthentication: defaultToOAuthAuthentication
        dnsEndpointType: dnsEndpointType
        minimumTlsVersion: minimumTlsVersion
        networkAcls: networkAcls
        publicNetworkAccess: publicNetworkAccess
    }
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-05-01' = {
#disable-next-line use-parent-property
    name: '${storageAccount.name}/default/${containerName}'
}

// Determine our connection string

var connectionString =  'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'

// Output our variable

output blobStorageConnectionString string = connectionString
output blobContainerName string = container.name