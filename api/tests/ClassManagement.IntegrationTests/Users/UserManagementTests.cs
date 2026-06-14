namespace ClassManagement.IntegrationTests.Users;

public class UserManagementTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<(string PublicId, string Email)> CreateUserAsync(
        HttpClient admin,
        string role = "Teacher",
        string? phoneNumber = null
    )
    {
        var email = TestConstants.UniqueEmail();
        var response = await admin.PostAsJsonAsync(
            "/api/admin/users",
            new
            {
                displayName = "New User",
                email,
                role,
                phoneNumber,
            }
        );
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await AuthHelper.ReadDataAsync(response);
        return (data.GetProperty("publicId").GetString()!, email);
    }

    private async Task<string> GetAdminPublicIdAsync(HttpClient admin)
    {
        var me = await admin.GetAsync("/api/auth/me");
        var data = await AuthHelper.ReadDataAsync(me);
        return data.GetProperty("publicId").GetString()!;
    }

    // ---- Create -------------------------------------------------------------

    [Fact]
    public async Task Create_AsAdmin_Returns201_AndEmailsWorkingPassword()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (publicId, email) = await CreateUserAsync(admin);

        Guid.TryParse(publicId, out _).Should().BeTrue();

        // The generated password is emailed, never returned in the response.
        var password = _factory.EmailQueue.GetLastWelcomePassword(email);
        password.Should().NotBeNullOrEmpty();

        // The emailed password actually works for login.
        var client = _factory.CreateClientWithCookies();
        var login = await AuthHelper.LoginAsync(client, email, password!);
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_WithDuplicateEmail_Returns409()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (_, email) = await CreateUserAsync(admin);

        var response = await admin.PostAsJsonAsync(
            "/api/admin/users",
            new
            {
                displayName = "Another",
                email,
                role = "Student",
                phoneNumber = (string?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithInvalidRole_Returns400()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        var response = await admin.PostAsJsonAsync(
            "/api/admin/users",
            new
            {
                displayName = "Bad Role",
                email = TestConstants.UniqueEmail(),
                role = "Superuser",
                phoneNumber = (string?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithAdminRole_Returns400()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        var response = await admin.PostAsJsonAsync(
            "/api/admin/users",
            new
            {
                displayName = "Should Fail",
                email = TestConstants.UniqueEmail(),
                role = "Admin",
                phoneNumber = (string?)null,
            }
        );

        // User management is scoped to Teachers/Students — Admin is not assignable here.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- Authorization ------------------------------------------------------

    [Fact]
    public async Task Endpoints_RequireAuth_AnonymousGets401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/admin/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoints_RequireAdmin_NonAdminGets403()
    {
        var student = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        (await student.GetAsync("/api/admin/users"))
            .StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);

        var create = await student.PostAsJsonAsync(
            "/api/admin/users",
            new
            {
                displayName = "Nope",
                email = TestConstants.UniqueEmail(),
                role = "Student",
                phoneNumber = (string?)null,
            }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- List / detail ------------------------------------------------------

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsPagedData()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (_, email) = await CreateUserAsync(admin);

        var data = await AuthHelper.ReadDataAsync(
            await admin.GetAsync($"/api/admin/users?searchTerm={email}")
        );

        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        data.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetUsers_FilterByRole_ReturnsOnlyThatRole()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (_, email) = await CreateUserAsync(admin, role: "Teacher");

        var data = await AuthHelper.ReadDataAsync(
            await admin.GetAsync($"/api/admin/users?searchTerm={email}&role=Teacher")
        );

        var items = data.GetProperty("items");
        items.GetArrayLength().Should().Be(1);
        items[0].GetProperty("roles")[0].GetString().Should().Be("Teacher");
    }

    [Fact]
    public async Task GetUsers_ExcludesAdminAccounts()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var adminEmail = TestConstants.AdminEmail;

        // The seeded admin (the caller) must never appear in the management list.
        var data = await AuthHelper.ReadDataAsync(
            await admin.GetAsync($"/api/admin/users?searchTerm={adminEmail}")
        );

        data.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GetUserById_OnAdmin_Returns403()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var adminPublicId = await GetAdminPublicIdAsync(admin);

        var response = await admin.GetAsync($"/api/admin/users/{adminPublicId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_ReturnsDetail()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (publicId, email) = await CreateUserAsync(admin);

        var data = await AuthHelper.ReadDataAsync(
            await admin.GetAsync($"/api/admin/users/{publicId}")
        );

        data.GetProperty("email").GetString().Should().Be(email);
        data.GetProperty("isLocked").GetBoolean().Should().BeFalse();
    }

    // ---- Update -------------------------------------------------------------

    [Fact]
    public async Task Update_AsAdmin_ChangesDisplayNameAndRole()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (publicId, _) = await CreateUserAsync(admin, role: "Teacher");

        var update = await admin.PutAsJsonAsync(
            $"/api/admin/users/{publicId}",
            new
            {
                displayName = "Renamed User",
                role = "Student",
                phoneNumber = "+84 123 456 789",
            }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(
            await admin.GetAsync($"/api/admin/users/{publicId}")
        );
        data.GetProperty("displayName").GetString().Should().Be("Renamed User");
        data.GetProperty("roles")[0].GetString().Should().Be("Student");
    }

    [Fact]
    public async Task Update_OnAdmin_Returns403()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var adminPublicId = await GetAdminPublicIdAsync(admin);

        var response = await admin.PutAsJsonAsync(
            $"/api/admin/users/{adminPublicId}",
            new
            {
                displayName = "Hacked",
                role = "Teacher",
                phoneNumber = (string?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- Lock / unlock ------------------------------------------------------

    [Fact]
    public async Task Lock_ThenLogin_Returns401()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (publicId, email) = await CreateUserAsync(admin);
        var password = _factory.EmailQueue.GetLastWelcomePassword(email)!;

        (await admin.PostAsync($"/api/admin/users/{publicId}/lock", null))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var client = _factory.CreateClientWithCookies();
        var login = await AuthHelper.LoginAsync(client, email, password);
        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unlock_RestoresLogin()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (publicId, email) = await CreateUserAsync(admin);
        var password = _factory.EmailQueue.GetLastWelcomePassword(email)!;

        await admin.PostAsync($"/api/admin/users/{publicId}/lock", null);
        (await admin.PostAsync($"/api/admin/users/{publicId}/unlock", null))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var client = _factory.CreateClientWithCookies();
        var login = await AuthHelper.LoginAsync(client, email, password);
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Lock_Self_Returns403()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var adminPublicId = await GetAdminPublicIdAsync(admin);

        var response = await admin.PostAsync($"/api/admin/users/{adminPublicId}/lock", null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- Delete -------------------------------------------------------------

    [Fact]
    public async Task Delete_AsAdmin_SoftDeletes_AndRemovesFromList()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (publicId, email) = await CreateUserAsync(admin);

        (await admin.DeleteAsync($"/api/admin/users/{publicId}"))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        (await admin.GetAsync($"/api/admin/users/{publicId}"))
            .StatusCode.Should()
            .Be(HttpStatusCode.NotFound);

        var data = await AuthHelper.ReadDataAsync(
            await admin.GetAsync($"/api/admin/users?searchTerm={email}")
        );
        data.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Delete_Self_Returns403()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var adminPublicId = await GetAdminPublicIdAsync(admin);

        var response = await admin.DeleteAsync($"/api/admin/users/{adminPublicId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
