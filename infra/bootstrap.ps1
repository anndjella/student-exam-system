#Requires -Version 5.1
<#
  ONE-TIME setup so GitHub Actions can deploy infra/.
#>
[CmdletBinding()]
param(
    [string]$SubscriptionId = '3cfd4685-61a2-4944-a8bf-32aeda2fa4e2',
    [string]$IdentityName   = 'id-github-student-exam-deploy',
    [string]$IdentityGroup  = 'rg-student-exam-dev',
    [string]$Repo           = 'anndjella/student-exam-system'
)

$ErrorActionPreference = 'Stop'
$scope = "/subscriptions/$SubscriptionId"

$principalId = az identity show --name $IdentityName --resource-group $IdentityGroup --query principalId -o tsv
if (-not $principalId) { throw "Identity '$IdentityName' not found in '$IdentityGroup'." }
Write-Host "Deploy identity: $principalId"

# Contributor                          -> create every resource in main.bicep
# Role Based Access Control Administrator -> create the role assignments in rbac.bicep
foreach ($role in @('Contributor', 'Role Based Access Control Administrator')) {
    $have = az role assignment list --assignee $principalId --scope $scope --role $role --query "[?scope=='$scope'] | [0].id" -o tsv
    if ($have) {
        Write-Host "  '$role' - already assigned" -ForegroundColor Yellow
        continue
    }
    az role assignment create `
        --assignee-object-id $principalId `
        --assignee-principal-type ServicePrincipal `
        --role $role `
        --scope $scope `
        --only-show-errors | Out-Null
    Write-Host "  '$role' - assigned" -ForegroundColor Green
}

# Federated credentials: which GitHub trigger may sign in without a secret.
#   master       -> deploy on push to master
#   pull_request -> what-if on PRs
$existing = @(az identity federated-credential list `
        --identity-name $IdentityName --resource-group $IdentityGroup `
        --query "[].subject" -o tsv)

$creds = [ordered]@{
    'github-master'       = "repo:${Repo}:ref:refs/heads/master"
    'github-pull-request' = "repo:${Repo}:pull_request"
}
foreach ($name in $creds.Keys) {
    $subject = $creds[$name]
    if ($existing -contains $subject) {
        Write-Host "  '$subject' - already trusted" -ForegroundColor Yellow
        continue
    }
    az identity federated-credential create `
        --name $name `
        --identity-name $IdentityName `
        --resource-group $IdentityGroup `
        --issuer 'https://token.actions.githubusercontent.com' `
        --subject $subject `
        --audiences 'api://AzureADTokenExchange' `
        --only-show-errors | Out-Null
    Write-Host "  '$subject' - trusted" -ForegroundColor Green
}

Write-Host "`nDone." -ForegroundColor Cyan
