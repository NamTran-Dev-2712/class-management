using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Exams;

// BR-3-08 / BR-4-05: a question used in one of the teacher's own exams cannot be soft-deleted until
// it is removed from those exams.
public class QuestionDeleteInUseTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Delete_QuestionUsedInOwnExam_Returns409()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var questionId = await QuestionsApi.CreateAsync(teacher, subjectId);
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(questionId, 1m)]);

        var resp = await teacher.DeleteAsync($"/api/teacher/questions/{questionId}");

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_QuestionAfterRemovedFromExam_Succeeds()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var questionId = await QuestionsApi.CreateAsync(teacher, subjectId);
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(questionId, 1m)]);

        // Remove it from the exam, then deletion is allowed.
        await ExamsApi.SaveQuestionsAsync(teacher, examId, []);

        var resp = await teacher.DeleteAsync($"/api/teacher/questions/{questionId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
