using System.Globalization;
using System.Threading.RateLimiting;
using ClassManagement.Api.Contracts.Common;
using ClassManagement.Api.Contracts.Exceptions;
using ClassManagement.Application.Interfaces.Localization;
using ClassManagement.Infrastructure.Configuration;
using ClassManagement.Infrastructure.Security;
using Microsoft.AspNetCore.Localization;
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
            });

        // Built-in OpenAPI document (Scalar reads this)
        services.AddOpenApi();

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

        app.UseExceptionHandler();
        app.UseCors();
        app.UseRateLimiter();
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
