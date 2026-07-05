using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Exams;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

// Shared request helpers for the MVP-5 assignment + attempt endpoints. Builds the full chain
// (subject → questions → exam → class → approved student → assignment) used across the test classes.
internal static class AssignmentsApi
{
    public static string UniqueTitle() => $"Assignment {Guid.NewGuid():N}";

    // Creates an exam owned by `teacher` with one question per given type, each worth `point`.
    // Returns the exam publicId.
    public static async Task<string> CreateExamWithQuestionsAsync(
        HttpClient teacher,
        string subjectId,
        string[] types,
        decimal point = 2m
    )
    {
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var questions = new List<(string, decimal)>();
        foreach (var type in types)
        {
            var qId = await QuestionsApi.CreateAsync(teacher, subjectId, type);
            questions.Add((qId, point));
        }
        await ExamsApi.SaveQuestionsAsync(teacher, examId, questions);
        return examId;
    }

    public static object BuildPayload(
        string examId,
        string classId,
        string? title = null,
        DateTime? opensAt = null,
        DateTime? closesAt = null,
        int? timeLimitMinutes = null,
        int maxAttempts = 1,
        string scorePolicy = "Highest",
        bool allowLate = false,
        string gradePublishPolicy = "Immediate",
        bool shuffleQuestions = false,
        bool shuffleOptions = false,
        bool showAnswersAfterGrade = false,
        string? description = null,
        bool requireFullscreen = false,
        bool detectTabSwitch = false,
        bool blockCopyPaste = false,
        int maxViolations = 0,
        string violationAction = "WarnOnly"
    ) =>
        new
        {
            examId,
            classId,
            title = title ?? UniqueTitle(),
            description,
            opensAt,
            closesAt,
            timeLimitMinutes,
            maxAttempts,
            scorePolicy,
            allowLate,
            gradePublishPolicy,
            shuffleQuestions,
            shuffleOptions,
            showAnswersAfterGrade,
            requireFullscreen,
            detectTabSwitch,
            blockCopyPaste,
            maxViolations,
            violationAction,
        };

    public static Task<HttpResponseMessage> CreateRawAsync(HttpClient teacher, object payload) =>
        teacher.PostAsJsonAsync("/api/teacher/assignments", payload);

