namespace ClassManagement.IntegrationTests.Auth;

public class ForgotPasswordTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task ForgotPassword_WithRegisteredEmail_Returns200AndQueuesOtp()
    {
        var email = TestConstants.UniqueEmail();
        var client = _factory.CreateClient();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword
        );

        var response = await AuthHelper.ForgotPasswordAsync(client, email);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _factory.EmailQueue.GetLastOtp(email).Should().NotBeNullOrEmpty();
        _factory.EmailQueue.GetLastOtp(email).Should().MatchRegex("^[0-9]{6}$");
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_Returns200_AndDoesNotQueue()
    {
        var email = TestConstants.UniqueEmail();

        var response = await AuthHelper.ForgotPasswordAsync(_factory.CreateClient(), email);

        // 200 regardless — never reveal whether the account exists.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _factory.EmailQueue.WasSentTo(email).Should().BeFalse();
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_Returns400()
    {
        var response = await AuthHelper.ForgotPasswordAsync(
            _factory.CreateClient(),
            "not-an-email"
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
