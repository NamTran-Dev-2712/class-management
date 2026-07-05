using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.IntegrationTests.Assignments;

// MVP-10 — browser-lockdown proctoring: config mapping, server-authoritative violation counting,
// threshold actions (AutoSubmit / LockAttempt), timeline reads + teacher/admin moderation.
public class ProctoringTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    // Builds a published assignment with the given proctoring config + an approved student.
    private async Task<(HttpClient Teacher, HttpClient Student, string AssignmentId)> SetupAsync(
        bool requireFullscreen = false,
        bool detectTabSwitch = false,
        bool blockCopyPaste = false,
        int maxViolations = 0,
        string violationAction = "WarnOnly"
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
                requireFullscreen: requireFullscreen,
                detectTabSwitch: detectTabSwitch,
                blockCopyPaste: blockCopyPaste,
                maxViolations: maxViolations,
                violationAction: violationAction
            )
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);
        return (teacher, student, assignmentId);
    }

    private static Guid AttemptGuid(string attemptId) => Guid.Parse(attemptId);

    [Fact]
    public async Task Create_WithProctoringConfig_PersistsAndReturnsInDetail()
    {
        var (teacher, _, assignmentId) = await SetupAsync(
            requireFullscreen: true,
            detectTabSwitch: true,
            blockCopyPaste: true,
            maxViolations: 3,
            violationAction: "AutoSubmit"
        );

        var detail = await AssignmentsApi.GetTeacherAsync(teacher, assignmentId);

        detail.GetProperty("requireFullscreen").GetBoolean().Should().BeTrue();
        detail.GetProperty("detectTabSwitch").GetBoolean().Should().BeTrue();
        detail.GetProperty("blockCopyPaste").GetBoolean().Should().BeTrue();
        detail.GetProperty("maxViolations").GetInt32().Should().Be(3);
        detail.GetProperty("violationAction").GetString().Should().Be("AutoSubmit");
    }

    [Fact]
    public async Task Create_DefaultsProctoringOff_BackwardCompatible()
    {
        var (teacher, student, assignmentId) = await SetupAsync();

        var detail = await AssignmentsApi.GetTeacherAsync(teacher, assignmentId);
        detail.GetProperty("requireFullscreen").GetBoolean().Should().BeFalse();
        detail.GetProperty("maxViolations").GetInt32().Should().Be(0);

        // The taking session reports proctoring disabled.
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        var proctoring = taking.GetProperty("proctoring");
        proctoring.GetProperty("requireFullscreen").GetBoolean().Should().BeFalse();
        taking.GetProperty("violationCount").GetInt32().Should().Be(0);
        taking.GetProperty("isLocked").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task TakingSession_ExposesProctoringConfig()
    {
        var (_, student, assignmentId) = await SetupAsync(
            requireFullscreen: true,
            detectTabSwitch: true,
            maxViolations: 5,
            violationAction: "AutoSubmit"
        );

        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        var proctoring = taking.GetProperty("proctoring");

        proctoring.GetProperty("requireFullscreen").GetBoolean().Should().BeTrue();
        proctoring.GetProperty("detectTabSwitch").GetBoolean().Should().BeTrue();
        proctoring.GetProperty("maxViolations").GetInt32().Should().Be(5);
        proctoring.GetProperty("violationAction").GetString().Should().Be("AutoSubmit");
    }

    [Fact]
    public async Task RecordEvents_IncrementsViolationCountServerSide()
    {
        var (teacher, student, assignmentId) = await SetupAsync(detectTabSwitch: true);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        var result = await AssignmentsApi.RecordTabSwitchesAsync(student, attemptId, 2);
        result.GetProperty("violationCount").GetInt32().Should().Be(2);
        result.GetProperty("submitted").GetBoolean().Should().BeFalse();

        // Counter survives across requests (server-authoritative).
        var again = await AssignmentsApi.RecordTabSwitchesAsync(student, attemptId, 1);
        again.GetProperty("violationCount").GetInt32().Should().Be(3);

        // Teacher roster surfaces the count.
        var attempts = await AssignmentsApi.GetAttemptsAsync(teacher, assignmentId);
        attempts.GetProperty("items")[0].GetProperty("violationCount").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task RecordEvents_WhenNotInProgress_Returns400()
    {
        var (_, student, assignmentId) = await SetupAsync(detectTabSwitch: true);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        (await AssignmentsApi.SubmitRawAsync(student, attemptId)).EnsureSuccessStatusCode();

        var resp = await AssignmentsApi.RecordEventsRawAsync(
            student,
            attemptId,
            new[] { new { eventType = "TabSwitch" } }
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordEvents_PastDeadline_Returns400()
    {
        var (_, student, assignmentId) = await SetupAsync(detectTabSwitch: true);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        // Force the deadline into the past (server time is authoritative).
        var guid = AttemptGuid(attemptId);
        await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            await db
                .Attempts.Where(a => a.PublicId == guid)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(a => a.DeadlineAt, DateTime.UtcNow.AddMinutes(-1))
                );
            return true;
        });

        var resp = await AssignmentsApi.RecordEventsRawAsync(
            student,
            attemptId,
            new[] { new { eventType = "TabSwitch" } }
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordEvents_ExceedThreshold_AutoSubmit_FinalizesAttempt()
    {
        var (_, student, assignmentId) = await SetupAsync(
            detectTabSwitch: true,
            maxViolations: 2,
            violationAction: "AutoSubmit"
        );
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        // 3 > 2 → auto-submit.
        var result = await AssignmentsApi.RecordTabSwitchesAsync(student, attemptId, 3);
        result.GetProperty("submitted").GetBoolean().Should().BeTrue();
        result.GetProperty("thresholdExceeded").GetBoolean().Should().BeTrue();

        var guid = AttemptGuid(attemptId);
        await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var attempt = await db.Attempts.SingleAsync(a => a.PublicId == guid);
            attempt.Status.Should().NotBe(AttemptStatus.InProgress);
            attempt.AutoSubmitted.Should().BeTrue();
            attempt.IsFlagged.Should().BeTrue();
            return true;
        });
    }

    [Fact]
    public async Task RecordEvents_ExceedThreshold_LockAttempt_LocksAndBlocks()
    {
        var (_, student, assignmentId) = await SetupAsync(
            detectTabSwitch: true,
            maxViolations: 1,
            violationAction: "LockAttempt"
        );
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        var result = await AssignmentsApi.RecordTabSwitchesAsync(student, attemptId, 2);
        result.GetProperty("locked").GetBoolean().Should().BeTrue();

        // A locked attempt rejects further events.
        var events = await AssignmentsApi.RecordEventsRawAsync(
            student,
            attemptId,
            new[] { new { eventType = "TabSwitch" } }
        );
        events.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // It stays InProgress (no new attempt status) but is locked in the DB.
        var guid = AttemptGuid(attemptId);
        await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var attempt = await db.Attempts.SingleAsync(a => a.PublicId == guid);
            attempt.IsLocked.Should().BeTrue();
            attempt.Status.Should().Be(AttemptStatus.InProgress);
            return true;
        });
    }

    [Fact]
    public async Task RecordEvents_MaxViolationsZero_NeverAutoSubmits()
    {
        var (_, student, assignmentId) = await SetupAsync(
            detectTabSwitch: true,
            maxViolations: 0,
            violationAction: "AutoSubmit"
        );
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        var result = await AssignmentsApi.RecordTabSwitchesAsync(student, attemptId, 10);
        result.GetProperty("violationCount").GetInt32().Should().Be(10);
        result.GetProperty("submitted").GetBoolean().Should().BeFalse();
        result.GetProperty("thresholdExceeded").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Teacher_GetAttemptEvents_ReturnsTimeline()
    {
        var (teacher, student, assignmentId) = await SetupAsync(detectTabSwitch: true);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        await AssignmentsApi.RecordTabSwitchesAsync(student, attemptId, 2);

        var events = await AssignmentsApi.GetAttemptEventsAsync(teacher, attemptId);
        events.GetArrayLength().Should().Be(2);
        events[0].GetProperty("eventType").GetString().Should().Be("TabSwitch");
    }

    [Fact]
    public async Task Student_CannotAccessTeacherTimeline()
    {
        var (_, student, assignmentId) = await SetupAsync(detectTabSwitch: true);
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        var resp = await student.GetAsync($"/api/teacher/assignments/attempts/{attemptId}/events");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceSubmit_AsOwner_SubmitsAttempt()
    {
        var (teacher, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        var resp = await AssignmentsApi.ForceSubmitRawAsync(teacher, attemptId);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var guid = AttemptGuid(attemptId);
        await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var attempt = await db.Attempts.SingleAsync(a => a.PublicId == guid);
            attempt.Status.Should().NotBe(AttemptStatus.InProgress);
            attempt.AutoSubmitted.Should().BeTrue();
            return true;
        });
    }

    [Fact]
    public async Task ForceSubmit_AsNonOwner_Returns403()
    {
        var (_, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var otherTeacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var resp = await AssignmentsApi.ForceSubmitRawAsync(otherTeacher, attemptId);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FlagAndUnflag_AsOwner_TogglesFlag()
    {
        var (teacher, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);

        (await AssignmentsApi.FlagRawAsync(teacher, attemptId))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var attempts = await AssignmentsApi.GetAttemptsAsync(teacher, assignmentId);
        attempts.GetProperty("items")[0].GetProperty("isFlagged").GetBoolean().Should().BeTrue();

        (await AssignmentsApi.UnflagRawAsync(teacher, attemptId))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);
        var after = await AssignmentsApi.GetAttemptsAsync(teacher, assignmentId);
        after.GetProperty("items")[0].GetProperty("isFlagged").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Unlock_AsOwner_ClearsLock()
    {
        var (teacher, student, assignmentId) = await SetupAsync(
            detectTabSwitch: true,
            maxViolations: 1,
            violationAction: "LockAttempt"
        );
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        await AssignmentsApi.RecordTabSwitchesAsync(student, attemptId, 2);

        (await AssignmentsApi.UnlockRawAsync(teacher, attemptId))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var guid = AttemptGuid(attemptId);
        await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var attempt = await db.Attempts.SingleAsync(a => a.PublicId == guid);
            attempt.IsLocked.Should().BeFalse();
            return true;
        });
    }

    [Fact]
    public async Task Admin_ForceSubmit_Works()
    {
        var (_, student, assignmentId) = await SetupAsync();
        var attemptId = await AssignmentsApi.StartAsync(student, assignmentId);
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        var resp = await AssignmentsApi.ForceSubmitRawAsync(admin, attemptId, admin: true);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
