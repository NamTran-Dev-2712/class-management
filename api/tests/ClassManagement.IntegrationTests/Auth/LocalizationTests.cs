namespace ClassManagement.IntegrationTests.Auth;

public class LocalizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private static async Task<string?> ReadMessageAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("message").GetString();
    }

    [Fact]
    public async Task Error_WithoutAcceptLanguage_ReturnsEnglishMessage()
    {
        var response = await AuthHelper.LoginAsync(
            _factory.CreateClient(),
            "nobody@nowhere.local",
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ReadMessageAsync(response)).Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Error_WithVietnameseAcceptLanguage_ReturnsVietnameseMessage()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("vi");

        var response = await AuthHelper.LoginAsync(
            client,
            "nobody@nowhere.local",
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ReadMessageAsync(response)).Should().Be("Thông tin đăng nhập không hợp lệ.");
    }

    [Fact]
    public async Task ValidationError_WithVietnameseAcceptLanguage_LocalizesErrors()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("vi");

        var response = await AuthHelper.LoginAsync(
            client,
            "not-an-email",
            TestConstants.DefaultPassword
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("message").GetString().Should().Be("Dữ liệu không hợp lệ.");
        json.GetProperty("errors")
            .EnumerateArray()
            .Select(e => e.GetString())
            .Should()
            .Contain("Email không đúng định dạng.");
    }
}
