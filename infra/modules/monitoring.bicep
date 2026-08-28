@description('Azure region for all resources.')
param location string

@description('Log Analytics workspace name.')
param workspaceName string

@description('Application Insights component name for the Student Exam API.')
param apiComponentName string

@description('Application Insights component name for the Notification Service.')
param notificationComponentName string

param tags object = {}

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
    features: { enableLogAccessUsingOnlyResourcePermissions: true }
    // Hard cap on log ingestion so a crash-loop / log storm can't run up a bill.
    workspaceCapping: { dailyQuotaGb: 1 }
  }
}

resource apiInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: apiComponentName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
    IngestionMode: 'LogAnalytics'
    RetentionInDays: 30
  }
}

resource notificationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: notificationComponentName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
    IngestionMode: 'LogAnalytics'
    RetentionInDays: 30
  }
}

output workspaceId string = workspace.id
output apiConnectionString string = apiInsights.properties.ConnectionString
output notificationConnectionString string = notificationInsights.properties.ConnectionString
