using ClassManagement.IntegrationTests.Classroom;

namespace ClassManagement.IntegrationTests.Payment;

// MVP-8 Slice 2 — plan-aware resource usage. A teacher with no active subscription is on Free; the usage
// endpoint reports current counts vs. the effective (live system_settings) limits.
public class ResourceUsageTests(ApiFactory factory) : IClassFixture<ApiFactory>
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
    public async Task GetUsage_FreeTeacher_ReportsPlanAndCounts()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var before = await teacher.GetAsync("/api/teacher/subscription/usage");
        before.StatusCode.Should().Be(HttpStatusCode.OK);
        var beforeData = await AuthHelper.ReadDataAsync(before);
        beforeData.GetProperty("planName").GetString().Should().Be("Free");
        beforeData.GetProperty("isPro").GetBoolean().Should().BeFalse();
        var usedBefore = beforeData.GetProperty("classes").GetProperty("used").GetInt32();

        (await CreateClassRawAsync(teacher)).StatusCode.Should().Be(HttpStatusCode.Created);

        var after = await teacher.GetAsync("/api/teacher/subscription/usage");
        var afterData = await AuthHelper.ReadDataAsync(after);
        afterData.GetProperty("classes").GetProperty("used").GetInt32().Should().Be(usedBefore + 1);
    }

    [Fact]
    public async Task GetUsage_AsStudent_Returns403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var response = await student.GetAsync("/api/teacher/subscription/usage");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
