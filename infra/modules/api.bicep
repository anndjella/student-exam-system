@description('Azure region.')
param location string

@description('App Service plan name.')
param planName string

@description('Web App name (globally unique).')
param appName string

param tags object = {}

@description('App Service plan SKU. F1 = Free (matches current). B1/P0v3/P1v3 to scale up.')
param planSku string = 'F1'

param appInsightsConnectionString string
param sqlConnectionStringSecretUri string
param notificationServiceBaseUrl string
param allowedCorsOrigins array
param internalApiKeySecretUri string
param jwtKeySecretUri string
param jwtIssuer string = 'StudentExamSystem'
param jwtAudience string = 'StudentExamSystem'

var isFree = planSku == 'F1' || planSku == 'D1'

var fixedSettings = [
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
  { name: 'ApplicationInsightsAgent_EXTENSION_VERSION', value: '~2' }
  { name: 'XDT_MicrosoftApplicationInsights_Mode', value: 'default' }
  { name: 'ConnectionStrings__DefaultConnection', value: '@Microsoft.KeyVault(SecretUri=${sqlConnectionStringSecretUri})' }
  { name: 'Services__NotificationService', value: notificationServiceBaseUrl }
  { name: 'ServiceAuthentication__ApiKey', value: '@Microsoft.KeyVault(SecretUri=${internalApiKeySecretUri})' }
  { name: 'Jwt__Key', value: '@Microsoft.KeyVault(SecretUri=${jwtKeySecretUri})' }
  { name: 'Jwt__Issuer', value: jwtIssuer }
  { name: 'Jwt__Audience', value: jwtAudience }
  { name: 'InternalHttp__TimeoutSeconds', value: '10' }
  { name: 'InternalHttp__RetryCount', value: '2' }
  { name: 'InternalHttp__RetryBaseDelayMilliseconds', value: '200' }
]

var corsSettings = [for (origin, i) in allowedCorsOrigins: {
  name: 'Cors__AllowedOrigins__${i}'
  value: origin
}]

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  tags: tags
  sku: { name: planSku }
  kind: 'app'
  properties: {}
}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  tags: tags
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      alwaysOn: !isFree
      healthCheckPath: isFree ? null : '/health/ready'
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      appSettings: concat(fixedSettings, corsSettings)
    }
  }
}

output principalId string = app.identity.principalId
output defaultHostName string = 'https://${app.properties.defaultHostName}'
output name string = app.name
output planId string = plan.id
