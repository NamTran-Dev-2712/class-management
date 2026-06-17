namespace ClassManagement.IntegrationTests.Exams;

public class ExamDuplicateTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Duplicate_PublicExam_CreatesIndependentPrivateCopy()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacherA);

        var sourceId = await ExamsApi.CreateAsync(
            teacherA,
            subjectId,
            "Shared Exam",
            visibility: "Public"
        );
        var q1 = await ExamsApi.CreateQuestionAsync(teacherA, subjectId, "Public");
        await ExamsApi.SaveQuestionsAsync(teacherA, sourceId, [(q1, 2m)]);

        // Teacher B duplicates the public exam.
        var dupResp = await teacherB.PostAsync($"/api/teacher/exams/{sourceId}/duplicate", null);
        dupResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var copyId = (await AuthHelper.ReadDataAsync(dupResp)).GetProperty("publicId").GetString()!;

        var copy = await ExamsApi.GetAsync(teacherB, copyId);
        copy.GetProperty("visibility").GetString().Should().Be("Private");
        copy.GetProperty("version").GetInt32().Should().Be(1);
        copy.GetProperty("totalQuestions").GetInt32().Should().Be(1);
        copy.GetProperty("totalPoint").GetDecimal().Should().Be(2m);

        // Editing the copy must not affect the original.
        await ExamsApi.SaveQuestionsAsync(teacherB, copyId, []);
        var source = await ExamsApi.GetAsync(teacherA, sourceId);
        source.GetProperty("totalQuestions").GetInt32().Should().Be(1, "original is independent");
    }

    [Fact]
    public async Task Duplicate_OtherTeachersPrivateExam_Returns404()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var id = await ExamsApi.CreateAsync(teacherA);

        var resp = await teacherB.PostAsync($"/api/teacher/exams/{id}/duplicate", null);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
