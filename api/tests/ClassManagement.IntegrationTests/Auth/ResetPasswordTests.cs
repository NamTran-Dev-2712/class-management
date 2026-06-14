namespace ClassManagement.IntegrationTests.Auth;

public class ResetPasswordTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;
    private const string NewPassword = "NewPass@123456";

    // Registers a user and runs forgot-password, returning the captured OTP.
    private async Task<string> RequestResetAsync(string email)
    {
        var client = _factory.CreateClient();
        await AuthHelper.RegisterAsync(
            client,
            TestConstants.DefaultDisplayName,
            email,
            TestConstants.DefaultPassword
        );
        await AuthHelper.ForgotPasswordAsync(client, email);

        var otp = _factory.EmailQueue.GetLastOtp(email);
        otp.Should().NotBeNullOrEmpty();
        return otp!;
    }

    [Fact]
    public async Task ResetPassword_WithValidOtp_Returns200_AndNewPasswordWorks()
    {
        var email = TestConstants.UniqueEmail();
        var otp = await RequestResetAsync(email);

        var response = await AuthHelper.ResetPasswordAsync(
            _factory.CreateClient(),
            email,
            otp,
            NewPassword,
            NewPassword
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await AuthHelper.LoginAsync(
            _factory.CreateClientWithCookies(),
            email,
            NewPassword
        );
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithWrongOtp_Returns400()
    {
        var email = TestConstants.UniqueEmail();
        await RequestResetAsync(email);

        var response = await AuthHelper.ResetPasswordAsync(
            _factory.CreateClient(),
            email,
            "000000",
            NewPassword,
            NewPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithWeakPassword_Returns400()
    {
        var email = TestConstants.UniqueEmail();
        var otp = await RequestResetAsync(email);

        var response = await AuthHelper.ResetPasswordAsync(
            _factory.CreateClient(),
            email,
            otp,
            "weakpass",
            "weakpass"
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ConfirmMismatch_Returns400()
    {
        var email = TestConstants.UniqueEmail();
        var otp = await RequestResetAsync(email);

        var response = await AuthHelper.ResetPasswordAsync(
            _factory.CreateClient(),
            email,
            otp,
            NewPassword,
            "DifferentPass@1"
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_UsedOtp_CannotBeReused()
    {
        var email = TestConstants.UniqueEmail();
        var otp = await RequestResetAsync(email);

        var first = await AuthHelper.ResetPasswordAsync(
            _factory.CreateClient(),
            email,
            otp,
            NewPassword,
            NewPassword
        );
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await AuthHelper.ResetPasswordAsync(
            _factory.CreateClient(),
            email,
            otp,
            NewPassword,
            NewPassword
        );
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_RevokesExistingRefreshTokens()
    {
        var email = TestConstants.UniqueEmail();

        // Logged-in session with an active refresh token.
        var session = await AuthHelper.CreateAuthenticatedClientAsync(_factory, email);

        await AuthHelper.ForgotPasswordAsync(_factory.CreateClient(), email);
        var otp = _factory.EmailQueue.GetLastOtp(email)!;

        var reset = await AuthHelper.ResetPasswordAsync(
            _factory.CreateClient(),
            email,
            otp,
            NewPassword,
            NewPassword
        );
        reset.StatusCode.Should().Be(HttpStatusCode.OK);

        // Old session's refresh token must no longer work.
        var refresh = await session.PostAsync("/api/auth/refresh", null);
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_AfterTooManyWrongAttempts_RejectsEvenCorrectOtp()
    {
        var email = TestConstants.UniqueEmail();
        var otp = await RequestResetAsync(email);
        var client = _factory.CreateClient();

        // MaxAttempts = 5 — exhaust them with wrong codes.
        for (var i = 0; i < 5; i++)
        {
            var wrong = await AuthHelper.ResetPasswordAsync(
                client,
                email,
                "000000",
                NewPassword,
                NewPassword
            );
            wrong.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // The correct OTP is now locked out.
        var correct = await AuthHelper.ResetPasswordAsync(
            client,
            email,
            otp,
            NewPassword,
            NewPassword
        );
        correct.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
