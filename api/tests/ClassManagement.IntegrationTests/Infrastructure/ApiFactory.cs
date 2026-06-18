using ClassManagement.Application.Interfaces.Messaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ClassManagement.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Some options are bound at service-registration time (Hangfire gating, rate-limit policies),
    // which runs before ConfigureAppConfiguration is applied. Only environment variables and lazily
    // bound options (e.g. connection strings, JwtOptions) reflect those overrides that early — so
    // set the build-time-sensitive test values here.
    static ApiFactory()
    {
        Environment.SetEnvironmentVariable("Hangfire__Enabled", "false");

        string[] policies =
        [
            "Login",
            "Register",
            "Refresh",
            "ChangePassword",
            "UpdateProfile",
            "ForgotPassword",
            "ResetPassword",
            "Logout",
            "SubjectWrite",
            "UserWrite",
            "ClassWrite",
            "ClassJoin",
            "QuestionWrite",
            "ExamWrite",
            "AssignmentWrite",
            "AttemptStart",
            "AttemptSave",
            "AttemptSubmit",
            "Read",
        ];
        foreach (var policy in policies)
        {
            Environment.SetEnvironmentVariable($"RateLimit__{policy}__PermitLimit", "100000");
            Environment.SetEnvironmentVariable($"RateLimit__{policy}__WindowSeconds", "60");
        }
    }

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("classmanagement_test")
        .WithUsername("test_user")
        .WithPassword("test_pass")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder().Build();

    // Captures password-reset OTPs so tests can drive the reset-password flow.
    public FakeEmailQueueService EmailQueue { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, cfg) =>
            {
                cfg.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        // Override with container connection strings
                        ["ConnectionStrings:DefaultConnection"] =
                            $"{_postgres.GetConnectionString()};Include Error Detail=true",
                        ["Redis:ConnectionString"] = _redis.GetConnectionString(),

                        // Use a deterministic test JWT secret
                        ["Jwt:SecretKey"] = "test-jwt-secret-key-minimum-32-characters!!",
                        ["Jwt:ExpiryMinutes"] = "60",

                        // Test seed credentials
                        ["Seed:AdminEmail"] = "admin@test.local",
                        ["Seed:AdminPassword"] = "Admin@123456",
                        ["Seed:AdminDisplayName"] = "Test Admin",

                        // Hangfire off in tests — email delivery is faked (see EmailQueue)
                        ["Hangfire:Enabled"] = "false",

                        // Deterministic client app base for building reset links
                        ["ClientApp:BaseUrl"] = "http://localhost:5173",
                        ["ClientApp:ResetPasswordPath"] = "/reset-password",

                        // Resend is never called in tests, but config must bind
                        ["Resend:ApiKey"] = "test-resend-key",
                        ["Resend:FromEmail"] = "no-reply@test.local",
                        ["Resend:FromName"] = "Class Management Test",

                        // Rate limits are made lenient via environment variables in the static
                        // constructor (they're bound at registration time, before this applies).
                    }
                );
            }
        );

        // Replace the Hangfire-backed email queue with an in-memory capture.
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailQueueService>();
            services.AddSingleton<IEmailQueueService>(EmailQueue);
        });
    }

    // Returns an HttpClient that automatically manages cookies across requests
    public HttpClient CreateClientWithCookies()
    {
        var handler = new CookieContainerHandler(Server.CreateHandler());
        return new HttpClient(handler) { BaseAddress = Server.BaseAddress };
    }

    // Provides scoped access to infrastructure services (e.g. DbContext for DB assertions)
    public async Task<T> WithScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
        await base.DisposeAsync();
    }
}
