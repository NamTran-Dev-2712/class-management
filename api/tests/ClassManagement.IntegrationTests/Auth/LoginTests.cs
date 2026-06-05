namespace ClassManagement.IntegrationTests.Auth;

public class LoginTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndSetsCookies()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClientWithCookies();
        await AuthHelper.RegisterAsync(client, "Test User", email, TestConstants.DefaultPassword);

        var response = await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");

        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.Contains("access_token"));
        cookies.Should().Contain(c => c.Contains("refresh_token"));
        cookies.Should().Contain(c => c.Contains("httponly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsUserProfile()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClient();
        await AuthHelper.RegisterAsync(client, "Test User", email, TestConstants.DefaultPassword);

        var response = await AuthHelper.LoginAsync(client, email, TestConstants.DefaultPassword);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("email").GetString().Should().Be(email);
        data.GetProperty("displayName").GetString().Should().Be("Test User");
        data.GetProperty("roles")
            .EnumerateArray()
            .Should()
            .Contain(e => e.GetString() == "Student");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClient();
        await AuthHelper.RegisterAsync(client, "Test User", email, TestConstants.DefaultPassword);

        var response = await AuthHelper.LoginAsync(client, email, "WrongPassword@1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var response = await AuthHelper.LoginAsync(
            _factory.CreateClient(),
            "nobody@nowhere.local",
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_Returns400()
    {
        var response = await AuthHelper.LoginAsync(
            _factory.CreateClient(),
            "not-an-email",
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
