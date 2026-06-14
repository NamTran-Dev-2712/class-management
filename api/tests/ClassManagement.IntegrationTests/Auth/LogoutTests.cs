namespace ClassManagement.IntegrationTests.Auth;

public class LogoutTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Logout_WhenAuthenticated_Returns200()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ClearsCookies()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        // Cookies are deleted by setting them to empty/expired
        cookies
            .Should()
            .Contain(c => c.Contains("access_token=;") || c.Contains("access_token= ;"));
        cookies
            .Should()
            .Contain(c => c.Contains("refresh_token=;") || c.Contains("refresh_token= ;"));
    }

    [Fact]
    public async Task Logout_WithoutCookies_Returns200_Idempotent()
    {
        var response = await _factory.CreateClient().PostAsync("/api/auth/logout", null);

        // Logout is always successful — no auth required
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ThenRefresh_Returns401()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        await client.PostAsync("/api/auth/logout", null);

        // Refresh should fail since the token is revoked
        var response = await client.PostAsync("/api/auth/refresh", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ThenProtectedEndpoint_Returns401()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        await client.PostAsync("/api/auth/logout", null);

        // After logout cookies are cleared, protected endpoints return 401
        var response = await client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_CalledTwice_BothReturn200()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var first = await client.PostAsync("/api/auth/logout", null);
        var second = await client.PostAsync("/api/auth/logout", null);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
