@description('Azure region.')
param location string

@description('Flex Consumption plan name.')
param planName string

@description('Function App name (globally unique).')
param appName string

param tags object = {}

param storageAccountName string
param appInsightsConnectionString string
param sqlConnectionStringSecretUri string
param studentExamApiBaseUrl string
param internalApiKeySecretUri string
param serviceBusFullyQualifiedNamespace string
param notificationQueueName string
param communicationConnectionStringSecretUri string
param communicationSenderAddress string
param reminderTimeZone string = 'Europe/Belgrade'

@description('Instance memory (MB) for Flex Consumption. Matches the current 2048.')
param instanceMemoryMB int = 2048

@description('Max scale-out instance count. Kept low to bound cost if something retries in a loop.')
param maximumInstanceCount int = 10

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
        version: '8.0'
      }
      scaleAndConcurrency: {
        instanceMemoryMB: instanceMemoryMB
        maximumInstanceCount: maximumInstanceCount
      }
    }
    siteConfig: {
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        { name: 'AzureWebJobsStorage', value: storageConnectionString }
        { name: 'DEPLOYMENT_STORAGE_CONNECTION_STRING', value: storageConnectionString }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
        { name: 'AZURE_FUNCTIONS_ENVIRONMENT', value: 'Production' }
        { name: 'WEBSITE_TIME_ZONE', value: reminderTimeZone }

        { name: 'SqlConnectionString', value: '@Microsoft.KeyVault(SecretUri=${sqlConnectionStringSecretUri})' }
        { name: 'StudentExamSystemBaseUrl', value: studentExamApiBaseUrl }
        { name: 'InternalApiKey', value: '@Microsoft.KeyVault(SecretUri=${internalApiKeySecretUri})' }
        { name: 'InternalHttpTimeoutSeconds', value: '10' }
        { name: 'InternalHttpRetryCount', value: '2' }
        { name: 'InternalHttpRetryBaseDelayMilliseconds', value: '200' }
        { name: 'AllowUntrustedDevelopmentCertificate', value: 'false' }

        { name: 'ServiceBusConnection__fullyQualifiedNamespace', value: serviceBusFullyQualifiedNamespace }
        { name: 'ServiceBusConnection__credential', value: 'managedidentity' }
        { name: 'NotificationQueueName', value: notificationQueueName }

        { name: 'EmailProvider', value: 'AzureCommunicationServices' }
        {
          name: 'AzureCommunicationEmailConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${communicationConnectionStringSecretUri})'
        }
        { name: 'AzureCommunicationEmailSenderAddress', value: communicationSenderAddress }

        { name: 'ReminderTimeZone', value: reminderTimeZone }
        { name: 'MissingExamResultReminderDays', value: '30' }
        { name: 'RegistrationReminderSchedule', value: '0 2 8 * * *' }
        { name: 'MissingExamResultReminderSchedule', value: '0 4 8 * * *' }
        { name: 'EmailRetrySchedule', value: '0 0 8 * * *' }
        { name: 'EmailMaxDeliveryAttempts', value: '3' }
        { name: 'EmailRetryBatchSize', value: '50' }
        { name: 'SmtpEnabled', value: 'false' }
      ]
    }
  }
}

output principalId string = app.identity.principalId
output defaultHostName string = 'https://${app.properties.defaultHostName}'
output name string = app.name
