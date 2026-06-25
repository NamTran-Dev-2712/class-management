using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.IntegrationTests.Admin;

public class AuditLogTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private Task<long?> GetUserIdAsync(string email) =>
        _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            return (long?)user?.Id;
        });

    [Fact]
    public async Task Login_WithValidCredentials_WritesLoginAuditEntry()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(client, "Audit User", email, TestConstants.DefaultPassword);

        var response = await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userId = await GetUserIdAsync(email);
        userId.Should().NotBeNull();

        var logged = await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            return await db.AuditLogs.AnyAsync(a =>
                a.Action == AuditActions.UserLogin && a.ActorId == userId
            );
        });
        logged.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithWrongPassword_WritesLoginFailedAuditEntry()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(client, "Audit User", email, TestConstants.DefaultPassword);

        var response = await AuthHelper.LoginAsync(client, email, "Wrong@123456");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var userId = await GetUserIdAsync(email);

        var logged = await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            return await db.AuditLogs.AnyAsync(a =>
                a.Action == AuditActions.UserLoginFailed && a.ActorId == userId
            );
        });
        logged.Should().BeTrue();
    }

    [Fact]
    public async Task Login_AfterFiveFailedAttempts_LocksAccountTemporarily()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(client, "Lock User", email, TestConstants.DefaultPassword);

        // Five wrong passwords trip the Identity lockout threshold (configured 5 / 15 min).
        for (var i = 0; i < 5; i++)
        {
            var fail = await AuthHelper.LoginAsync(client, email, "Wrong@123456");
            fail.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // The next attempt — even with the correct password — is rejected as temporarily locked.
        var locked = await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);
        locked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await locked.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().Contain("Too many");
    }

    [Fact]
    public async Task GetAuditLogs_AsAdmin_ReturnsEntries()
    {
        // Generate at least one audit entry, then read the admin log.
        var email = TestConstants.UniqueEmail();
        var userClient = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(
            userClient,
            "Seen User",
            email,
            TestConstants.DefaultPassword
        );
        await AuthHelper.LoginAsync(userClient, email, TestConstants.DefaultPassword);

        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var response = await admin.GetAsync("/api/admin/audit-logs?action=user.login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("items").EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAuditLogs_AsNonAdmin_Returns403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var response = await student.GetAsync("/api/admin/audit-logs");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
