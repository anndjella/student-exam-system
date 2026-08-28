@description('Global data location for the Communication Services resources.')
param dataLocation string = 'Europe'

@description('Communication Services resource name.')
param communicationName string

@description('Email Communication Services resource name.')
param emailName string

param tags object = {}

resource email 'Microsoft.Communication/emailServices@2023-04-01' = {
  name: emailName
  location: 'global'
  tags: tags
  properties: { dataLocation: dataLocation }
}

resource managedDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = {
  parent: email
  name: 'AzureManagedDomain'
  location: 'global'
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource communication 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: communicationName
  location: 'global'
  tags: tags
  properties: {
    dataLocation: dataLocation
    linkedDomains: [managedDomain.id]
  }
}

output communicationName string = communication.name
output communicationId string = communication.id
output senderAddress string = 'DoNotReply@${managedDomain.properties.fromSenderDomain}'
output endpoint string = 'https://${communication.properties.hostName}'
