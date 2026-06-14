namespace ClassManagement.IntegrationTests.Auth;

public class RegisterTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var response = await AuthHelper.RegisterAsync(
            _client,
            "John Doe",
            TestConstants.UniqueEmail(),
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("userId").GetInt64().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var email = TestConstants.UniqueEmail();
        await AuthHelper.RegisterAsync(_client, "First User", email, TestConstants.DefaultPassword);

        var response = await AuthHelper.RegisterAsync(
            _client,
            "Second User",
            email,
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400()
    {
        var response = await AuthHelper.RegisterAsync(
            _client,
            "Test User",
            TestConstants.UniqueEmail(),
            "weakpassword" // no uppercase, no special char
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithShortDisplayName_Returns400()
    {
        var response = await AuthHelper.RegisterAsync(
            _client,
            "A", // too short
            TestConstants.UniqueEmail(),
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var response = await AuthHelper.RegisterAsync(
            _client,
            "Test User",
            "not-an-email",
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_AsTeacher_Returns201()
    {
        var response = await AuthHelper.RegisterAsync(
            _client,
            "Teacher User",
            TestConstants.UniqueEmail(),
            TestConstants.DefaultPassword,
            role: "Teacher"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_AsAdmin_Returns400()
    {
        // Admin must not be self-registerable.
        var response = await AuthHelper.RegisterAsync(
            _client,
            "Sneaky Admin",
            TestConstants.UniqueEmail(),
            TestConstants.DefaultPassword,
            role: "Admin"
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
