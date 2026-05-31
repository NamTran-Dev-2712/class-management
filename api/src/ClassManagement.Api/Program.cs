using ClassManagement.Api;
using ClassManagement.Application;
using ClassManagement.Infrastructure;
using ClassManagement.Infrastructure.Persistence.DbContext;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiMiddleware();
app.MapControllers();

// Health check endpoints:
// GET /health        — all checks, JSON (HealthChecks.UI format)
// GET /health/ready  — tagged "ready" (Postgres + Redis) — K8s readiness probe
// GET /health/live   — always Healthy, no deps — K8s liveness probe
app.MapHealthChecks(
    "/health",
    new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
);

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    }
);

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    }
);

// Apply pending migrations + seed reference data
await DatabaseSeeder.SeedAsync(app.Services);

app.Run();
