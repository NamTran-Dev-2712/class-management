namespace ClassManagement.IntegrationTests.Admin;

public class DashboardTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task GetOverview_AsAdmin_ReturnsCountersAndCharts()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        var response = await admin.GetAsync("/api/admin/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("totalUsers").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        data.TryGetProperty("activeClasses", out _).Should().BeTrue();
        data.TryGetProperty("openAssignments", out _).Should().BeTrue();
        data.TryGetProperty("pendingReports", out _).Should().BeTrue();

        // 30-day gap-filled trend + status breakdowns are present.
        data.GetProperty("newUsersDaily").GetArrayLength().Should().Be(30);
        data.GetProperty("reportsByStatus").GetArrayLength().Should().BeGreaterThan(0);
        data.GetProperty("assignmentsByStatus").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetOverview_AsStudent_Returns403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        var response = await student.GetAsync("/api/admin/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOverview_Unauthenticated_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/admin/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
