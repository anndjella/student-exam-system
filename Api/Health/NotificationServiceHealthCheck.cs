using Application.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Health;

public sealed class NotificationServiceHealthCheck : IHealthCheck
{
    private readonly INotificationService _notificationService;

    public NotificationServiceHealthCheck(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _notificationService.IsHealthyAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("NotificationService reported that it is not ready.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "NotificationService is unavailable.",
                exception);
        }
    }
}
