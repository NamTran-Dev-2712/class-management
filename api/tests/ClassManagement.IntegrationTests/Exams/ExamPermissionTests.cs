namespace ClassManagement.IntegrationTests.Exams;

public class ExamPermissionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Student_CannotCreateExam_Returns403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        var resp = await ExamsApi.CreateRawAsync(student, ExamsApi.BuildPayload());

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OtherTeacher_CannotUpdate_Returns403Or404()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var id = await ExamsApi.CreateAsync(teacherA);

        var resp = await teacherB.PutAsJsonAsync(
            $"/api/teacher/exams/{id}",
            new
            {
                subjectId = (string?)null,
                title = "Hijacked",
                description = (string?)null,
                visibility = "Private",
            }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OtherTeacher_CannotDelete_Returns403()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var id = await ExamsApi.CreateAsync(teacherA);

        var resp = await teacherB.DeleteAsync($"/api/teacher/exams/{id}");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OtherTeacher_CannotViewPrivateDetail_Returns403()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var id = await ExamsApi.CreateAsync(teacherA);

        var resp = await teacherB.GetAsync($"/api/teacher/exams/{id}");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CanViewAnyExam()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var id = await ExamsApi.CreateAsync(teacher);

        var resp = await admin.GetAsync($"/api/admin/exams/{id}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Teacher_CannotAccessAdminEndpoint_Returns403()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var resp = await teacher.GetAsync("/api/admin/exams");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
