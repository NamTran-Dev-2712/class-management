using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Exams;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

// MVP-6 manual grading: teacher scores writing answers; the attempt finalizes to Graded once every
// writing question has a grade, with total = auto + manual (Scenario 1, BR-6-01..03).
public class ManualGradingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private sealed record Setup(
        HttpClient Teacher,
        HttpClient Student,
        string AssignmentId,
        string AttemptId,
        string WritingQuestionId,
        decimal AutoScore
    );

    // Builds an assignment with one SingleChoice (4pt, answered correctly) + one LongWriting (5pt) and
    // submits it, leaving the attempt in NeedManualGrading with total_auto_score = 4.
    private async Task<Setup> SubmitWritingAttemptAsync(string gradePublishPolicy = "Immediate")
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var singleId = await QuestionsApi.CreateAsync(teacher, subjectId, "SingleChoice");
        var writingId = await QuestionsApi.CreateAsync(teacher, subjectId, "LongWriting");
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(singleId, 4m), (writingId, 5m)]);

        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId, gradePublishPolicy: gradePublishPolicy)
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);

        var answers = new List<object>();
        string writingQuestionId = string.Empty;
        foreach (var q in taking.GetProperty("questions").EnumerateArray())
        {
            var qId = q.GetProperty("publicId").GetString()!;
            var type = q.GetProperty("type").GetString();
            if (type is "ShortWriting" or "LongWriting")
            {
                writingQuestionId = qId;
                answers.Add(new { questionId = qId, textAnswer = "My essay answer." });
            }
            else
            {
                var alpha = q.GetProperty("options")
                    .EnumerateArray()
                    .First(o => o.GetProperty("content").GetString() == "Alpha")
                    .GetProperty("publicId")
                    .GetString()!;
                answers.Add(new { questionId = qId, selectedOptionIds = new[] { alpha } });
            }
        }

        await AssignmentsApi.SaveAnswersRawAsync(student, attemptId, answers);
        (await AssignmentsApi.SubmitRawAsync(student, attemptId)).EnsureSuccessStatusCode();

        return new Setup(teacher, student, assignmentId, attemptId, writingQuestionId, 4m);
    }

    [Fact]
    public async Task GradeAllWritingAnswers_TransitionsToGraded_WithTotal()
    {
        var s = await SubmitWritingAttemptAsync();

        var grading = await AssignmentsApi.GetGradingAsync(s.Teacher, s.AttemptId);
        grading.GetProperty("status").GetString().Should().Be("NeedManualGrading");

        var resp = await AssignmentsApi.GradeRawAsync(
            s.Teacher,
            s.AttemptId,
            new[]
            {
                new
                {
                    questionPublicId = s.WritingQuestionId,
                    score = 4m,
                    feedback = "Good.",
                },
            }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await AssignmentsApi.GetGradingAsync(s.Teacher, s.AttemptId);
        detail.GetProperty("status").GetString().Should().Be("Graded");
        detail.GetProperty("totalScore").GetDecimal().Should().Be(8m); // 4 auto + 4 manual
        detail.GetProperty("totalManualScore").GetDecimal().Should().Be(4m);
    }

    [Fact]
    public async Task GradeWriting_ScoreAboveMax_Returns400()
    {
        var s = await SubmitWritingAttemptAsync();

        var resp = await AssignmentsApi.GradeRawAsync(
            s.Teacher,
            s.AttemptId,
            new[]
            {
                new
                {
                    questionPublicId = s.WritingQuestionId,
                    score = 99m,
                    feedback = (string?)null,
                },
            }
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EditGradeAfterGraded_RecomputesTotalImmediately()
    {
        var s = await SubmitWritingAttemptAsync();

        await AssignmentsApi.GradeRawAsync(
            s.Teacher,
            s.AttemptId,
            new[]
            {
                new
                {
                    questionPublicId = s.WritingQuestionId,
                    score = 5m,
                    feedback = "Full.",
                },
            }
        );
        await AssignmentsApi.GradeRawAsync(
            s.Teacher,
            s.AttemptId,
            new[]
            {
                new
                {
                    questionPublicId = s.WritingQuestionId,
                    score = 2m,
                    feedback = "Revised.",
                },
            }
        );

        var detail = await AssignmentsApi.GetGradingAsync(s.Teacher, s.AttemptId);
        detail.GetProperty("status").GetString().Should().Be("Graded");
        detail.GetProperty("totalScore").GetDecimal().Should().Be(6m); // 4 auto + 2 manual
    }

    [Fact]
    public async Task GradeAttempt_ByNonOwner_Returns403()
    {
        var s = await SubmitWritingAttemptAsync();
        var otherTeacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var resp = await AssignmentsApi.GradeRawAsync(
            otherTeacher,
            s.AttemptId,
            new[]
            {
                new
                {
                    questionPublicId = s.WritingQuestionId,
                    score = 3m,
                    feedback = (string?)null,
                },
            }
        );

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
