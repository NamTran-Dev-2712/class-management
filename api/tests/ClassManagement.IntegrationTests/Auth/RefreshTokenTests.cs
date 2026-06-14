namespace ClassManagement.IntegrationTests.Auth;

public class RefreshTokenTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Refresh_WithValidToken_Returns200AndNewCookies()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword
        );
        await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);

        var response = await client.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.Contains("access_token"));
        cookies.Should().Contain(c => c.Contains("refresh_token"));
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsUserProfile()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword
        );
        await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);

        var response = await client.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("email").GetString().Should().Be(email);
    }

    [Fact]
    public async Task Refresh_WithNoRefreshCookie_Returns401()
    {
        var response = await _factory.CreateClient().PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithTamperedToken_Returns401()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword
        );
        await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);

        // Manually send a tampered token
        var tamperedClient = _factory.CreateClient();
        tamperedClient.DefaultRequestHeaders.Add("Cookie", "refresh_token=tampered_invalid_token");

        var response = await tamperedClient.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_AfterLogout_TokenIsRevoked_Returns401()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword
        );
        await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);

        // Logout revokes the refresh token
        await client.PostAsync("/api/auth/logout", null);

        // Refresh should fail now
        var response = await client.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_Twice_RotatesTokens()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword
        );
        await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);

        var first = await client.PostAsync("/api/auth/refresh", null);
        var second = await client.PostAsync("/api/auth/refresh", null);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
