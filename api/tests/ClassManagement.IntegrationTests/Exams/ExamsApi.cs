using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Exams;

// Shared request helpers for the exam-builder endpoints, mirroring QuestionsApi.
internal static class ExamsApi
{
    public static string UniqueTitle() => $"Exam {Guid.NewGuid():N}";

    public static Task<string> GetActiveSubjectIdAsync(HttpClient client) =>
        QuestionsApi.GetActiveSubjectIdAsync(client);

    // Creates a question in the teacher's bank and returns its publicId (so exams have content).
    public static Task<string> CreateQuestionAsync(
        HttpClient teacher,
        string? subjectId = null,
        string visibility = "Private"
    ) => QuestionsApi.CreateAsync(teacher, subjectId!, "SingleChoice", visibility: visibility);

    public static object BuildPayload(
        string? subjectId = null,
        string? title = null,
        string? description = null,
        string visibility = "Private"
    ) =>
        new
        {
            subjectId,
            title = title ?? UniqueTitle(),
            description,
            visibility,
        };

    public static Task<HttpResponseMessage> CreateRawAsync(HttpClient teacher, object payload) =>
        teacher.PostAsJsonAsync("/api/teacher/exams", payload);

    public static async Task<string> CreateAsync(
        HttpClient teacher,
        string? subjectId = null,
        string? title = null,
        string? description = null,
        string visibility = "Private"
    )
    {
        var resp = await CreateRawAsync(
            teacher,
            BuildPayload(subjectId, title, description, visibility)
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await AuthHelper.ReadDataAsync(resp);
        return data.GetProperty("publicId").GetString()!;
    }

    // Saves the full ordered question list. `questions` = (questionPublicId, point) tuples.
    public static Task<HttpResponseMessage> SaveQuestionsRawAsync(
        HttpClient teacher,
        string examId,
        IEnumerable<(string QuestionId, decimal Point)> questions
    ) =>
        teacher.PutAsJsonAsync(
            $"/api/teacher/exams/{examId}/questions",
            new
            {
                questions = questions.Select(q => new
                {
                    questionId = q.QuestionId,
                    point = q.Point,
                }),
            }
        );

    public static async Task SaveQuestionsAsync(
        HttpClient teacher,
        string examId,
        IEnumerable<(string QuestionId, decimal Point)> questions
    )
    {
        var resp = await SaveQuestionsRawAsync(teacher, examId, questions);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public static async Task<JsonElement> GetAsync(HttpClient teacher, string examId)
    {
        var resp = await teacher.GetAsync($"/api/teacher/exams/{examId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static async Task<JsonElement> PreviewAsync(HttpClient teacher, string examId)
    {
        var resp = await teacher.GetAsync($"/api/teacher/exams/{examId}/preview");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static async Task<JsonElement> ListAsync(HttpClient teacher, string query = "")
    {
        var resp = await teacher.GetAsync($"/api/teacher/exams{query}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }
}
