using ClassManagement.IntegrationTests.Classroom;

namespace ClassManagement.IntegrationTests.Admin;

public class NotificationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<HttpClient> ApproveStudentIntoClassAsync()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var inviteCode = await ClassroomApi.GetInviteCodeAsync(teacher, classId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(student, inviteCode))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var membershipId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        var approve = await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{membershipId}/approve",
            null
        );
        approve.StatusCode.Should().Be(HttpStatusCode.OK);
        return student;
    }

    [Fact]
    public async Task ApproveMember_NotifiesStudent_WithPayload()
    {
        var student = await ApproveStudentIntoClassAsync();

        var response = await student.GetAsync("/api/notifications");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(response);
        var items = data.GetProperty("items").EnumerateArray().ToList();
        items.Should().NotBeEmpty();

        var approved = items.First(n =>
            n.GetProperty("eventType").GetString() == "ClassJoinApproved"
        );
        approved.GetProperty("status").GetString().Should().Be("Unread");
        approved.GetProperty("payload").GetProperty("className").GetString().Should().NotBeNull();
    }

    [Fact]
    public async Task UnreadCount_ReflectsMarkRead()
    {
        var student = await ApproveStudentIntoClassAsync();

        var before = await student.GetAsync("/api/notifications/unread-count");
        before.StatusCode.Should().Be(HttpStatusCode.OK);
        var beforeCount = (await AuthHelper.ReadDataAsync(before)).GetProperty("count").GetInt32();
        beforeCount.Should().BeGreaterThanOrEqualTo(1);

        var markAll = await student.PostAsync("/api/notifications/read-all", null);
        markAll.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await student.GetAsync("/api/notifications/unread-count");
        var afterCount = (await AuthHelper.ReadDataAsync(after)).GetProperty("count").GetInt32();
        afterCount.Should().Be(0);
    }

    [Fact]
    public async Task GetNotifications_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/notifications");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task NotificationHub_Negotiate_RequiresAuth()
    {
        // The realtime hub is [Authorize]: anonymous negotiate is rejected.
        var anon = _factory.CreateClient();
        var unauth = await anon.PostAsync("/hubs/notifications/negotiate?negotiateVersion=1", null);
        unauth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // An authenticated user can negotiate a connection.
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var ok = await student.PostAsync("/hubs/notifications/negotiate?negotiateVersion=1", null);
        ok.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
