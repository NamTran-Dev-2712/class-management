namespace ClassManagement.IntegrationTests.Auth;

public class GetProfileTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task GetProfile_WhenAuthenticated_Returns200WithProfile()
    {
        var email = TestConstants.UniqueEmail();
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory, email);

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("email").GetString().Should().Be(email);
        data.GetProperty("displayName").GetString().Should().Be(TestConstants.DefaultDisplayName);
        data.GetProperty("publicId").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("roles").EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProfile_WhenUnauthenticated_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_CalledTwice_ReturnsSameData()
    {
        var email = TestConstants.UniqueEmail();
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory, email);

        var first = await AuthHelper.ReadDataAsync(await client.GetAsync("/api/auth/me"));
        var second = await AuthHelper.ReadDataAsync(await client.GetAsync("/api/auth/me"));

        first.GetProperty("email").GetString().Should().Be(second.GetProperty("email").GetString());
        first
            .GetProperty("publicId")
            .GetString()
            .Should()
            .Be(second.GetProperty("publicId").GetString());
    }
}
