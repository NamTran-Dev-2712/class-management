using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

public class AttemptTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    // Builds: teacher with a published assignment (single SingleChoice question, 2 pts) + an approved
    // student. Returns the clients and the assignment id.
    private async Task<(HttpClient Teacher, HttpClient Student, string AssignmentId)> SetupAsync(
        int maxAttempts = 1,
        string gradePublishPolicy = "Immediate",
        int? timeLimitMinutes = null
    )
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await AssignmentsApi.CreateExamWithQuestionsAsync(
            teacher,
            subjectId,
            ["SingleChoice"]
        );
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(
                examId,
                classId,
                maxAttempts: maxAttempts,
                gradePublishPolicy: gradePublishPolicy,
                timeLimitMinutes: timeLimitMinutes
            )
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);
        return (teacher, student, assignmentId);
    }

    // The default SingleChoice question marks the option with content "Alpha" as correct.
    private static (string QuestionId, string OptionId) CorrectSingleChoice(JsonElement taking)
    {
        var q = taking.GetProperty("questions")[0];
        var qid = q.GetProperty("publicId").GetString()!;
        foreach (var o in q.GetProperty("options").EnumerateArray())
        {
            if (o.GetProperty("content").GetString() == "Alpha")
                return (qid, o.GetProperty("publicId").GetString()!);
        }
        throw new InvalidOperationException("Correct option not found.");
    }

    [Fact]
    public async Task Start_AsApprovedStudent_ReturnsTakingWithQuestions()
    {
        var (_, student, assignmentId) = await SetupAsync();

        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);

        taking.GetProperty("status").GetString().Should().Be("InProgress");
        taking.GetProperty("questions").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task Start_AsNonMember_Returns403()
    {
        var (_, _, assignmentId) = await SetupAsync();
        var outsider = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        var resp = await AssignmentsApi.StartRawAsync(outsider, assignmentId);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Start_WhenAttemptInProgress_ReturnsSameAttempt()
    {
        var (_, student, assignmentId) = await SetupAsync(maxAttempts: 3);

        var first = await AssignmentsApi.StartAsync(student, assignmentId);
        var second = await AssignmentsApi.StartAsync(student, assignmentId);

        second.Should().Be(first);
    }

    [Fact]
    public async Task AutoSave_PersistsAcrossReload()
    {
        var (_, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        var (questionId, optionId) = CorrectSingleChoice(taking);

        var save = await AssignmentsApi.SaveAnswersRawAsync(
            student,
            attemptId,
            new[]
            {
                new
                {
                    questionId,
                    selectedOptionIds = new[] { optionId },
                    textAnswer = (string?)null,
                },
            }
        );
        save.StatusCode.Should().Be(HttpStatusCode.OK);

        // Reload: the draft answer is still there.
        var reloaded = await AssignmentsApi.GetTakingAsync(student, attemptId);
        var selected = reloaded.GetProperty("questions")[0].GetProperty("selectedOptionIds");
        selected.GetArrayLength().Should().Be(1);
        selected[0].GetString().Should().Be(optionId);
    }

    [Fact]
    public async Task Submit_AutoGradesSingleChoice_AndReleasesScore()
    {
        var (_, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        var (questionId, optionId) = CorrectSingleChoice(taking);

        await AssignmentsApi.SaveAnswersRawAsync(
            student,
            attemptId,
            new[] { new { questionId, selectedOptionIds = new[] { optionId } } }
        );

        var submit = await AssignmentsApi.SubmitRawAsync(student, attemptId);
        submit.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await AssignmentsApi.GetResultAsync(student, attemptId);
        result.GetProperty("status").GetString().Should().Be("AutoGraded");
        result.GetProperty("scoreReleased").GetBoolean().Should().BeTrue();
        result.GetProperty("totalScore").GetDecimal().Should().Be(2m);
    }

    [Fact]
    public async Task Submit_Twice_Returns400()
    {
        var (_, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        var first = await AssignmentsApi.SubmitRawAsync(student, attemptId);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await AssignmentsApi.SubmitRawAsync(student, attemptId);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MaxAttempts_BlocksStartAfterLimitReached()
    {
        var (_, student, assignmentId) = await SetupAsync(maxAttempts: 1);

        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        await AssignmentsApi.SubmitRawAsync(student, attemptId);

        var startAgain = await AssignmentsApi.StartRawAsync(student, assignmentId);
        startAgain.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnansweredSingleChoice_ScoresZero()
    {
        var (_, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        // Submit without answering.
        var submit = await AssignmentsApi.SubmitRawAsync(student, attemptId);
        submit.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await AssignmentsApi.GetResultAsync(student, attemptId);
        result.GetProperty("status").GetString().Should().Be("AutoGraded");
        result.GetProperty("totalScore").GetDecimal().Should().Be(0m);
    }
}
