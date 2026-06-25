using System.Linq;
using ClassManagement.IntegrationTests.Classroom;

namespace ClassManagement.IntegrationTests.Admin;

public class SystemSettingsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private static Task<HttpResponseMessage> CreateClassRawAsync(HttpClient teacher) =>
        teacher.PostAsJsonAsync(
            "/api/teacher/classes",
            new
            {
                name = ClassroomApi.UniqueName(),
                description = (string?)null,
                subjectId = (string?)null,
                subjectName = "General",
                coverImageUrl = (string?)null,
            }
        );

    [Fact]
    public async Task GetSettings_AsAdmin_IncludesSeededKeys()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var response = await admin.GetAsync("/api/admin/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await AuthHelper.ReadDataAsync(response);
        items
            .EnumerateArray()
            .Select(s => s.GetProperty("key").GetString())
            .Should()
            .Contain("max_classes_per_teacher");
    }

    [Fact]
    public async Task UpdateMaxClasses_EnforcedLiveOnNextCreate()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        // Lower the per-teacher class cap to 1 — should take effect immediately (cache invalidated).
        var update = await admin.PutAsJsonAsync(
            "/api/admin/settings/max_classes_per_teacher",
            new { value = "1" }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var first = await CreateClassRawAsync(teacher);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await CreateClassRawAsync(teacher);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Restore unlimited so other tests in this class are unaffected.
        await admin.PutAsJsonAsync(
            "/api/admin/settings/max_classes_per_teacher",
            new { value = "0" }
        );
    }

    [Fact]
    public async Task UpdateSetting_AsNonAdmin_Returns403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var response = await student.PutAsJsonAsync(
            "/api/admin/settings/max_classes_per_teacher",
            new { value = "5" }
        );
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Dashboard_AsAdmin_ReturnsCounters()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var response = await admin.GetAsync("/api/admin/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("totalUsers").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        data.TryGetProperty("pendingReports", out _).Should().BeTrue();

        // Chart series (MVP-7.5): 30-day new-user trend + status breakdowns.
        data.GetProperty("newUsersDaily").EnumerateArray().Should().HaveCount(30);
        data.GetProperty("reportsByStatus").EnumerateArray().Should().NotBeEmpty();
        data.GetProperty("assignmentsByStatus").EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task AppName_PublicConfig_ReflectsSetting()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        // The string setting is sent as a plain value; the handler stores it as JSON.
        var unique = $"ClassHub_{Guid.NewGuid():N}";
        var set = await admin.PutAsJsonAsync(
            "/api/admin/settings/app_name",
            new { value = unique }
        );
        set.StatusCode.Should().Be(HttpStatusCode.OK);

        // Anonymous client can read the public config and sees the new name.
        var anon = _factory.CreateClient();
        var cfg = await anon.GetAsync("/api/public/app-config");
        cfg.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(cfg);
        data.GetProperty("appName").GetString().Should().Be(unique);
        data.GetProperty("maintenanceMode").GetBoolean().Should().BeFalse();

        await admin.PutAsJsonAsync(
            "/api/admin/settings/app_name",
            new { value = "Class Management" }
        );
    }

    [Fact]
    public async Task InviteCodeLength_LiveSetting_AppliesToNewClass()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var set = await admin.PutAsJsonAsync(
            "/api/admin/settings/invite_code_length",
            new { value = "6" }
        );
        set.StatusCode.Should().Be(HttpStatusCode.OK);

        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var code = await ClassroomApi.GetInviteCodeAsync(teacher, classId);
        code.Length.Should().Be(6);

        await admin.PutAsJsonAsync("/api/admin/settings/invite_code_length", new { value = "8" });
    }

    [Fact]
    public async Task MaintenanceMode_BlocksNonAdminWrites_AllowsReadsAndAdmin()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        try
        {
            (
                await admin.PutAsJsonAsync(
                    "/api/admin/settings/maintenance_mode",
                    new { value = "true" }
                )
            )
                .StatusCode.Should()
                .Be(HttpStatusCode.OK);

            // Non-admin write is blocked with 503.
            var blocked = await CreateClassRawAsync(teacher);
            blocked.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

            // Reads stay open for non-admins.
            var read = await teacher.GetAsync("/api/teacher/classes");
            read.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            await admin.PutAsJsonAsync(
                "/api/admin/settings/maintenance_mode",
                new { value = "false" }
            );
        }

        // After maintenance ends, writes work again.
        var ok = await CreateClassRawAsync(teacher);
        ok.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task MaxStudentsPerClass_LiveSetting_RejectsApprovalOverCap()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        (
            await admin.PutAsJsonAsync(
                "/api/admin/settings/max_students_per_class",
                new { value = "1" }
            )
        )
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var invite = await ClassroomApi.GetInviteCodeAsync(teacher, classId);

        var s1 = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(s1, invite)).EnsureSuccessStatusCode();
        var s2 = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await ClassroomApi.JoinAsync(s2, invite)).EnsureSuccessStatusCode();

        // Approve the first (ok), then the second must be rejected by the live cap.
        var pending1 = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        var ok = await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{pending1}/approve",
            null
        );
        ok.StatusCode.Should().Be(HttpStatusCode.OK);

        var pending2 = await ClassroomApi.GetFirstMemberIdAsync(teacher, classId, "Pending");
        var rejected = await teacher.PostAsync(
            $"/api/teacher/classes/{classId}/members/{pending2}/approve",
            null
        );
        rejected.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await admin.PutAsJsonAsync(
            "/api/admin/settings/max_students_per_class",
            new { value = "0" }
        );
    }
}
