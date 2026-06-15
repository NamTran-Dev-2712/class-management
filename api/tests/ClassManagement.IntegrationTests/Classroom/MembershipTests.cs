namespace ClassManagement.IntegrationTests.Classroom;

public class MembershipTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<(HttpClient Teacher, string ClassId, string InviteCode)> SeedClassAsync()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var code = await ClassroomApi.GetInviteCodeAsync(teacher, classId);
        return (teacher, classId, code);
    }

    [Fact]
    public async Task Join_ThenApprove_StudentSeesClassInTheirList()
    {
        var (teacher, classId, code) = await SeedClassAsync();
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        (await ClassroomApi.JoinAsync(student, code)).StatusCode.Should().Be(HttpStatusCode.OK);

        var membershipId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        var approve = await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{membershipId}/approve",
            null
        );
        approve.StatusCode.Should().Be(HttpStatusCode.OK);

        var myClasses = await AuthHelper.ReadDataAsync(
            await student.GetAsync("/api/student/classes")
        );
        myClasses.GetProperty("totalCount").GetInt32().Should().Be(1);
        myClasses
            .GetProperty("items")[0]
            .GetProperty("classPublicId")
            .GetString()
            .Should()
            .Be(classId);
    }

    [Fact]
    public async Task Join_Twice_WhilePending_Returns409()
    {
        var (_, _, code) = await SeedClassAsync();
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        (await ClassroomApi.JoinAsync(student, code)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ClassroomApi.JoinAsync(student, code))
            .StatusCode.Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reject_ThenStudentCanRetry()
    {
        var (teacher, classId, code) = await SeedClassAsync();
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        await ClassroomApi.JoinAsync(student, code);
        var membershipId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");

        var reject = await teacher.PostAsJsonAsync(
            $"/api/teacher/classes/{classId}/members/{membershipId}/reject",
            new { rejectionReason = "Not this term" }
        );
        reject.StatusCode.Should().Be(HttpStatusCode.OK);

        var requests = await AuthHelper.ReadDataAsync(
            await student.GetAsync("/api/student/classes/requests")
        );
        requests.GetProperty("items")[0].GetProperty("status").GetString().Should().Be("Rejected");

        // Rejected students may re-join (a new Pending row).
        (await ClassroomApi.JoinAsync(student, code))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Kick_RemovesStudentFromRoster()
    {
        var (teacher, classId, code) = await SeedClassAsync();
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        await ClassroomApi.JoinAsync(student, code);
        var pendingId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{pendingId}/approve",
            null
        );

        var approvedId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Approved");
        var kick = await teacher.DeleteAsync(
            $"/api/teacher/classes/{classId}/members/{approvedId}"
        );
        kick.StatusCode.Should().Be(HttpStatusCode.OK);

        var roster = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/classes/{classId}/members?status=Approved")
        );
        roster.GetProperty("totalCount").GetInt32().Should().Be(0);

        var myClasses = await AuthHelper.ReadDataAsync(
            await student.GetAsync("/api/student/classes")
        );
        myClasses.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Leave_AsApprovedStudent_RemovesFromClass()
    {
        var (teacher, classId, code) = await SeedClassAsync();
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        await ClassroomApi.JoinAsync(student, code);
        var pendingId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{pendingId}/approve",
            null
        );

        var leave = await student.PostAsync($"/api/student/classes/{classId}/leave", null);
        leave.StatusCode.Should().Be(HttpStatusCode.OK);

        var myClasses = await AuthHelper.ReadDataAsync(
            await student.GetAsync("/api/student/classes")
        );
        myClasses.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Join_ArchivedClass_Returns400()
    {
        var (teacher, classId, code) = await SeedClassAsync();
        await teacher.PostAsync($"/api/teacher/classes/{classId}/archive", null);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(student, code))
            .StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StudentRoster_OnlyApprovedMemberCanView()
    {
        var (teacher, classId, code) = await SeedClassAsync();
        var member = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await ClassroomApi.JoinAsync(member, code);
        var pendingId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{pendingId}/approve",
            null
        );

        var memberView = await member.GetAsync($"/api/student/classes/{classId}/members");
        memberView.StatusCode.Should().Be(HttpStatusCode.OK);

        var outsider = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var outsiderView = await outsider.GetAsync($"/api/student/classes/{classId}/members");
        outsiderView.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
