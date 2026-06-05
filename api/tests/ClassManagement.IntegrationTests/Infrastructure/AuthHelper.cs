namespace ClassManagement.IntegrationTests.Infrastructure;

public static class AuthHelper
{
    public static Task<HttpResponseMessage> RegisterAsync(
        HttpClient client,
        string displayName,
        string email,
        string password,
        string role = "Student"
    ) =>
        client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName,
                email,
                password,
                role,
            }
        );

    public static Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password
    ) => client.PostAsJsonAsync("/api/auth/login", new { email, password });

    // Register + login in one step, returns an authenticated client
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        ApiFactory factory,
        string? email = null,
        string? password = null
    )
    {
        email ??= TestConstants.UniqueEmail();
        password ??= TestConstants.DefaultPassword;

        var client = factory.CreateClientWithCookies();
        await RegisterAsync(client, TestConstants.DefaultDisplayName, email, password);
        var loginResponse = await LoginAsync(client, email, password);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return client;
    }

    // Reads Data field from ApiResponse<T> envelope
    public static async Task<JsonElement> ReadDataAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data");
    }
}
