namespace ClassManagement.IntegrationTests.Auth;

public class ChangePasswordTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;
    private const string NewPassword = "NewPass@123456";

    [Fact]
    public async Task ChangePassword_WithValidData_Returns200()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = TestConstants.DefaultPassword,
                newPassword = NewPassword,
                confirmNewPassword = NewPassword,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_ClearsCookies()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = TestConstants.DefaultPassword,
                newPassword = NewPassword,
                confirmNewPassword = NewPassword,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Set-Cookie with empty value or expired date means deletion
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies
            .Should()
            .Contain(c => c.Contains("access_token=;") || c.Contains("access_token= ;"));
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Returns400()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = "WrongPass@1",
                newPassword = NewPassword,
                confirmNewPassword = NewPassword,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_SameAsCurrentPassword_Returns400()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = TestConstants.DefaultPassword,
                newPassword = TestConstants.DefaultPassword,
                confirmNewPassword = TestConstants.DefaultPassword,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ConfirmMismatch_Returns400()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = TestConstants.DefaultPassword,
                newPassword = NewPassword,
                confirmNewPassword = "DifferentPass@1",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WeakNewPassword_Returns400()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = TestConstants.DefaultPassword,
                newPassword = "weakpassword",
                confirmNewPassword = "weakpassword",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WhenUnauthenticated_Returns401()
    {
        var response = await _factory
            .CreateClient()
            .PostAsJsonAsync(
                "/api/auth/change-password",
                new
                {
                    currentPassword = TestConstants.DefaultPassword,
                    newPassword = NewPassword,
                    confirmNewPassword = NewPassword,
                }
            );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_NewPasswordCanBeUsedToLogin()
    {
        var email = TestConstants.UniqueEmail();
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory, email);

        await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = TestConstants.DefaultPassword,
                newPassword = NewPassword,
                confirmNewPassword = NewPassword,
            }
        );

        var freshClient = _factory.CreateClientWithCookies();
        var loginResponse = await AuthHelper.LoginAsync(freshClient, email, NewPassword);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
