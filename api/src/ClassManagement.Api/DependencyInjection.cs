using ClassManagement.Api.Contracts.Exceptions;
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

        app.UseExceptionHandler();
        app.UseCors();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
