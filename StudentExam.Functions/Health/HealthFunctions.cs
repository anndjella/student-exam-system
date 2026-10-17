using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StudentExam.Functions.Health;

// Same contract as the Api's MapHealthChecks: plain-text status,
// 200 when Healthy/Degraded, 503 when Unhealthy.
public sealed class HealthFunctions
{
    private readonly HealthCheckService _healthChecks;

    public HealthFunctions(HealthCheckService healthChecks)
    {
        _healthChecks = healthChecks;
    }

    // liveness: the process is up, no dependency checks
    [Function("HealthLive")]
    public IActionResult Live(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/live")] HttpRequest req)
        => ToResult(HealthStatus.Healthy);

    // readiness: database + NotificationService
    [Function("HealthReady")]
    public async Task<IActionResult> Ready(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/ready")] HttpRequest req,
        CancellationToken ct)
    {
        var report = await _healthChecks.CheckHealthAsync(
            registration => registration.Tags.Contains("ready"),
            ct);

        return ToResult(report.Status);
    }

    private static IActionResult ToResult(HealthStatus status)
        => new ContentResult
        {
            Content = status.ToString(),
            ContentType = "text/plain",
            StatusCode = status == HealthStatus.Unhealthy
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status200OK
        };
}
