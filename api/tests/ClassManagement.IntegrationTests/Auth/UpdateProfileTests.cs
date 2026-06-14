namespace ClassManagement.IntegrationTests.Auth;

public class UpdateProfileTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task UpdateProfile_WithValidData_Returns200()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PatchAsJsonAsync(
            "/api/auth/profile",
            new
            {
                displayName = "Updated Name",
                bio = "My new bio",
                avatarUrl = (string?)null,
                phoneNumber = (string?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("displayName").GetString().Should().Be("Updated Name");
        data.GetProperty("bio").GetString().Should().Be("My new bio");
    }

    [Fact]
    public async Task UpdateProfile_WithPhoneNumber_Returns200()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PatchAsJsonAsync(
            "/api/auth/profile",
            new
            {
                displayName = "Test User",
                bio = (string?)null,
                avatarUrl = (string?)null,
                phoneNumber = "+84901234567",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("phoneNumber").GetString().Should().Be("+84901234567");
    }

    [Fact]
    public async Task UpdateProfile_WhenUnauthenticated_Returns401()
    {
        var response = await _factory
            .CreateClient()
            .PatchAsJsonAsync(
                "/api/auth/profile",
                new
                {
                    displayName = "Hacker",
                    bio = (string?)null,
                    avatarUrl = (string?)null,
                    phoneNumber = (string?)null,
                }
            );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithTooShortDisplayName_Returns400()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PatchAsJsonAsync(
            "/api/auth/profile",
            new
            {
                displayName = "A",
                bio = (string?)null,
                avatarUrl = (string?)null,
                phoneNumber = (string?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WithTooLongBio_Returns400()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);
        var longBio = new string('x', 501);

        var response = await client.PatchAsJsonAsync(
            "/api/auth/profile",
            new
            {
                displayName = "Valid Name",
                bio = longBio,
                avatarUrl = (string?)null,
                phoneNumber = (string?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidAvatarUrl_Returns400()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PatchAsJsonAsync(
            "/api/auth/profile",
            new
            {
                displayName = "Valid Name",
                bio = (string?)null,
                avatarUrl = "not-a-url",
                phoneNumber = (string?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_ThenGetProfile_ReturnsUpdatedData()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        await client.PatchAsJsonAsync(
            "/api/auth/profile",
            new
            {
                displayName = "New Display Name",
                bio = "New bio",
                avatarUrl = (string?)null,
                phoneNumber = (string?)null,
            }
        );

        var profileResponse = await client.GetAsync("/api/auth/me");
        var data = await AuthHelper.ReadDataAsync(profileResponse);

        data.GetProperty("displayName").GetString().Should().Be("New Display Name");
        data.GetProperty("bio").GetString().Should().Be("New bio");
    }
}