    public static async Task<string> CreateAsync(HttpClient teacher, object payload)
    {
        var resp = await CreateRawAsync(teacher, payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await AuthHelper.ReadDataAsync(resp);
        return data.GetProperty("publicId").GetString()!;
    }

    public static Task<HttpResponseMessage> PublishRawAsync(
        HttpClient teacher,
        string assignmentId
    ) => teacher.PostAsync($"/api/teacher/assignments/{assignmentId}/publish", null);

    public static async Task PublishAsync(HttpClient teacher, string assignmentId)
    {
        var resp = await PublishRawAsync(teacher, assignmentId);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public static async Task<JsonElement> GetTeacherAsync(HttpClient teacher, string assignmentId)
    {
        var resp = await teacher.GetAsync($"/api/teacher/assignments/{assignmentId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    // Approves `student` into the teacher's class (join by invite code → approve the pending request).
    public static async Task ApproveStudentAsync(
        HttpClient teacher,
        HttpClient student,
        string classId
    )
    {
        var code = await ClassroomApi.GetInviteCodeAsync(teacher, classId);
        var join = await ClassroomApi.JoinAsync(student, code);
        join.IsSuccessStatusCode.Should().BeTrue();
        var pendingId = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        var approve = await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{pendingId}/approve",
            null
        );
        approve.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public static Task<HttpResponseMessage> StartRawAsync(
        HttpClient student,
        string assignmentId
    ) => student.PostAsync($"/api/student/assignments/{assignmentId}/attempts", null);

    public static async Task<string> StartAsync(HttpClient student, string assignmentId)
    {
        var resp = await StartRawAsync(student, assignmentId);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(resp);
        return data.GetProperty("attemptId").GetString()!;
    }

    public static async Task<JsonElement> GetTakingAsync(HttpClient student, string attemptId)
    {
        var resp = await student.GetAsync($"/api/student/assignments/attempts/{attemptId}/taking");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static Task<HttpResponseMessage> SaveAnswersRawAsync(
        HttpClient student,
        string attemptId,
        object answers
    ) =>
        student.PatchAsJsonAsync(
            $"/api/student/assignments/attempts/{attemptId}/answers",
            new { answers }
        );

    public static Task<HttpResponseMessage> SubmitRawAsync(HttpClient student, string attemptId) =>
        student.PostAsync($"/api/student/assignments/attempts/{attemptId}/submit", null);

    public static async Task<JsonElement> GetResultAsync(HttpClient student, string attemptId)
    {
        var resp = await student.GetAsync($"/api/student/assignments/attempts/{attemptId}/result");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static Task<HttpResponseMessage> GetResultRawAsync(
        HttpClient student,
        string attemptId
    ) => student.GetAsync($"/api/student/assignments/attempts/{attemptId}/result");

    // ── MVP-10: proctoring events / moderation ───────────────────────────────────────────────

    public static Task<HttpResponseMessage> RecordEventsRawAsync(
        HttpClient student,
        string attemptId,
        object events
    ) =>
        student.PostAsJsonAsync(
            $"/api/student/assignments/attempts/{attemptId}/events",
            new { events }
        );

    // Sends `count` TabSwitch events in one batch and returns the parsed RecordEventsResultDto.
    public static async Task<JsonElement> RecordTabSwitchesAsync(
        HttpClient student,
        string attemptId,
        int count
    )
    {
        var events = Enumerable
            .Range(0, count)
            .Select(_ => new { eventType = "TabSwitch", occurredAt = (DateTime?)null })
            .ToArray();
        var resp = await RecordEventsRawAsync(student, attemptId, events);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static async Task<JsonElement> GetAttemptEventsAsync(
        HttpClient teacher,
        string attemptId
    )
    {
        var resp = await teacher.GetAsync($"/api/teacher/assignments/attempts/{attemptId}/events");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static Task<HttpResponseMessage> ForceSubmitRawAsync(
        HttpClient client,
        string attemptId,
        bool admin = false
    ) =>
        client.PostAsync(
            admin
                ? $"/api/admin/assignments/attempts/{attemptId}/force-submit"
                : $"/api/teacher/assignments/attempts/{attemptId}/force-submit",
            null
        );

    public static Task<HttpResponseMessage> FlagRawAsync(HttpClient teacher, string attemptId) =>
        teacher.PostAsync($"/api/teacher/assignments/attempts/{attemptId}/flag", null);

    public static Task<HttpResponseMessage> UnflagRawAsync(HttpClient teacher, string attemptId) =>
        teacher.PostAsync($"/api/teacher/assignments/attempts/{attemptId}/unflag", null);

    public static Task<HttpResponseMessage> UnlockRawAsync(HttpClient teacher, string attemptId) =>
        teacher.PostAsync($"/api/teacher/assignments/attempts/{attemptId}/unlock", null);

    // ── MVP-6: grading / reports ─────────────────────────────────────────────────────────────

    public static async Task<JsonElement> GetAttemptsAsync(HttpClient teacher, string assignmentId)
    {
        var resp = await teacher.GetAsync($"/api/teacher/assignments/{assignmentId}/attempts");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static async Task<JsonElement> GetGradingAsync(HttpClient teacher, string attemptId)
    {
        var resp = await teacher.GetAsync($"/api/teacher/assignments/attempts/{attemptId}/grading");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static Task<HttpResponseMessage> GradeRawAsync(
        HttpClient teacher,
        string attemptId,
        object grades
    ) =>
        teacher.PostAsJsonAsync(
            $"/api/teacher/assignments/attempts/{attemptId}/grade",
            new { grades }
        );

    public static Task<HttpResponseMessage> ReleaseGradesRawAsync(
        HttpClient teacher,
        string assignmentId
    ) => teacher.PostAsync($"/api/teacher/assignments/{assignmentId}/release-grades", null);

    public static async Task<JsonElement> GetReportAsync(HttpClient teacher, string assignmentId)
    {
        var resp = await teacher.GetAsync($"/api/teacher/assignments/{assignmentId}/report");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static Task<HttpResponseMessage> ExportRawAsync(
        HttpClient teacher,
        string assignmentId
    ) => teacher.GetAsync($"/api/teacher/assignments/{assignmentId}/export");

    // The first attempt's single writing question id (taking view exposes one question per attempt).
    public static string WritingQuestionId(JsonElement taking) =>
        taking.GetProperty("questions")[0].GetProperty("publicId").GetString()!;
}
