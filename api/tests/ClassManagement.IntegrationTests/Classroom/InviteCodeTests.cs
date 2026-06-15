namespace ClassManagement.IntegrationTests.Classroom;

public class InviteCodeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Regenerate_InvalidatesOldCode_NewCodeWorks()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var oldCode = await ClassroomApi.GetInviteCodeAsync(teacher, classId);

        var regen = await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/invite-code/regenerate",
            null
        );
        regen.StatusCode.Should().Be(HttpStatusCode.OK);
        var newCode = (await AuthHelper.ReadDataAsync(regen))
            .GetProperty("inviteCode")
            .GetString()!;
        newCode.Should().NotBe(oldCode);

        var staleJoiner = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(staleJoiner, oldCode))
            .StatusCode.Should()
            .Be(HttpStatusCode.NotFound);

        var freshJoiner = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(freshJoiner, newCode))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Join_WithUnknownCode_Returns404()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(student, "ZZ99ZZ99"))
            .StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Join_WithMalformedCode_Returns400()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(student, "!!"))
            .StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }
}
