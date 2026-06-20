using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Exams;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

// MVP-6 grade publishing: the Manual policy hides scores from the student until the teacher publishes;
// Immediate shows them on submit; a student can never read another student's result (BR-6-04/05).
public class GradePublishingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<(
        HttpClient Teacher,
        HttpClient Student,
        string AssignmentId,
        string AttemptId,
        string ClassId,
        string WritingQuestionId
    )> SubmitManualAsync(string gradePublishPolicy)
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var writingId = await QuestionsApi.CreateAsync(teacher, subjectId, "LongWriting");
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(writingId, 5m)]);

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
        var writingQuestionId = AssignmentsApi.WritingQuestionId(taking);

        await AssignmentsApi.SaveAnswersRawAsync(
            student,
            attemptId,
            new[] { new { questionId = writingQuestionId, textAnswer = "Essay." } }
        );
        (await AssignmentsApi.SubmitRawAsync(student, attemptId)).EnsureSuccessStatusCode();

        return (teacher, student, assignmentId, attemptId, classId, writingQuestionId);
    }

    [Fact]
    public async Task ManualPolicy_HidesScoreUntilTeacherPublishes()
    {
        var s = await SubmitManualAsync("Manual");

        // Teacher grades the writing answer → attempt becomes Graded.
        (
            await AssignmentsApi.GradeRawAsync(
                s.Teacher,
                s.AttemptId,
                new[]
                {
                    new
                    {
                        questionPublicId = s.WritingQuestionId,
                        score = 5m,
                        feedback = "Nice.",
                    },
                }
            )
        ).EnsureSuccessStatusCode();

        // Before publish: the student must not see the score (null is omitted from the JSON).
        var before = await AssignmentsApi.GetResultAsync(s.Student, s.AttemptId);
        before.GetProperty("scoreReleased").GetBoolean().Should().BeFalse();
        var hasScore =
            before.TryGetProperty("totalScore", out var scoreEl)
            && scoreEl.ValueKind != JsonValueKind.Null;
        hasScore.Should().BeFalse();

        // Teacher publishes grades.
        (
            await AssignmentsApi.ReleaseGradesRawAsync(s.Teacher, s.AssignmentId)
        ).EnsureSuccessStatusCode();

        // After publish: the score is visible.
        var after = await AssignmentsApi.GetResultAsync(s.Student, s.AttemptId);
        after.GetProperty("scoreReleased").GetBoolean().Should().BeTrue();
        after.GetProperty("totalScore").GetDecimal().Should().Be(5m);
    }

    [Fact]
    public async Task ImmediatePolicy_ShowsAutoScoreOnSubmit()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var singleId = await QuestionsApi.CreateAsync(teacher, subjectId, "SingleChoice");
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(singleId, 4m)]);
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId, gradePublishPolicy: "Immediate")
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        var q = taking.GetProperty("questions")[0];
        var alpha = q.GetProperty("options")
            .EnumerateArray()
            .First(o => o.GetProperty("content").GetString() == "Alpha")
            .GetProperty("publicId")
            .GetString()!;
        await AssignmentsApi.SaveAnswersRawAsync(
            student,
            attemptId,
            new[]
            {
                new
                {
                    questionId = q.GetProperty("publicId").GetString(),
                    selectedOptionIds = new[] { alpha },
                },
            }
        );
        (await AssignmentsApi.SubmitRawAsync(student, attemptId)).EnsureSuccessStatusCode();

        var result = await AssignmentsApi.GetResultAsync(student, attemptId);
        result.GetProperty("scoreReleased").GetBoolean().Should().BeTrue();
        result.GetProperty("totalScore").GetDecimal().Should().Be(4m);
    }

    [Fact]
    public async Task StudentCannotViewAnotherStudentsResult_Returns403()
    {
        var s = await SubmitManualAsync("Immediate");
        var otherStudent = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        var resp = await AssignmentsApi.GetResultRawAsync(otherStudent, s.AttemptId);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
