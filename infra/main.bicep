targetScope = 'subscription'

@description('Azure region for all regional resources.')
param location string = 'swedencentral'

@description('Resource group name.')
param resourceGroupName string = 'rg-studentexam-swc'

@description('Short deterministic suffix appended to globally-unique resource names.')
param uniqueSuffix string = toLower(take(uniqueString(subscription().id, 'studentexam-swc'), 5))

@description('App Service plan SKU for the API. F1 = Free.')
param apiPlanSku string = 'F1'

@description('SQL authentication administrator login.')
param sqlAdminLogin string = 'studentexamadmin'

@description('SQL authentication administrator password (>= 12 chars, complex).')
@secure()
param sqlAdminPassword string

@description('Optional Entra ID admin object id for SQL (a group). Empty = SQL auth only.')
param sqlAadAdminObjectId string = ''

@description('Optional Entra ID admin display name for SQL.')
param sqlAadAdminLogin string = ''

@description('Extra CORS origins in addition to the Static Web App.')
param additionalCorsOrigins array = []

@description('Email address for Azure Monitor alerts (e.g. dead-letter queue). Empty = alert rule only.')
param alertEmail string = ''

@description('Spend (this billing currency) at which warning emails start. NOT a charge or a cap - just notifications. 0 = no budget.')
param monthlyBudgetAmount int = 10

@description('Budget start date (first of a month). Defaults to the current month.')
param budgetStartDate string = utcNow('yyyy-MM-01')

@description('Shared internal API key between the API and the Notification Service.')
@secure()
param internalApiKey string

@description('Symmetric signing key for API JWT tokens (>= 32 chars).')
@secure()
param jwtSigningKey string

param tags object = {
  project: 'student-exam-system'
  managedBy: 'bicep'
}

// ---- names --------------------------------------------------------------------------
var names = {
  logAnalytics: 'log-studentexam-swc'
  apiInsights: 'appi-studentexam-swc'
  notificationInsights: 'appi-notification-swc'
  apiPlan: 'plan-studentexam-swc'
  apiApp: 'app-studentexam-api-swc'
  functionPlan: 'plan-notification-swc'
  functionApp: 'func-notification-swc'
  sqlServer: 'sql-studentexam-swc-${uniqueSuffix}'
  sqlDatabase: 'sqldb-studentexam'
  serviceBus: 'sb-notification-swc-${uniqueSuffix}'
  serviceBusQueue: 'notification-delivery'
  storage: 'stnotification${uniqueSuffix}'
  keyVault: 'kv-studentexam-${uniqueSuffix}'
  communication: 'acs-studentexam-swc-${uniqueSuffix}'
  emailService: 'acsemail-studentexam-${uniqueSuffix}'
  frontendApp: 'app-studentexam-web-swc'
}

var apiBaseUrl = 'https://${names.apiApp}.azurewebsites.net'
var functionBaseUrl = 'https://${names.functionApp}.azurewebsites.net'
var frontendBaseUrl = 'https://${names.frontendApp}.azurewebsites.net'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Cost-notification only. Creates no charge, caps nothing. Emails alertEmail at 50/75/90/100%
// of monthlyBudgetAmount so you get an early heads-up long before the credit is at risk.
resource budget 'Microsoft.Consumption/budgets@2023-11-01' = if (monthlyBudgetAmount > 0 && !empty(alertEmail)) {
  name: 'budget-studentexam-monthly'
  properties: {
    category: 'Cost'
    amount: monthlyBudgetAmount
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: '${budgetStartDate}T00:00:00Z'
    }
    notifications: {
      warn50: { enabled: true, operator: 'GreaterThanOrEqualTo', threshold: 50, contactEmails: [ alertEmail ], thresholdType: 'Actual' }
      warn75: { enabled: true, operator: 'GreaterThanOrEqualTo', threshold: 75, contactEmails: [ alertEmail ], thresholdType: 'Actual' }
      warn90: { enabled: true, operator: 'GreaterThanOrEqualTo', threshold: 90, contactEmails: [ alertEmail ], thresholdType: 'Actual' }
      warn100: { enabled: true, operator: 'GreaterThanOrEqualTo', threshold: 100, contactEmails: [ alertEmail ], thresholdType: 'Actual' }
    }
  }
}

module monitoring 'modules/monitoring.bicep' = {
  scope: rg
  name: 'monitoring'
  params: {
    location: location
    tags: tags
    workspaceName: names.logAnalytics
    apiComponentName: names.apiInsights
    notificationComponentName: names.notificationInsights
  }
}

module communication 'modules/communication.bicep' = {
  scope: rg
  name: 'communication'
  params: {
    tags: tags
    communicationName: names.communication
    emailName: names.emailService
  }
}

