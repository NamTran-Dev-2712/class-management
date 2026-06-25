using System.Linq;

namespace ClassManagement.IntegrationTests.Admin;

public class ReportTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    // Registers + logs in a user, returning the cookie client and the user's public id.
    private async Task<(HttpClient Client, string PublicId)> CreateUserAsync(string role)
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword,
            role
        );
        var login = await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var publicId = (await AuthHelper.ReadDataAsync(login)).GetProperty("publicId").GetString()!;
        return (client, publicId);
    }

    private static Task<HttpResponseMessage> SubmitUserReportAsync(
        HttpClient client,
        string targetPublicId
    ) =>
        client.PostAsJsonAsync(
            "/api/reports",
            new
            {
                targetType = "User",
                targetPublicId,
                reason = "Spam",
                description = "Spamming the platform.",
            }
        );

    [Fact]
    public async Task SubmitReport_IsIdempotent_PerTargetAndReporter()
    {
        var (reporter, _) = await CreateUserAsync("Student");
        var (_, targetId) = await CreateUserAsync("Student");

        var first = await SubmitUserReportAsync(reporter, targetId);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstId = (await AuthHelper.ReadDataAsync(first)).GetProperty("publicId").GetString();

        var second = await SubmitUserReportAsync(reporter, targetId);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondId = (await AuthHelper.ReadDataAsync(second)).GetProperty("publicId").GetString();

        secondId.Should().Be(firstId);

        // The reporter sees exactly one report in their own list.
        var mine = await reporter.GetAsync("/api/reports/mine");
        mine.StatusCode.Should().Be(HttpStatusCode.OK);
        (await AuthHelper.ReadDataAsync(mine))
            .GetProperty("items")
            .EnumerateArray()
            .Count(r => r.GetProperty("targetPublicId").GetString() == targetId)
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task AdminReview_BanUser_LocksTarget_AndNotifiesReporter()
    {
        var (reporter, _) = await CreateUserAsync("Student");
        var (target, targetId) = await CreateUserAsync("Student");

        var submit = await SubmitUserReportAsync(reporter, targetId);
        var reportId = (await AuthHelper.ReadDataAsync(submit)).GetProperty("publicId").GetString();

        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var review = await admin.PostAsJsonAsync(
            $"/api/admin/reports/{reportId}/review",
            new
            {
                status = "Resolved",
                adminAction = "BanUser",
                adminNote = "Confirmed spam.",
            }
        );
        review.StatusCode.Should().Be(HttpStatusCode.OK);

        // The banned user can no longer log in.
        var relogin = await AuthHelper.LoginAsync(
            _factory.CreateClient(),
            await GetEmailFromMineAsync(target),
            TestConstants.DefaultPassword
        );
        relogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // The reporter received a ReportResolved notification.
        var notifications = await reporter.GetAsync("/api/notifications");
        (await AuthHelper.ReadDataAsync(notifications))
            .GetProperty("items")
            .EnumerateArray()
            .Should()
            .Contain(n => n.GetProperty("eventType").GetString() == "ReportResolved");
    }

    // The target user's email isn't exposed via /reports/mine; re-derive it by re-registering would
    // change identity, so instead fetch it from the target's own profile.
    private static async Task<string> GetEmailFromMineAsync(HttpClient targetClient)
    {
        var me = await targetClient.GetAsync("/api/auth/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await AuthHelper.ReadDataAsync(me)).GetProperty("email").GetString()!;
    }

    [Fact]
    public async Task AdminReports_AsNonAdmin_Returns403()
    {
        var (student, _) = await CreateUserAsync("Student");
        var response = await student.GetAsync("/api/admin/reports");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
