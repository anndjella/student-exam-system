@description('Azure region.')
param location string

@description('Key Vault name (3-24 chars, globally unique).')
param name string

param tags object = {}

@description('Shared internal API key used between the API and the Notification Service.')
@secure()
param internalApiKey string

@description('Symmetric signing key for the API JWT tokens.')
@secure()
param jwtSigningKey string

@description('Azure Communication Services connection string (email).')
@secure()
param communicationConnectionString string

param sqlServerFqdn string
param sqlDatabaseName string
param sqlAdminLogin string
@secure()
param sqlAdminPassword string

resource vault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    publicNetworkAccess: 'Enabled'
  }
}

resource internalApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vault
  name: 'internal-api-key'
  properties: { value: internalApiKey }
}

resource jwtKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vault
  name: 'jwt-signing-key'
  properties: { value: jwtSigningKey }
}

resource acsConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vault
  name: 'communication-connection-string'
  properties: { value: communicationConnectionString }
}

resource sqlConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vault
  name: 'sql-connection-string'
  properties: {
    value: 'Server=tcp:${sqlServerFqdn},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;'
  }
}

output vaultName string = vault.name
output vaultId string = vault.id
output vaultUri string = vault.properties.vaultUri
output internalApiKeySecretUri string = internalApiKeySecret.properties.secretUri
output jwtKeySecretUri string = jwtKeySecret.properties.secretUri
output acsConnectionSecretUri string = acsConnectionSecret.properties.secretUri
output sqlConnectionSecretUri string = sqlConnectionSecret.properties.secretUri
