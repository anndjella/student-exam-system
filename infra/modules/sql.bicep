@description('Azure region.')
param location string

@description('Logical SQL server name (globally unique).')
param serverName string

@description('Database name.')
param databaseName string

param tags object = {}

@description('SQL authentication administrator login.')
param sqlAdminLogin string = 'studentexamadmin'

@description('SQL authentication administrator password.')
@secure()
param sqlAdminPassword string

@description('Optional Entra ID admin object id (a user or group). Empty = no Entra admin.')
param aadAdminObjectId string = ''

@description('Optional Entra ID admin display name.')
param aadAdminLogin string = ''

@description('Database SKU. GP_S_Gen5_1 = General Purpose serverless, max 1 vCore (cheaper than the current 2).')
param skuName string = 'GP_S_Gen5_1'

@description('Serverless max vCore capacity. 1 halves the worst-case active cost vs the current 2.')
param skuCapacity int = 1

@description('Auto-pause delay in minutes. 60 = the minimum; the DB stops billing compute when idle.')
param autoPauseDelayMinutes int = 60

@description('Max size in bytes. 32 GiB is included free under the SQL free offer.')
param maxSizeBytes int = 34359738368

@description('Use the Azure SQL Database free offer (shared ~100k vCore-seconds/month across the subscription).')
param useFreeLimit bool = true

@description('What happens when the free allowance runs out. AutoPause = never charge (DB pauses until next month). BillOverUsage = keep running and bill.')
@allowed([ 'AutoPause', 'BillOverUsage' ])
param freeLimitExhaustionBehavior string = 'AutoPause'

@description('Extra client IP firewall rules: [{ name, startIp, endIp }].')
param clientFirewallRules array = []

var hasAadAdmin = !empty(aadAdminObjectId)

resource server 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    administrators: hasAadAdmin ? {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: aadAdminLogin
      sid: aadAdminObjectId
      tenantId: tenant().tenantId
      azureADOnlyAuthentication: false
    } : null
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: server
  name: databaseName
  location: location
  tags: tags
  sku: { name: skuName, tier: 'GeneralPurpose', family: 'Gen5', capacity: skuCapacity }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: maxSizeBytes
    autoPauseDelay: autoPauseDelayMinutes
    minCapacity: json('0.5')
    zoneRedundant: false
    useFreeLimit: useFreeLimit
    freeLimitExhaustionBehavior: useFreeLimit ? freeLimitExhaustionBehavior : null
  }
}

resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: server
  name: 'AllowAllWindowsAzureIps'
  properties: { startIpAddress: '0.0.0.0', endIpAddress: '0.0.0.0' }
}

resource clientRules 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = [for rule in clientFirewallRules: {
  parent: server
  name: rule.name
  properties: { startIpAddress: rule.startIp, endIpAddress: rule.endIp }
}]

output serverName string = server.name
output serverFqdn string = server.properties.fullyQualifiedDomainName
output databaseName string = database.name
output adminLogin string = sqlAdminLogin
