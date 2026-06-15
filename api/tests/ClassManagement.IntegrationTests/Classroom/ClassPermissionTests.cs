namespace ClassManagement.IntegrationTests.Classroom;

public class ClassPermissionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task TeacherEndpoints_RequireTeacherRole_StudentGets403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var resp = await student.GetAsync("/api/teacher/classes");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StudentEndpoints_RequireStudentRole_TeacherGets403()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var resp = await teacher.PostAsJsonAsync(
            "/api/student/classes/join",
            new { inviteCode = "ABC123" }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminClassesEndpoint_RequiresAdmin_TeacherGets403()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var resp = await teacher.GetAsync("/api/admin/classes");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TeacherEndpoints_RequireAuth_AnonymousGets401()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/teacher/classes");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveMember_ForClassNotOwned_Returns403()
    {
        var owner = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(owner);
        var code = await ClassroomApi.GetInviteCodeAsync(owner, classId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await ClassroomApi.JoinAsync(student, code);
        var membershipId = await ClassroomApi.GetFirstMemberIdAsync(owner, classId, "Pending");

        var attacker = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var resp = await attacker.PostAsync(
            $"/api/teacher/classes/{classId}/members/{membershipId}/approve",
            null
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
