namespace ClassManagement.IntegrationTests.Exams;

// Exam tags mirror the question-tag behaviour: free-form, normalized [a-z0-9-], ≤10, used for search.
public class ExamTagTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private static object CreateBody(string title, string[]? tags) =>
        new
        {
            subjectId = (string?)null,
            title,
            description = (string?)null,
            visibility = "Private",
            tags,
        };

    private static List<string> TagsOf(JsonElement el) =>
        el.GetProperty("tags").EnumerateArray().Select(t => t.GetString()!).ToList();

    [Fact]
    public async Task Create_WithTags_ReturnsTagsInDetailAndList()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var title = ExamsApi.UniqueTitle();

        var create = await teacher.PostAsJsonAsync(
            "/api/teacher/exams",
            CreateBody(title, ["algebra", "midterm"])
        );
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var examId = (await AuthHelper.ReadDataAsync(create)).GetProperty("publicId").GetString()!;

        var detail = await ExamsApi.GetAsync(teacher, examId);
        TagsOf(detail).Should().BeEquivalentTo(["algebra", "midterm"]);

        var list = await ExamsApi.ListAsync(teacher, "?pageSize=50");
        var row = list.GetProperty("items")
            .EnumerateArray()
            .First(e => e.GetProperty("publicId").GetString() == examId);
        TagsOf(row).Should().Contain("algebra");
    }

    [Fact]
    public async Task Create_NormalizesAndDedupesTags()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var create = await teacher.PostAsJsonAsync(
            "/api/teacher/exams",
            CreateBody(ExamsApi.UniqueTitle(), ["  Hello World  ", "Hello World", "a!!b"])
        );
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var examId = (await AuthHelper.ReadDataAsync(create)).GetProperty("publicId").GetString()!;

        var detail = await ExamsApi.GetAsync(teacher, examId);
        TagsOf(detail).Should().BeEquivalentTo(["hello-world", "ab"]);
    }

    [Fact]
    public async Task Update_ReplacesTags()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var create = await teacher.PostAsJsonAsync(
            "/api/teacher/exams",
            CreateBody(ExamsApi.UniqueTitle(), ["old-tag"])
        );
        var examId = (await AuthHelper.ReadDataAsync(create)).GetProperty("publicId").GetString()!;

        var before = await ExamsApi.GetAsync(teacher, examId);
        var version = before.GetProperty("version").GetInt32();

        var update = await teacher.PutAsJsonAsync(
            $"/api/teacher/exams/{examId}",
            new
            {
                subjectId = (string?)null,
                title = before.GetProperty("title").GetString(),
                description = (string?)null,
                visibility = "Private",
                tags = new[] { "new-tag", "another" },
            }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await ExamsApi.GetAsync(teacher, examId);
        TagsOf(after).Should().BeEquivalentTo(["new-tag", "another"]);
        // Metadata edit must NOT bump the version.
        after.GetProperty("version").GetInt32().Should().Be(version);
    }

    [Fact]
    public async Task Filter_ByTag_ReturnsOnlyMatchingExams()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var marker = $"tag{Guid.NewGuid():N}".ToLowerInvariant()[..12];

        var a = await teacher.PostAsJsonAsync(
            "/api/teacher/exams",
            CreateBody(ExamsApi.UniqueTitle(), [marker])
        );
        var taggedId = (await AuthHelper.ReadDataAsync(a)).GetProperty("publicId").GetString()!;
        await teacher.PostAsJsonAsync(
            "/api/teacher/exams",
            CreateBody(ExamsApi.UniqueTitle(), ["unrelated"])
        );

        var list = await ExamsApi.ListAsync(teacher, $"?tag={marker}&pageSize=50");
        var ids = list.GetProperty("items")
            .EnumerateArray()
            .Select(e => e.GetProperty("publicId").GetString())
            .ToList();

        ids.Should().ContainSingle().Which.Should().Be(taggedId);
    }

    [Fact]
    public async Task Create_WithMoreThanTenTags_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var tags = Enumerable.Range(1, 11).Select(i => $"tag-{i}").ToArray();

        var resp = await teacher.PostAsJsonAsync(
            "/api/teacher/exams",
            CreateBody(ExamsApi.UniqueTitle(), tags)
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Duplicate_CopiesTags()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var create = await teacher.PostAsJsonAsync(
            "/api/teacher/exams",
            CreateBody(ExamsApi.UniqueTitle(), ["copyme", "shared"])
        );
        var examId = (await AuthHelper.ReadDataAsync(create)).GetProperty("publicId").GetString()!;

        var dup = await teacher.PostAsync($"/api/teacher/exams/{examId}/duplicate", null);
        dup.StatusCode.Should().Be(HttpStatusCode.Created);
        var copyId = (await AuthHelper.ReadDataAsync(dup)).GetProperty("publicId").GetString()!;

        var copy = await ExamsApi.GetAsync(teacher, copyId);
        TagsOf(copy).Should().BeEquivalentTo(["copyme", "shared"]);
    }
}
