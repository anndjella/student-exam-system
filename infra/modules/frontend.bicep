@description('Azure region.')
param location string

@description('Frontend Web App name (globally unique).')
param appName string

@description('App Service plan resource id (shared with the API, matching the current setup).')
param planId string

param tags object = {}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  tags: tags
  properties: {
    serverFarmId: planId
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      defaultDocuments: ['index.html']
    }
  }
}

output name string = app.name
output defaultHostName string = 'https://${app.properties.defaultHostName}'
