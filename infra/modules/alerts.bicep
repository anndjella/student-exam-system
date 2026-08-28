@description('Service Bus namespace resource id to monitor.')
param serviceBusNamespaceId string

@description('Queue name whose dead-letter sub-queue is watched.')
param queueName string

@description('Email address that receives alert notifications. Empty = alert rule only, no email.')
param alertEmail string = ''

param tags object = {}

var hasEmail = !empty(alertEmail)

resource actionGroup 'Microsoft.Insights/actionGroups@2023-09-01-preview' = if (hasEmail) {
  name: 'ag-studentexam-alerts'
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'examalerts'
    enabled: true
    emailReceivers: [
      {
        name: 'owner'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

// Fires when the notification-delivery queue's dead-letter sub-queue is not empty,
// i.e. a message failed processing 5 times and would otherwise be silently stuck.
resource deadLetterAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'alert-notification-deadletter'
  location: 'global'
  tags: tags
  properties: {
    description: 'A notification message landed in the dead-letter queue (failed 5 delivery attempts).'
    severity: 2
    enabled: true
    scopes: [ serviceBusNamespaceId ]
    // Metric alerts cannot run once a day; PT1H is the least frequent allowed.
    // Cost is a flat per-rule charge regardless of frequency.
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H'
    autoMitigate: true
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'DeadLetteredMessages'
          metricName: 'DeadletteredMessages'
          metricNamespace: 'Microsoft.ServiceBus/namespaces'
          operator: 'GreaterThan'
          threshold: 0
          timeAggregation: 'Maximum'
          criterionType: 'StaticThresholdCriterion'
          dimensions: [
            {
              name: 'EntityName'
              operator: 'Include'
              values: [ queueName ]
            }
          ]
        }
      ]
    }
    actions: hasEmail ? [ { actionGroupId: actionGroup.id } ] : []
  }
}

output alertName string = deadLetterAlert.name
