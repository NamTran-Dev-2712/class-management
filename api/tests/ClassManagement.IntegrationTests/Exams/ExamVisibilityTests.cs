namespace ClassManagement.IntegrationTests.Exams;

public class ExamVisibilityTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task SetPublic_MakesExamVisibleInPublicPoolToOtherTeachers()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var title = ExamsApi.UniqueTitle();
        var id = await ExamsApi.CreateAsync(teacherA, title: title);

        var patch = await teacherA.PatchAsJsonAsync(
            $"/api/teacher/exams/{id}/visibility",
            new { visibility = "Public" }
        );
        patch.StatusCode.Should().Be(HttpStatusCode.OK);

        var publicResp = await teacherB.GetAsync(
            $"/api/teacher/exams/public?searchTerm={Uri.EscapeDataString(title)}"
        );
        publicResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(publicResp);
        data.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task PrivateExam_NotVisibleToOtherTeacherDetail()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var id = await ExamsApi.CreateAsync(teacherA);

        var resp = await teacherB.GetAsync($"/api/teacher/exams/public/{id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