resource communicationResource 'Microsoft.Communication/communicationServices@2023-04-01' existing = {
  scope: rg
  name: names.communication
  dependsOn: [communication]
}

module sql 'modules/sql.bicep' = {
  scope: rg
  name: 'sql'
  params: {
    location: location
    tags: tags
    serverName: names.sqlServer
    databaseName: names.sqlDatabase
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    aadAdminObjectId: sqlAadAdminObjectId
    aadAdminLogin: sqlAadAdminLogin
  }
}

module keyVault 'modules/keyvault.bicep' = {
  scope: rg
  name: 'keyvault'
  params: {
    location: location
    tags: tags
    name: names.keyVault
    internalApiKey: internalApiKey
    jwtSigningKey: jwtSigningKey
    communicationConnectionString: communicationResource.listKeys().primaryConnectionString
    sqlServerFqdn: sql.outputs.serverFqdn
    sqlDatabaseName: sql.outputs.databaseName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
  }
}

module serviceBus 'modules/servicebus.bicep' = {
  scope: rg
  name: 'servicebus'
  params: {
    location: location
    tags: tags
    namespaceName: names.serviceBus
    queueName: names.serviceBusQueue
  }
}

module storage 'modules/storage.bicep' = {
  scope: rg
  name: 'storage'
  params: {
    location: location
    tags: tags
    name: names.storage
  }
}

var corsOrigins = union([frontendBaseUrl], additionalCorsOrigins)

module api 'modules/api.bicep' = {
  scope: rg
  name: 'api'
  params: {
    location: location
    tags: tags
    planName: names.apiPlan
    appName: names.apiApp
    planSku: apiPlanSku
    appInsightsConnectionString: monitoring.outputs.apiConnectionString
    sqlConnectionStringSecretUri: keyVault.outputs.sqlConnectionSecretUri
    notificationServiceBaseUrl: '${functionBaseUrl}/'
    allowedCorsOrigins: corsOrigins
    internalApiKeySecretUri: keyVault.outputs.internalApiKeySecretUri
    jwtKeySecretUri: keyVault.outputs.jwtKeySecretUri
  }
}

module frontend 'modules/frontend.bicep' = {
  scope: rg
  name: 'frontend'
  params: {
    location: location
    tags: tags
    appName: names.frontendApp
    planId: api.outputs.planId
  }
}

module functionApp 'modules/functionapp.bicep' = {
  scope: rg
  name: 'functionapp'
  params: {
    location: location
    tags: tags
    planName: names.functionPlan
    appName: names.functionApp
    storageAccountName: storage.outputs.accountName
    appInsightsConnectionString: monitoring.outputs.notificationConnectionString
    sqlConnectionStringSecretUri: keyVault.outputs.sqlConnectionSecretUri
    studentExamApiBaseUrl: '${apiBaseUrl}/'
    internalApiKeySecretUri: keyVault.outputs.internalApiKeySecretUri
    serviceBusFullyQualifiedNamespace: serviceBus.outputs.namespaceHostName
    notificationQueueName: serviceBus.outputs.queueName
    communicationConnectionStringSecretUri: keyVault.outputs.acsConnectionSecretUri
    communicationSenderAddress: communication.outputs.senderAddress
  }
}

module rbac 'modules/rbac.bicep' = {
  scope: rg
  name: 'rbac'
  params: {
    keyVaultName: names.keyVault
    serviceBusNamespaceName: names.serviceBus
    apiPrincipalId: api.outputs.principalId
    functionPrincipalId: functionApp.outputs.principalId
  }
}

module alerts 'modules/alerts.bicep' = {
  scope: rg
  name: 'alerts'
  params: {
    tags: tags
    serviceBusNamespaceId: serviceBus.outputs.namespaceId
    queueName: serviceBus.outputs.queueName
    alertEmail: alertEmail
  }
}

// ---- outputs -----------------------------------------------------------------------
output resourceGroup string = rg.name
output apiUrl string = api.outputs.defaultHostName
output apiName string = names.apiApp
output functionUrl string = functionApp.outputs.defaultHostName
output functionName string = names.functionApp
output frontendUrl string = frontend.outputs.defaultHostName
output frontendName string = names.frontendApp
output sqlServerFqdn string = sql.outputs.serverFqdn
output sqlDatabaseName string = names.sqlDatabase
output serviceBusNamespace string = serviceBus.outputs.namespaceHostName
output serviceBusQueue string = names.serviceBusQueue
output keyVaultName string = names.keyVault
output communicationSenderAddress string = communication.outputs.senderAddress
