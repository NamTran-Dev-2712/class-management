using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ClassManagement.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("classmanagement_test")
        .WithUsername("test_user")
        .WithPassword("test_pass")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder().Build();

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

                        // Very lenient rate limits — prevent test failures due to rate limiting
                        ["RateLimit:Login:PermitLimit"] = "1000",
                        ["RateLimit:Login:WindowSeconds"] = "60",
                        ["RateLimit:Register:PermitLimit"] = "1000",
                        ["RateLimit:Register:WindowSeconds"] = "60",
                        ["RateLimit:Refresh:PermitLimit"] = "1000",
                        ["RateLimit:Refresh:WindowSeconds"] = "60",
                        ["RateLimit:ChangePassword:PermitLimit"] = "1000",
                        ["RateLimit:ChangePassword:WindowSeconds"] = "60",
                        ["RateLimit:UpdateProfile:PermitLimit"] = "1000",
                        ["RateLimit:UpdateProfile:WindowSeconds"] = "60",
                    }
                );
            }
        );
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
