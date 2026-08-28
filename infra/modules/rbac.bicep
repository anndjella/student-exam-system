@description('Key Vault name for secret-read grants.')
param keyVaultName string

@description('Service Bus namespace name for messaging grants.')
param serviceBusNamespaceName string

@description('API Web App system-assigned principalId.')
param apiPrincipalId string

@description('Function App system-assigned principalId.')
param functionPrincipalId string

var roles = {
  keyVaultSecretsUser: '4633458b-17de-408a-b874-0445c86b69e6'
  serviceBusDataSender: '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
  serviceBusDataReceiver: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = { name: keyVaultName }
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = { name: serviceBusNamespaceName }

resource apiKeyVault 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, apiPrincipalId, roles.keyVaultSecretsUser)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsUser)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource functionKeyVault 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, functionPrincipalId, roles.keyVaultSecretsUser)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsUser)
    principalId: functionPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource functionSbSender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBus.id, functionPrincipalId, roles.serviceBusDataSender)
  scope: serviceBus
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.serviceBusDataSender)
    principalId: functionPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource functionSbReceiver 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBus.id, functionPrincipalId, roles.serviceBusDataReceiver)
  scope: serviceBus
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.serviceBusDataReceiver)
    principalId: functionPrincipalId
    principalType: 'ServicePrincipal'
  }
}
