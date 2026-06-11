namespace ClassManagement.IntegrationTests.Catalog;

public class SubjectTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private static string UniqueName() => $"Subject_{Guid.NewGuid():N}";

    private static async Task<string> CreateSubjectAsync(
        HttpClient client,
        string name,
        bool isActive = true,
        int displayOrder = 0,
        string? description = "A subject"
    )
    {
        var response = await client.PostAsJsonAsync(
            "/api/subjects",
            new
            {
                name,
                description,
                isActive,
                displayOrder,
            }
        );
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await AuthHelper.ReadDataAsync(response);
        return data.GetProperty("publicId").GetString()!;
    }

    [Fact]
    public async Task Create_AsAdmin_Returns201_WithPublicId()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        var publicId = await CreateSubjectAsync(client, UniqueName());
        Guid.TryParse(publicId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithDuplicateName_Returns409()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        var name = UniqueName();
        await CreateSubjectAsync(client, name);

        var response = await client.PostAsJsonAsync(
            "/api/subjects",
            new
            {
                name,
                description = (string?)null,
                isActive = true,
                displayOrder = 0,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithInvalidName_Returns400()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);

        var response = await client.PostAsJsonAsync(
            "/api/subjects",
            new
            {
                name = "A",
                description = (string?)null,
                isActive = true,
                displayOrder = 0,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetList_ReturnsPaginatedEnvelope()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        await CreateSubjectAsync(client, UniqueName());

        var response = await client.GetAsync("/api/subjects?pageNumber=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("pageNumber").GetInt32().Should().Be(1);
        data.GetProperty("pageSize").GetInt32().Should().Be(5);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
        data.GetProperty("items").GetArrayLength().Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetList_SearchTerm_ReturnsMatchingSubject()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        var name = UniqueName();
        await CreateSubjectAsync(client, name);

        var response = await client.GetAsync($"/api/subjects?searchTerm={name}");
        var data = await AuthHelper.ReadDataAsync(response);

        data.GetProperty("totalCount").GetInt32().Should().Be(1);
        data.GetProperty("items")[0].GetProperty("name").GetString().Should().Be(name);
    }

    [Fact]
    public async Task GetList_SortByNameAsc_ReturnsSorted()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);

        var response = await client.GetAsync(
            "/api/subjects?pageSize=100&sortBy=name&sortOrder=asc"
        );
        var data = await AuthHelper.ReadDataAsync(response);

        var names = data.GetProperty("items")
            .EnumerateArray()
            .Select(i => i.GetProperty("name").GetString()!)
            .ToList();

        names.Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Fact]
    public async Task GetById_ExistingSubject_ReturnsIt()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        var name = UniqueName();
        var publicId = await CreateSubjectAsync(client, name);

        var response = await client.GetAsync($"/api/subjects/{publicId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("name").GetString().Should().Be(name);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        var response = await client.GetAsync($"/api/subjects/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_AsAdmin_PersistsChanges()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        var publicId = await CreateSubjectAsync(client, UniqueName());
        var newName = UniqueName();

        var update = await client.PutAsJsonAsync(
            $"/api/subjects/{publicId}",
            new
            {
                name = newName,
                description = "Updated",
                isActive = false,
                displayOrder = 5,
            }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(
            await client.GetAsync($"/api/subjects/{publicId}")
        );
        data.GetProperty("name").GetString().Should().Be(newName);
        data.GetProperty("isActive").GetBoolean().Should().BeFalse();
        data.GetProperty("displayOrder").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task Delete_AsAdmin_SoftDeletes()
    {
        var client = await AuthHelper.CreateAdminClientAsync(_factory);
        var name = UniqueName();
        var publicId = await CreateSubjectAsync(client, name);

        var delete = await client.DeleteAsync($"/api/subjects/{publicId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        // Gone from detail …
        (await client.GetAsync($"/api/subjects/{publicId}"))
            .StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
        // … and from the list.
        var data = await AuthHelper.ReadDataAsync(
            await client.GetAsync($"/api/subjects?searchTerm={name}")
        );
        data.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task WriteEndpoints_RequireAdmin_NonAdminGets403()
    {
        var studentClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        // Reads are open to any role.
        (await studentClient.GetAsync("/api/subjects"))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        // Writes are Admin-only.
        var create = await studentClient.PostAsJsonAsync(
            "/api/subjects",
            new
            {
                name = UniqueName(),
                description = (string?)null,
                isActive = true,
                displayOrder = 0,
            }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReadEndpoints_AllowAnonymous()
    {
        var response = await _factory.CreateClient().GetAsync("/api/subjects");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WriteEndpoints_RequireAuth_AnonymousGets401()
    {
        var create = await _factory
            .CreateClient()
            .PostAsJsonAsync(
                "/api/subjects",
                new
                {
                    name = UniqueName(),
                    description = (string?)null,
                    isActive = true,
                    displayOrder = 0,
                }
            );
        create.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
