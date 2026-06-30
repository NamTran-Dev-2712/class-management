using ClassManagement.IntegrationTests.Classroom;

namespace ClassManagement.IntegrationTests.OutputCache;

// Output caching is enabled in tests so the cache path is actually exercised. These guard the
// security-critical invariants (no cross-user leak, authorization runs before the cache) and that
// writes evict cached reads.
public class OutputCacheTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task TeacherClasses_AreNotSharedAcrossTeachers()
    {
        // Teacher A creates a class and lists it (populating A's per-user cache entry).
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacherA);
        var aList = await AuthHelper.ReadDataAsync(
            await teacherA.GetAsync("/api/teacher/classes?pageSize=50")
        );
        aList.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);

        // Teacher B must never receive A's cached list — vary-by-user keys them separately and the
        // query is owner-scoped, so B sees only their own (empty) classes.
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var bList = await AuthHelper.ReadDataAsync(
            await teacherB.GetAsync("/api/teacher/classes?pageSize=50")
        );
        bList.GetProperty("totalCount").GetInt32().Should().Be(0);
        bList
            .GetProperty("items")
            .EnumerateArray()
            .Select(i => i.GetProperty("publicId").GetString())
            .Should()
            .NotContain(classId);
    }

    [Fact]
    public async Task AdminUsers_CachedForAdmin_NotServedToNonAdmin()
    {
        // Admin populates the shared cache for the users list.
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        (await admin.GetAsync("/api/admin/users?pageSize=10"))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        // A student hitting the exact same URL is rejected by authorization (which runs before the
        // output cache) — a cached admin response can never be served to a non-admin.
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        (await student.GetAsync("/api/admin/users?pageSize=10"))
            .StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubjectsList_SecondGet_IsServedFromCache()
    {
        var client = _factory.CreateClient();

        // The ApiResponse envelope echoes HttpContext.TraceIdentifier. A cache HIT replays the exact
        // cached body, so the second identical GET carries the SAME traceId as the first — proving it
        // was served from the cache rather than re-executing the endpoint.
        static async Task<string> TraceIdAsync(HttpResponseMessage r) =>
            (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("traceId").GetString()!;

        var first = await TraceIdAsync(await client.GetAsync("/api/subjects?pageSize=7"));
        var second = await TraceIdAsync(await client.GetAsync("/api/subjects?pageSize=7"));

        second.Should().Be(first);
    }

    [Fact]
    public async Task SubjectsList_IsEvicted_AfterCreate()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        // Prime the cache for this exact list query.
        var before = await AuthHelper.ReadDataAsync(
            await admin.GetAsync("/api/subjects?pageSize=100")
        );
        var beforeCount = before.GetProperty("totalCount").GetInt32();

        // A write must evict the tag…
        var create = await admin.PostAsJsonAsync(
            "/api/subjects",
            new
            {
                name = $"Cache_{Guid.NewGuid():N}",
                description = (string?)null,
                isActive = true,
                displayOrder = 0,
            }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        // …so the same cached query now returns fresh data (count grew by one).
        var after = await AuthHelper.ReadDataAsync(
            await admin.GetAsync("/api/subjects?pageSize=100")
        );
        after.GetProperty("totalCount").GetInt32().Should().Be(beforeCount + 1);
    }

    [Fact]
    public async Task SubjectDetail_SecondGet_IsCached_AndEvictedOnUpdate()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        // Create a subject and read its id (the detail GET is anonymous + output-cached).
        var subjectId = await AuthHelper.ReadDataAsync(
            await admin.PostAsJsonAsync(
                "/api/subjects",
                new
                {
                    name = $"Detail_{Guid.NewGuid():N}",
                    description = "v1",
                    isActive = true,
                    displayOrder = 0,
                }
            )
        );
        var id = subjectId.GetProperty("publicId").GetString();

        // Anonymous client so the OutputCache default policy (which skips authenticated requests)
        // actually caches the response — the second identical GET replays the cached body (same traceId).
        var anon = _factory.CreateClient();

        static async Task<string> TraceIdAsync(HttpResponseMessage r) =>
            (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("traceId").GetString()!;

        var first = await TraceIdAsync(await anon.GetAsync($"/api/subjects/{id}"));
        var second = await TraceIdAsync(await anon.GetAsync($"/api/subjects/{id}"));
        second.Should().Be(first);

        // A write evicts the "subjects" tag, so the next GET re-executes (fresh traceId).
        var update = await admin.PutAsJsonAsync(
            $"/api/subjects/{id}",
            new
            {
                name = $"Detail_{Guid.NewGuid():N}",
                description = "v2",
                isActive = true,
                displayOrder = 0,
            }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var third = await TraceIdAsync(await anon.GetAsync($"/api/subjects/{id}"));
        third.Should().NotBe(second);
    }
}
