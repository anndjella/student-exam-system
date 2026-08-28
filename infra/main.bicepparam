using './main.bicep'

param location = 'swedencentral'
param resourceGroupName = 'rg-studentexam-swc'
param apiPlanSku = 'F1'

param sqlAdminLogin = 'studentexamadmin'

param sqlAadAdminObjectId = readEnvironmentVariable('SQL_ADMIN_GROUP_OBJECT_ID', '')
param sqlAadAdminLogin = readEnvironmentVariable('SQL_ADMIN_GROUP_NAME', '')

param additionalCorsOrigins = ['http://localhost:5173']

// Email that receives the dead-letter alert and cost-notification emails.
param alertEmail = readEnvironmentVariable('ALERT_EMAIL', 'milanmima2000@gmail.com')

// Spend (EUR) at which warning emails begin. Not a charge, not a cap - just notifications.
param monthlyBudgetAmount = 10

param sqlAdminPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD', '')
param internalApiKey = readEnvironmentVariable('INTERNAL_API_KEY', '')
param jwtSigningKey = readEnvironmentVariable('JWT_SIGNING_KEY', '')
