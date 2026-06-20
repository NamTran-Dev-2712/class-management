using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Exams;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

// MVP-6 reports & export: submission counts + score stats over a class, the score_policy=highest rule,
// and the server-generated CSV (Scenarios 3 & 4).
public class AssignmentReportTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<(
        HttpClient Teacher,
        string AssignmentId,
        string ClassId
    )> SetupSingleChoiceAsync(int maxAttempts = 1, string scorePolicy = "Highest")
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var singleId = await QuestionsApi.CreateAsync(teacher, subjectId, "SingleChoice");
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(singleId, 4m)]);
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(
                examId,
                classId,
                maxAttempts: maxAttempts,
                scorePolicy: scorePolicy
            )
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);
        return (teacher, assignmentId, classId);
    }

    private async Task<HttpClient> ApprovedStudentAsync(HttpClient teacher, string classId)
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);
        return student;
    }

    // Starts + submits one attempt answering the SingleChoice correctly (4) or wrong (0).
    private static async Task SubmitSingleChoiceAsync(
        HttpClient student,
        string assignmentId,
        bool correct
    )
    {
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        var q = taking.GetProperty("questions")[0];
        var pick = q.GetProperty("options")
            .EnumerateArray()
            .First(o => o.GetProperty("content").GetString() == (correct ? "Alpha" : "Beta"))
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
                    selectedOptionIds = new[] { pick },
                },
            }
        );
        (await AssignmentsApi.SubmitRawAsync(student, attemptId)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Report_ReturnsSubmissionCountsAndScoreStats()
    {
        var (teacher, assignmentId, classId) = await SetupSingleChoiceAsync();
        var s1 = await ApprovedStudentAsync(teacher, classId);
        var s2 = await ApprovedStudentAsync(teacher, classId);
        await ApprovedStudentAsync(teacher, classId); // s3 never submits

        await SubmitSingleChoiceAsync(s1, assignmentId, correct: true); // 4
        await SubmitSingleChoiceAsync(s2, assignmentId, correct: false); // 0

        var report = await AssignmentsApi.GetReportAsync(teacher, assignmentId);

        report.GetProperty("totalStudents").GetInt32().Should().Be(3);
        report.GetProperty("submittedCount").GetInt32().Should().Be(2);
        report.GetProperty("notSubmittedCount").GetInt32().Should().Be(1);
        report.GetProperty("gradedCount").GetInt32().Should().Be(2);
        report.GetProperty("averageScore").GetDecimal().Should().Be(2m);
        report.GetProperty("minScore").GetDecimal().Should().Be(0m);
        report.GetProperty("maxScore").GetDecimal().Should().Be(4m);
        report.GetProperty("students").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Report_ScorePolicyHighest_UsesMaxAcrossAttempts()
    {
        var (teacher, assignmentId, classId) = await SetupSingleChoiceAsync(
            maxAttempts: 2,
            scorePolicy: "Highest"
        );
        var student = await ApprovedStudentAsync(teacher, classId);

        await SubmitSingleChoiceAsync(student, assignmentId, correct: false); // attempt 1 → 0
        await SubmitSingleChoiceAsync(student, assignmentId, correct: true); // attempt 2 → 4

        var report = await AssignmentsApi.GetReportAsync(teacher, assignmentId);

        report.GetProperty("maxScore").GetDecimal().Should().Be(4m);
        var row = report.GetProperty("students")[0];
        row.GetProperty("effectiveScore").GetDecimal().Should().Be(4m);
        row.GetProperty("attemptCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task ExportCsv_ReturnsFileWithRows()
    {
        var (teacher, assignmentId, classId) = await SetupSingleChoiceAsync();
        var s1 = await ApprovedStudentAsync(teacher, classId);
        await ApprovedStudentAsync(teacher, classId); // not submitted
        await SubmitSingleChoiceAsync(s1, assignmentId, correct: true);

        var resp = await AssignmentsApi.ExportRawAsync(teacher, assignmentId);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var csv = await resp.Content.ReadAsStringAsync();
        // Header + 2 student rows.
        csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length.Should().Be(3);
    }

    [Fact]
    public async Task Report_ByNonOwner_Returns403()
    {
        var (_, assignmentId, _) = await SetupSingleChoiceAsync();
        var otherTeacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var resp = await otherTeacher.GetAsync($"/api/teacher/assignments/{assignmentId}/report");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
