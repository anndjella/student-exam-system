@description('Azure region.')
param location string

@description('Dedicated Flex Consumption plan for the HTTP API.')
param planName string

@description('HTTP API Function App name (globally unique).')
param appName string

param tags object = {}
param storageAccountName string
param appInsightsConnectionString string
param sqlConnectionStringSecretUri string
param internalApiKeySecretUri string
param jwtKeySecretUri string
param notificationServiceBaseUrl string
param allowedCorsOrigins array
param jwtIssuer string = 'StudentExamSystem'
param jwtAudience string = 'StudentExamSystem'

@allowed([512, 2048, 4096])
param instanceMemoryMB int = 2048

@description('Scale-out ceiling, not a reserved instance count. Regional subscription quotas also apply.')
@minValue(40)
@maxValue(1000)
param maximumInstanceCount int = 40

@description('Concurrent HTTP requests per instance. All HTTP functions scale as one group.')
@minValue(1)
param httpConcurrency int = 16

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' existing = {
  parent: storage
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'deploymentpackage'
  properties: { publicAccess: 'None' }
}

// Use the same storage connection mechanism as the existing notification app.
// Keys are resolved by ARM, never committed or exposed as deployment outputs.
var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  tags: tags
  sku: { name: 'FC1', tier: 'FlexConsumption' }
  kind: 'functionapp'
  properties: { reserved: true }
}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storage.properties.primaryEndpoints.blob}${deploymentContainer.name}'
          authentication: {
            type: 'StorageAccountConnectionString'
            storageAccountConnectionStringName: 'DEPLOYMENT_STORAGE_CONNECTION_STRING'
          }
        }
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
      scaleAndConcurrency: {
        instanceMemoryMB: instanceMemoryMB
        maximumInstanceCount: maximumInstanceCount
        triggers: {
          http: { perInstanceConcurrency: httpConcurrency }
        }
        // No always-ready instances: allow scale to zero.
        alwaysReady: []
      }
    }
    siteConfig: {
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: allowedCorsOrigins
        supportCredentials: true
      }
      appSettings: [
        { name: 'AzureWebJobsStorage', value: storageConnectionString }
        { name: 'DEPLOYMENT_STORAGE_CONNECTION_STRING', value: storageConnectionString }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
        { name: 'AZURE_FUNCTIONS_ENVIRONMENT', value: 'Production' }
        { name: 'ConnectionStrings__DefaultConnection', value: '@Microsoft.KeyVault(SecretUri=${sqlConnectionStringSecretUri})' }
        { name: 'Jwt__Key', value: '@Microsoft.KeyVault(SecretUri=${jwtKeySecretUri})' }
        { name: 'Jwt__Issuer', value: jwtIssuer }
        { name: 'Jwt__Audience', value: jwtAudience }
        { name: 'Services__NotificationService', value: notificationServiceBaseUrl }
        { name: 'ServiceAuthentication__ApiKey', value: '@Microsoft.KeyVault(SecretUri=${internalApiKeySecretUri})' }
        { name: 'InternalHttp__TimeoutSeconds', value: '10' }
        { name: 'InternalHttp__RetryCount', value: '2' }
        { name: 'InternalHttp__RetryBaseDelayMilliseconds', value: '200' }
      ]
    }
  }
}

output principalId string = app.identity.principalId
output defaultHostName string = 'https://${app.properties.defaultHostName}'
output name string = app.name
