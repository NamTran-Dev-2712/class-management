using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClassManagement.Api.Controllers;

[AllowAnonymous]
public sealed class HealthCheckController : BaseApiController
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromServices] HealthCheckService healthCheckService,
        CancellationToken ct
    )
    {
        var report = await healthCheckService.CheckHealthAsync(ct);

        var result = new
        {
            status = report.Status.ToString(),
            version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production",
            uptimeSeconds = (long)(DateTimeOffset.UtcNow - StartTime).TotalSeconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds,
            }),
        };

        var httpStatusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
        return StatusCode(
            httpStatusCode,
            ApiResponse<object>.Ok(result, HttpContext.TraceIdentifier)
        );
    }
}
