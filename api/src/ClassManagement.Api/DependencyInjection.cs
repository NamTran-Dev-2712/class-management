using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using ClassManagement.Api.Contracts.Common;
using ClassManagement.Api.Contracts.Exceptions;
using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Interfaces.Localization;
using ClassManagement.Application.Interfaces.Settings;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Configuration;
using ClassManagement.Infrastructure.Security;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace ClassManagement.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy = System
                    .Text
                    .Json
                    .JsonNamingPolicy
                    .CamelCase;
                opts.JsonSerializerOptions.DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull;
                // Accept/emit enum names (e.g. "SingleChoice") instead of numbers in request/response
                // bodies — keeps the API self-describing and matches the string enums in read DTOs.
                opts.JsonSerializerOptions.Converters.Add(
                    new System.Text.Json.Serialization.JsonStringEnumConverter()
                );
            });

        // Built-in OpenAPI document (Scalar reads this)
        services.AddOpenApi();

        // SignalR realtime (MVP-7.5) — single-server. For multi-instance scale-out add a backplane
        // (e.g. .AddStackExchangeRedis(...)); the IRealtimeNotifier abstraction stays unchanged.
        services.AddSignalR();
        services.AddScoped<
            ClassManagement.Application.Interfaces.Notifications.IRealtimeNotifier,
            Hubs.SignalRNotifier
        >();

        // Global exception handler — returns ApiResponse<T> on all errors
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Health checks: Postgres + Redis
        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("DefaultConnection")!,
                name: "postgres",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "database"]
            )
            .AddRedis(
                redisConnectionString: configuration["Redis:ConnectionString"]!,
                name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: ["ready", "cache"]
            );

        // CORS — tighten in production via appsettings
        var allowedOrigins =
            configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173"];
        services.AddCors(opts =>
            opts.AddDefaultPolicy(policy =>
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            )
        );

        // Rate limiting — all limits configured from appsettings (no hardcoded values)
        var rl =
            configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
            ?? new RateLimitOptions();

        services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            opts.OnRejected = async (ctx, ct) =>
            {
                var localizer =
                    ctx.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
                ctx.HttpContext.Response.ContentType = "application/json";
                await ctx.HttpContext.Response.WriteAsJsonAsync(
                    ApiResponse<object?>.Fail(
                        429,
                        localizer["Error.TooManyRequests"],
                        null,
                        ctx.HttpContext.TraceIdentifier
                    ),
                    ct
                );
            };

            void AddIpFixedWindow(string name, RateLimitPolicyOptions policy) =>
                opts.AddPolicy(
                    name,
                    ctx =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = policy.PermitLimit,
                                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                                QueueLimit = 0,
                            }
                        )
                );

            // Per-user fixed window for authenticated student actions (start/save/submit). Partitioned
            // by the user id claim (falling back to IP) so many students behind one school NAT don't
            // share a bucket, while a single student is still throttled. See MVP-5 §12 auto-save risk.
            void AddUserFixedWindow(string name, RateLimitPolicyOptions policy) =>
                opts.AddPolicy(
                    name,
                    ctx =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? ctx.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = policy.PermitLimit,
                                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                                QueueLimit = 0,
                            }
                        )
                );

            AddIpFixedWindow(RateLimitOptions.Policies.Login, rl.Login);
            AddIpFixedWindow(RateLimitOptions.Policies.Register, rl.Register);
            AddIpFixedWindow(RateLimitOptions.Policies.Refresh, rl.Refresh);
            AddIpFixedWindow(RateLimitOptions.Policies.ChangePassword, rl.ChangePassword);
            AddIpFixedWindow(RateLimitOptions.Policies.UpdateProfile, rl.UpdateProfile);
            AddIpFixedWindow(RateLimitOptions.Policies.ForgotPassword, rl.ForgotPassword);
            AddIpFixedWindow(RateLimitOptions.Policies.ResetPassword, rl.ResetPassword);
            AddIpFixedWindow(RateLimitOptions.Policies.Logout, rl.Logout);
            AddIpFixedWindow(RateLimitOptions.Policies.SubjectWrite, rl.SubjectWrite);
            AddIpFixedWindow(RateLimitOptions.Policies.UserWrite, rl.UserWrite);
            AddIpFixedWindow(RateLimitOptions.Policies.ClassWrite, rl.ClassWrite);
            AddIpFixedWindow(RateLimitOptions.Policies.ClassJoin, rl.ClassJoin);
            AddIpFixedWindow(RateLimitOptions.Policies.QuestionWrite, rl.QuestionWrite);
            AddIpFixedWindow(RateLimitOptions.Policies.ExamWrite, rl.ExamWrite);
            AddIpFixedWindow(RateLimitOptions.Policies.AssignmentWrite, rl.AssignmentWrite);
            AddUserFixedWindow(RateLimitOptions.Policies.AttemptStart, rl.AttemptStart);
            AddUserFixedWindow(RateLimitOptions.Policies.AttemptSave, rl.AttemptSave);
            AddUserFixedWindow(RateLimitOptions.Policies.AttemptSubmit, rl.AttemptSubmit);
            AddIpFixedWindow(RateLimitOptions.Policies.GradeWrite, rl.GradeWrite);
            AddIpFixedWindow(RateLimitOptions.Policies.Export, rl.Export);
            // Admin & moderation (MVP-7): report submit + notification writes are per-user (shared NAT),
            // admin actions + settings writes are per-IP.
            AddUserFixedWindow(RateLimitOptions.Policies.ReportWrite, rl.ReportWrite);
            AddUserFixedWindow(RateLimitOptions.Policies.NotificationWrite, rl.NotificationWrite);
            AddIpFixedWindow(RateLimitOptions.Policies.AdminAction, rl.AdminAction);
            AddIpFixedWindow(RateLimitOptions.Policies.SystemSettingWrite, rl.SystemSettingWrite);
            AddUserFixedWindow(RateLimitOptions.Policies.PaymentCheckout, rl.PaymentCheckout);
            AddIpFixedWindow(RateLimitOptions.Policies.PaymentWebhook, rl.PaymentWebhook);
            AddIpFixedWindow(RateLimitOptions.Policies.Read, rl.Read);
        });

        // Output caching — caches GET responses (keyed by all query params + Accept-Language, plus the
        // user id for personalized reads) and is invalidated on writes via IOutputCacheStore tags.
        // Registered unconditionally so IOutputCacheStore is always resolvable for eviction; the
        // middleware itself is gated by OutputCache:Enabled (see UseApiMiddleware).
        var oc =
            configuration.GetSection(OutputCacheSettings.SectionName).Get<OutputCacheSettings>()
            ?? new OutputCacheSettings();

        services.AddOutputCache(options =>
        {
            var sharedExpiry = TimeSpan.FromSeconds(oc.DefaultExpirySeconds);
            var perUserExpiry = TimeSpan.FromSeconds(oc.PerUserExpirySeconds);

            // Shared read: same response for everyone at a given authorization level — vary only by
            // query params (no hardcoded keys) and the request culture.
            OutputCachePolicyBuilder Shared(OutputCachePolicyBuilder b, string tag) =>
                b.Expire(sharedExpiry)
                    .SetVaryByQuery("*")
                    .SetVaryByHeader("Accept-Language")
                    .Tag(tag);

            // Per-user read: personalized lists — additionally vary by the caller's id so one user
            // never receives another's cached page.
            OutputCachePolicyBuilder PerUser(OutputCachePolicyBuilder b, string tag) =>
                Shared(b, tag)
                    .Expire(perUserExpiry)
                    .VaryByValue(ctx => new KeyValuePair<string, string>(
                        "uid",
                        ctx.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anon"
                    ));

            options.AddPolicy(
                OutputCachePolicies.SubjectsRead,
                b => Shared(b, OutputCacheTags.Subjects)
            );
            options.AddPolicy(OutputCachePolicies.UsersRead, b => Shared(b, OutputCacheTags.Users));
            options.AddPolicy(
                OutputCachePolicies.AdminClassesRead,
                b => Shared(b, OutputCacheTags.Classrooms)
            );
            options.AddPolicy(
                OutputCachePolicies.TeacherClassesRead,
                b => PerUser(b, OutputCacheTags.Classrooms)
            );
            options.AddPolicy(
                OutputCachePolicies.StudentClassesRead,
                b => PerUser(b, OutputCacheTags.Classrooms)
            );

            // Question bank (MVP-3): a teacher's own bank is personalized (PerUser); the public pool
            // and the admin all-questions list are the same for every caller at that authorization
            // level (Shared). All tagged "questions" so any write evicts every related read.
            options.AddPolicy(
                OutputCachePolicies.TeacherQuestionsRead,
                b => PerUser(b, OutputCacheTags.Questions)
            );
            options.AddPolicy(
                OutputCachePolicies.PublicQuestionsRead,
                b => Shared(b, OutputCacheTags.Questions)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminQuestionsRead,
                b => Shared(b, OutputCacheTags.Questions)
            );

            // Exam builder (MVP-4): a teacher's own bank is personalized (PerUser); the public pool
            // and the admin-wide list are the same for everyone at a given authorization level
            // (Shared). All tagged "exams" so any write evicts every related read.
            options.AddPolicy(
                OutputCachePolicies.TeacherExamsRead,
                b => PerUser(b, OutputCacheTags.Exams)
            );
            options.AddPolicy(
                OutputCachePolicies.PublicExamsRead,
                b => Shared(b, OutputCacheTags.Exams)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminExamsRead,
                b => Shared(b, OutputCacheTags.Exams)
            );

            // Assignment & online testing (MVP-5): teacher's own assignments + student's own list +
            // attempt rosters/history are personalized (PerUser); the admin-wide list is Shared. All
            // tagged "assignments" so any assignment/attempt write evicts every related read. NOTE: the
            // live attempt-taking GET and the attempt-result GET are deliberately NOT output-cached —
            // they are time-sensitive (remaining time) / score-gated and must always be fresh.
            options.AddPolicy(
                OutputCachePolicies.TeacherAssignmentsRead,
                b => PerUser(b, OutputCacheTags.Assignments)
            );
            options.AddPolicy(
                OutputCachePolicies.StudentAssignmentsRead,
                b => PerUser(b, OutputCacheTags.Assignments)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminAssignmentsRead,
                b => Shared(b, OutputCacheTags.Assignments)
            );
            options.AddPolicy(
                OutputCachePolicies.AttemptsRead,
                b => PerUser(b, OutputCacheTags.Assignments)
            );
            // Assignment report (MVP-6): owner-scoped (PerUser); evicted by grade/release writes via the
            // shared "assignments" tag. The CSV export + grading-detail GETs are NOT cached.
            options.AddPolicy(
                OutputCachePolicies.AssignmentReportRead,
                b => PerUser(b, OutputCacheTags.Assignments)
            );

            // Admin & moderation (MVP-7). Audit log / admin reports / dashboard are the same for every
            // admin (Shared); a user's own notifications, unread count, and own reports are personalized
            // (PerUser). System settings are admin-wide (Shared). Each tagged by its resource domain.
            options.AddPolicy(
                OutputCachePolicies.AdminAuditLogsRead,
                b => Shared(b, OutputCacheTags.AuditLogs)
            );
            options.AddPolicy(
                OutputCachePolicies.NotificationsRead,
                b => PerUser(b, OutputCacheTags.Notifications)
            );
            options.AddPolicy(
                OutputCachePolicies.NotificationUnreadCount,
                b => PerUser(b, OutputCacheTags.Notifications)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminReportsRead,
                b => Shared(b, OutputCacheTags.Reports)
            );
            options.AddPolicy(
                OutputCachePolicies.MyReportsRead,
                b => PerUser(b, OutputCacheTags.Reports)
            );
            options.AddPolicy(
                OutputCachePolicies.SystemSettingsRead,
                b => Shared(b, OutputCacheTags.SystemSettings)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminDashboardRead,
                b => Shared(b, OutputCacheTags.AdminDashboard)
            );
            // Public app config (brand + maintenance) — same for everyone, tagged system-settings so an
            // admin settings change evicts it.
            options.AddPolicy(
                OutputCachePolicies.PublicConfigRead,
                b => Shared(b, OutputCacheTags.SystemSettings)
            );

            // Premium & payment (MVP-8)
            options.AddPolicy(OutputCachePolicies.PlansRead, b => Shared(b, OutputCacheTags.Plans));
            options.AddPolicy(
                OutputCachePolicies.TeacherSubscriptionRead,
                b => PerUser(b, OutputCacheTags.Subscriptions)
            );
            options.AddPolicy(
                OutputCachePolicies.SubscriptionUsageRead,
                b => PerUser(b, OutputCacheTags.Subscriptions)
            );
            options.AddPolicy(
                OutputCachePolicies.InvoicesRead,
                b => PerUser(b, OutputCacheTags.Invoices)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminSubscriptionsRead,
                b => Shared(b, OutputCacheTags.Subscriptions)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminPaymentsRead,
                b => Shared(b, OutputCacheTags.Payments)
            );
            options.AddPolicy(
                OutputCachePolicies.AdminRevenueRead,
                b => Shared(b, OutputCacheTags.Payments)
            );
        });

        return services;
    }

    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // OpenAPI JSON: GET /openapi/v1.json
            app.MapOpenApi();
            // Scalar UI: GET /scalar/v1
            app.MapScalarApiReference(opts =>
                opts.WithTitle("Class Management API")
                    .WithTheme(ScalarTheme.Purple)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            );
        }

        // Request culture (Accept-Language) must be set before the exception handler so
        // error responses are localized; registered first to stay outer in the pipeline.
        var loc =
            app.Configuration.GetSection(LocalizationOptions.SectionName).Get<LocalizationOptions>()
            ?? new LocalizationOptions();
        var cultures = loc.SupportedCultures.Select(c => new CultureInfo(c)).ToList();
        app.UseRequestLocalization(
            new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(loc.DefaultCulture),
                SupportedCultures = cultures,
                SupportedUICultures = cultures,
                ApplyCurrentCultureToResponseHeaders = true,
            }
        );

        // Security headers on every response (production hardening, MVP-7). The strict API-only CSP is
        // applied outside Development so it doesn't break the Scalar UI (which loads scripts) in dev.
        var isDevelopment = app.Environment.IsDevelopment();
        app.Use(
            async (context, next) =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "no-referrer";
                headers["X-XSS-Protection"] = "0";
                if (!isDevelopment)
                    headers["Content-Security-Policy"] =
                        "default-src 'none'; frame-ancestors 'none'";
                await next();
            }
        );

        app.UseExceptionHandler();
        app.UseCors();
        app.UseRateLimiter();
        if (!isDevelopment)
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        app.UseAuthentication();
        app.UseAuthorization();

        // Maintenance gate (MVP-7.5): while maintenance_mode is on, reject non-admin write requests with
        // 503 (reads + admins + auth endpoints stay open). Live — the settings cache is invalidated on the
        // admin toggle. Only read the (cached) flag for write methods to avoid per-GET overhead.
        app.Use(
            async (context, next) =>
            {
                var method = context.Request.Method;
                var isWrite =
                    HttpMethods.IsPost(method)
                    || HttpMethods.IsPut(method)
                    || HttpMethods.IsPatch(method)
                    || HttpMethods.IsDelete(method);

                if (isWrite && !context.Request.Path.StartsWithSegments("/api/auth"))
                {
                    var settings =
                        context.RequestServices.GetRequiredService<ISystemSettingsService>();
                    var maintenance = await settings.GetBoolAsync(
                        SystemSettingKeys.MaintenanceMode,
                        false,
                        context.RequestAborted
                    );
                    if (maintenance && !context.User.IsInRole(ApplicationRoles.Admin))
                    {
                        var localizer =
                            context.RequestServices.GetRequiredService<ILocalizationService>();
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(
                            ApiResponse<object?>.Fail(
                                StatusCodes.Status503ServiceUnavailable,
                                localizer["Error.Maintenance"],
                                null,
                                context.TraceIdentifier
                            ),
                            context.RequestAborted
                        );
                        return;
                    }
                }

                await next();
            }
        );

        // Output cache runs AFTER authentication/authorization so a cache hit can never bypass authz
        // (the authorization middleware has already vetted the request). Gated by config so it can be
        // turned off without code changes. Reads OutputCache:Enabled from the fully-built config
        // (honours test ConfigureAppConfiguration overrides).
        var outputCacheEnabled = app.Configuration.GetValue(
            $"{OutputCacheSettings.SectionName}:Enabled",
            true
        );
        if (outputCacheEnabled)
            app.UseOutputCache();

        return app;
    }
}
