namespace ClassManagement.IntegrationTests.Admin;

public class AnnouncementTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Send_ToStudents_NotifiesRecipients()
    {
        // Ensure at least one active student exists to receive the broadcast.
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        var send = await admin.PostAsJsonAsync(
            "/api/admin/announcements",
            new
            {
                title = "Scheduled maintenance",
                body = "The system will be down briefly tonight.",
                role = "Student",
            }
        );
        send.StatusCode.Should().Be(HttpStatusCode.OK);
        var recipients = (await AuthHelper.ReadDataAsync(send))
            .GetProperty("recipients")
            .GetInt32();
        recipients.Should().BeGreaterThanOrEqualTo(1);

        // The student sees the SystemAnnouncement in their notification feed.
        var feed = await AuthHelper.ReadDataAsync(await student.GetAsync("/api/notifications"));
        feed.GetProperty("items")
            .EnumerateArray()
            .Select(n => n.GetProperty("eventType").GetString())
            .Should()
            .Contain("SystemAnnouncement");
    }

    [Fact]
    public async Task Send_AsNonAdmin_Returns403()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var send = await teacher.PostAsJsonAsync(
            "/api/admin/announcements",
            new
            {
                title = "Nope",
                body = (string?)null,
                role = (string?)null,
            }
        );
        send.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
