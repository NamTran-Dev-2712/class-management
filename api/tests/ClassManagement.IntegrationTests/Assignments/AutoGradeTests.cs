using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Exams;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

public class AutoGradeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<(
        HttpClient Student,
        string AttemptId,
        JsonElement Taking
    )> SetupAttemptAsync(string questionType, decimal point = 4m)
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var qId = await QuestionsApi.CreateAsync(teacher, subjectId, questionType);
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(qId, point)]);
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId)
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        return (student, attemptId, taking);
    }

    private static string OptionIdByContent(JsonElement taking, string content)
    {
        foreach (
            var o in taking.GetProperty("questions")[0].GetProperty("options").EnumerateArray()
        )
        {
            if (o.GetProperty("content").GetString() == content)
                return o.GetProperty("publicId").GetString()!;
        }
        throw new InvalidOperationException($"Option '{content}' not found.");
    }

    private static string QuestionId(JsonElement taking) =>
        taking.GetProperty("questions")[0].GetProperty("publicId").GetString()!;

    [Fact]
    public async Task MultipleChoice_AllCorrectSelected_FullScore()
    {
        var (student, attemptId, taking) = await SetupAttemptAsync("MultipleChoice");
        var questionId = QuestionId(taking);
        // Default MultipleChoice: Alpha + Beta correct, Gamma wrong.
        var alpha = OptionIdByContent(taking, "Alpha");
        var beta = OptionIdByContent(taking, "Beta");

        await AssignmentsApi.SaveAnswersRawAsync(
            student,
            attemptId,
            new[] { new { questionId, selectedOptionIds = new[] { alpha, beta } } }
        );
        await AssignmentsApi.SubmitRawAsync(student, attemptId);

        var result = await AssignmentsApi.GetResultAsync(student, attemptId);
        result.GetProperty("totalScore").GetDecimal().Should().Be(4m);
    }

    [Fact]
    public async Task MultipleChoice_PartialSelection_ZeroScore()
    {
        var (student, attemptId, taking) = await SetupAttemptAsync("MultipleChoice");
        var questionId = QuestionId(taking);
        var alpha = OptionIdByContent(taking, "Alpha"); // missing Beta → all-or-nothing → 0

        await AssignmentsApi.SaveAnswersRawAsync(
            student,
            attemptId,
            new[] { new { questionId, selectedOptionIds = new[] { alpha } } }
        );
        await AssignmentsApi.SubmitRawAsync(student, attemptId);

        var result = await AssignmentsApi.GetResultAsync(student, attemptId);
        result.GetProperty("totalScore").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task WritingQuestion_RoutesToNeedManualGrading()
    {
        var (student, attemptId, taking) = await SetupAttemptAsync("LongWriting");
        var questionId = QuestionId(taking);

        await AssignmentsApi.SaveAnswersRawAsync(
            student,
            attemptId,
            new[] { new { questionId, textAnswer = "My essay answer." } }
        );
        await AssignmentsApi.SubmitRawAsync(student, attemptId);

        var result = await AssignmentsApi.GetResultAsync(student, attemptId);
        result.GetProperty("status").GetString().Should().Be("NeedManualGrading");
    }
}
