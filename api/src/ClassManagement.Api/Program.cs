using ClassManagement.Api;
using ClassManagement.Api.Hubs;
using ClassManagement.Api.Security;
using ClassManagement.Application;
using ClassManagement.Infrastructure;
using ClassManagement.Infrastructure.Configuration;
using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.Infrastructure.Services.Assignments.Jobs;
using ClassManagement.Infrastructure.Services.Notifications.Jobs;
using ClassManagement.Infrastructure.Services.Payment.Jobs;
using Hangfire;
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
app.MapHub<NotificationHub>("/hubs/notifications");

// Hangfire dashboard — Admin-only (gated by config; authentication runs in UseApiMiddleware)
var hangfireOptions =
    app.Configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>()
    ?? new HangfireOptions();
if (hangfireOptions.Enabled)
{
    app.MapHangfireDashboard(
        hangfireOptions.DashboardPath,
        new DashboardOptions { Authorization = [new HangfireAdminAuthorizationFilter()] }
    );

    // Recurring assignment/attempt lifecycle sweep (Scheduled→Open, Open→Closed, auto-submit). Cron is
    // config-driven; the handler is idempotent so a re-run is safe.
    if (hangfireOptions.EnableServer)
    {
        var assignmentOptions =
            app.Configuration.GetSection(AssignmentOptions.SectionName).Get<AssignmentOptions>()
            ?? new AssignmentOptions();
        var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();
        recurringJobs.AddOrUpdate<AssignmentLifecycleJob>(
            "assignment-lifecycle",
            job => job.ExecuteAsync(),
            assignmentOptions.LifecycleSweepCron
        );

        // Notification background jobs (MVP-7): due-soon reminders + retention cleanup. Crons are
        // config-driven; both handlers are idempotent so a re-run is safe.
        var notificationOptions =
            app.Configuration.GetSection(NotificationOptions.SectionName).Get<NotificationOptions>()
            ?? new NotificationOptions();
        recurringJobs.AddOrUpdate<AssignmentDueSoonJob>(
            "assignment-due-soon",
            job => job.ExecuteAsync(),
            notificationOptions.DueSoonSweepCron
        );
        recurringJobs.AddOrUpdate<NotificationCleanupJob>(
            "notification-cleanup",
            job => job.ExecuteAsync(),
            notificationOptions.CleanupCron
        );

        // Subscription lifecycle sweep (MVP-8): expiry → PastDue/Cancelled, grace → Expired, stale
        // payment timeout, and expiring-soon reminders. Config-driven cron; handler is idempotent.
        var subscriptionOptions =
            app.Configuration.GetSection(SubscriptionOptions.SectionName).Get<SubscriptionOptions>()
            ?? new SubscriptionOptions();
        recurringJobs.AddOrUpdate<SubscriptionLifecycleJob>(
            "subscription-lifecycle",
            job => job.ExecuteAsync(),
            subscriptionOptions.LifecycleSweepCron
        );
    }
}

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

// log url on startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var url = app.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000";
logger.LogInformation("Starting API at {Url}", url);
logger.LogInformation(
    "Health endpoints: {Url}/health, {Url}/health/ready, {Url}/health/live",
    url,
    url,
    url
);
logger.LogInformation("API documentation available at {Url}/scalar/v1", url);

// Apply pending migrations + seed reference data
await DatabaseSeeder.SeedAsync(app.Services);

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
