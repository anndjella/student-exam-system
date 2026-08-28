@description('Azure region.')
param location string

@description('Service Bus namespace name (globally unique).')
param namespaceName string

@description('Queue name consumed by the Notification Service.')
param queueName string

@description('SKU. Basic = pay-per-operation (cheapest). Standard adds duplicate detection, topics, sessions.')
@allowed(['Basic', 'Standard'])
param sku string = 'Basic'

param tags object = {}

resource namespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  tags: tags
  sku: { name: sku, tier: sku }
  properties: {
    minimumTlsVersion: '1.2'
    disableLocalAuth: false
  }
}

resource queue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: queueName
  properties: {
    requiresDuplicateDetection: sku == 'Standard'
    duplicateDetectionHistoryTimeWindow: sku == 'Standard' ? 'PT1H' : null
    requiresSession: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 5
    defaultMessageTimeToLive: 'P7D'
    deadLetteringOnMessageExpiration: true
    enableBatchedOperations: true
  }
}

output namespaceName string = namespace.name
output namespaceId string = namespace.id
output namespaceHostName string = '${namespace.name}.servicebus.windows.net'
output queueName string = queue.name
