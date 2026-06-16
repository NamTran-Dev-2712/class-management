namespace ClassManagement.IntegrationTests.Questions;

public class QuestionPermissionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Student_CannotAccessQuestionBank_Returns403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        var list = await student.GetAsync("/api/teacher/questions");
        list.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var create = await student.PostAsJsonAsync(
            "/api/teacher/questions",
            new { content = "anything at all here" }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Teacher_CannotEditOrDeleteAnotherTeachersQuestion()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacherA);
        var id = await QuestionsApi.CreateAsync(teacherA, subjectId, "SingleChoice");

        var update = await teacherB.PutAsJsonAsync(
            $"/api/teacher/questions/{id}",
            QuestionsApi.BuildPayload(subjectId, "SingleChoice")
        );
        update.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var del = await teacherB.DeleteAsync($"/api/teacher/questions/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var detail = await teacherB.GetAsync($"/api/teacher/questions/{id}");
        detail.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CanListAllQuestions_IncludingPrivate()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var content = QuestionsApi.UniqueContent();
        await QuestionsApi.CreateAsync(
            teacher,
            subjectId,
            "ShortWriting",
            content: content,
            visibility: "Private"
        );

        var resp = await admin.GetAsync(
            $"/api/admin/questions?searchTerm={Uri.EscapeDataString(content)}"
        );
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(resp);
        data.GetProperty("totalCount").GetInt32().Should().Be(1);
    }
}
