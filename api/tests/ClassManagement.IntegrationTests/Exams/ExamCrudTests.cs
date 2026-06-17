namespace ClassManagement.IntegrationTests.Exams;

public class ExamCrudTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Create_AsTeacher_WithSubject_Returns201()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);

        var id = await ExamsApi.CreateAsync(teacher, subjectId, "Midterm");

        var data = await ExamsApi.GetAsync(teacher, id);
        data.GetProperty("title").GetString().Should().Be("Midterm");
        data.GetProperty("version").GetInt32().Should().Be(1);
        data.GetProperty("totalQuestions").GetInt32().Should().Be(0);
        data.GetProperty("totalPoint").GetDecimal().Should().Be(0);
        data.GetProperty("subjectName").GetString().Should().NotBeNull();
    }

    [Fact]
    public async Task Create_AsTeacher_WithoutSubject_Returns201()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var id = await ExamsApi.CreateAsync(teacher);

        var data = await ExamsApi.GetAsync(teacher, id);
        // Null properties are omitted from the JSON envelope, so absent == no subject.
        var hasSubject =
            data.TryGetProperty("subjectPublicId", out var subj)
            && subj.ValueKind != JsonValueKind.Null;
        hasSubject.Should().BeFalse();
    }

    [Fact]
    public async Task Create_WithShortTitle_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var resp = await ExamsApi.CreateRawAsync(teacher, ExamsApi.BuildPayload(title: "ab"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Metadata_DoesNotBumpVersion()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var id = await ExamsApi.CreateAsync(teacher, title: "Original");

        var resp = await teacher.PutAsJsonAsync(
            $"/api/teacher/exams/{id}",
            new
            {
                subjectId = (string?)null,
                title = "Renamed",
                description = "Updated description",
                visibility = "Private",
            }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await ExamsApi.GetAsync(teacher, id);
        data.GetProperty("title").GetString().Should().Be("Renamed");
        data.GetProperty("description").GetString().Should().Be("Updated description");
        data.GetProperty("version").GetInt32().Should().Be(1, "metadata edits do not bump version");
    }

    [Fact]
    public async Task Delete_NotInUse_SoftDeletesAndDisappearsFromList()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var id = await ExamsApi.CreateAsync(teacher, title: "ToDelete");

        var del = await teacher.DeleteAsync($"/api/teacher/exams/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await teacher.GetAsync($"/api/teacher/exams/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnExams()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        await ExamsApi.CreateAsync(teacherA, title: "A-Exam");

        var listB = await ExamsApi.ListAsync(teacherB, "?searchTerm=A-Exam");
        listB.GetProperty("items").GetArrayLength().Should().Be(0);
    }
}
