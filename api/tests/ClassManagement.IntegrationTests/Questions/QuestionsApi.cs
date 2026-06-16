namespace ClassManagement.IntegrationTests.Questions;

// Shared request helpers for the question-bank endpoints, mirroring ClassroomApi.
internal static class QuestionsApi
{
    public static string UniqueContent() =>
        $"What is the meaning of {Guid.NewGuid():N}? Please explain.";

    // Fetches an active seeded subject's publicId (questions must be tied to an active subject).
    public static async Task<string> GetActiveSubjectIdAsync(HttpClient client)
    {
        var resp = await client.GetAsync("/api/subjects?pageSize=1&isActive=true&sortBy=name");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = (await AuthHelper.ReadDataAsync(resp)).GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0, "the seeder seeds active subjects");
        return items[0].GetProperty("publicId").GetString()!;
    }

    public static object[]? DefaultOptions(string type) =>
        type switch
        {
            "SingleChoice" =>
            [
                new { content = "Alpha", isCorrect = true },
                new { content = "Beta", isCorrect = false },
            ],
            "MultipleChoice" =>
            [
                new { content = "Alpha", isCorrect = true },
                new { content = "Beta", isCorrect = true },
                new { content = "Gamma", isCorrect = false },
            ],
            "TrueFalse" =>
            [
                new { content = "True", isCorrect = true },
                new { content = "False", isCorrect = false },
            ],
            _ => null,
        };

    public static object BuildPayload(
        string? subjectId,
        string type = "SingleChoice",
        string? content = null,
        string difficulty = "Medium",
        string visibility = "Private",
        object[]? options = null,
        string[]? tags = null,
        double suggestedPoint = 1.0,
        string? explanation = null
    ) =>
        new
        {
            subjectId,
            type,
            content = content ?? UniqueContent(),
            difficulty,
            suggestedPoint,
            visibility,
            explanation,
            options = options ?? DefaultOptions(type),
            tags = tags ?? Array.Empty<string>(),
        };

    public static async Task<HttpResponseMessage> CreateRawAsync(
        HttpClient teacher,
        object payload
    ) => await teacher.PostAsJsonAsync("/api/teacher/questions", payload);

    public static async Task<string> CreateAsync(
        HttpClient teacher,
        string subjectId,
        string type = "SingleChoice",
        string? content = null,
        string difficulty = "Medium",
        string visibility = "Private",
        object[]? options = null,
        string[]? tags = null,
        double suggestedPoint = 1.0,
        string? explanation = null
    )
    {
        var resp = await CreateRawAsync(
            teacher,
            BuildPayload(
                subjectId,
                type,
                content,
                difficulty,
                visibility,
                options,
                tags,
                suggestedPoint,
                explanation
            )
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await AuthHelper.ReadDataAsync(resp);
        return data.GetProperty("publicId").GetString()!;
    }

    public static async Task<JsonElement> ListAsync(HttpClient teacher, string query = "")
    {
        var resp = await teacher.GetAsync($"/api/teacher/questions{query}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }
}
