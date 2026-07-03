using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.IntegrationTests.Assignments;
using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Exams;
using ClassManagement.IntegrationTests.Questions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.IntegrationTests.Media;

// MVP-9: publishing an assignment freezes referenced media into snapshot_media, and the cleanup job never
// removes a soft-deleted asset that a snapshot still pins (BR-9-06 / BR-9-08).
public class MediaSnapshotFreezeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private Task<HttpClient> TeacherAsync() =>
        AuthHelper.CreateRoleClientAsync(_factory, ApplicationRoles.Teacher);

    private Task<int> RunCleanupAsync(int pendingTtlMinutes = -1) =>
        _factory.WithScopeAsync(sp =>
            sp.GetRequiredService<ISender>()
                .Send(new RunMediaCleanupCommand(pendingTtlMinutes, 500))
        );

    private Task<bool> AssetExistsAsync(Guid publicId) =>
        _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            return await db.MediaAssets.IgnoreQueryFilters().AnyAsync(m => m.PublicId == publicId);
        });

    // Builds subject → question(with inline media) → exam → class(approved student) → published assignment.
    private async Task<(
        string AssignmentId,
        Guid MediaId,
        HttpClient Student
    )> PublishWithMediaAsync(HttpClient teacher)
    {
        var (mediaId, url) = await MediaHelper.UploadAsync(teacher);

        // Question content embeds the media URL inline (frozen into the snapshot content + pinned).
        var content = $"Look at this image and answer.\n\n![diagram]({url})\n\nWhat is shown?";
        var qId = await QuestionsApi.CreateAsync(
            teacher,
            subjectId: null!,
            type: "SingleChoice",
            content: content
        );

        var examId = await ExamsApi.CreateAsync(teacher);
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(qId, 2m)]);

        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var student = await AuthHelper.CreateRoleClientAsync(_factory, ApplicationRoles.Student);
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);

        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId)
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        return (assignmentId, mediaId, student);
    }

    [Fact]
    public async Task Publish_FreezesReferencedMedia_IntoSnapshotMedia()
    {
        var teacher = await TeacherAsync();
        var (_, mediaId, _) = await PublishWithMediaAsync(teacher);

        var pinned = await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            return await db.SnapshotMedia.AnyAsync(m => m.MediaPublicId == mediaId);
        });

        pinned.Should().BeTrue("publishing pins referenced media into snapshot_media");
    }

    [Fact]
    public async Task Cleanup_KeepsPinnedSoftDeletedMedia()
    {
        var teacher = await TeacherAsync();
        var (_, mediaId, student) = await PublishWithMediaAsync(teacher);

        // Teacher deletes the source media (soft-delete).
        var delete = await teacher.DeleteAsync($"/api/teacher/media/{mediaId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup must NOT physically remove it — it is still pinned by the published snapshot.
        await RunCleanupAsync();
        (await AssetExistsAsync(mediaId)).Should().BeTrue();

        // The student can still start the attempt and read the frozen content (URL preserved).
        var attemptId = await AssignmentsApi.StartAsync(
            student, /* assignmentId */
            await FirstAssignmentIdAsync(student)
        );
        var taking = await AssignmentsApi.GetTakingAsync(student, attemptId);
        taking
            .GetProperty("questions")[0]
            .GetProperty("content")
            .GetString()
            .Should()
            .Contain(mediaId.ToString("N"));
    }

    [Fact]
    public async Task Cleanup_RemovesUnpinnedSoftDeletedMedia()
    {
        var teacher = await TeacherAsync();
        var (mediaId, _) = await MediaHelper.UploadAsync(teacher);

        var delete = await teacher.DeleteAsync($"/api/teacher/media/{mediaId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        await RunCleanupAsync();
        (await AssetExistsAsync(mediaId)).Should().BeFalse();
    }

    [Fact]
    public async Task Cleanup_RemovesExpiredPendingMedia()
    {
        var teacher = await TeacherAsync();
        var presign = await MediaHelper.PresignAsync(teacher, "x.png", "image/png", 1024);
        var pendingId = presign.GetProperty("mediaPublicId").GetGuid();

        // Negative TTL forces the cutoff into the future so the just-created Pending row is expired.
        await RunCleanupAsync(pendingTtlMinutes: -1);
        (await AssetExistsAsync(pendingId)).Should().BeFalse();
    }

    // Helper: the student's single approved assignment id.
    private static async Task<string> FirstAssignmentIdAsync(HttpClient student)
    {
        var resp = await student.GetAsync("/api/student/assignments?pageSize=1");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(resp);
        return data.GetProperty("items")[0].GetProperty("publicId").GetString()!;
    }
}
