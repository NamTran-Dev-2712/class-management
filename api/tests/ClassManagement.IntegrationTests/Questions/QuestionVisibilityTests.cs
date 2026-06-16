namespace ClassManagement.IntegrationTests.Questions;

public class QuestionVisibilityTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task SetVisibility_ToPublic_MakesItAppearInPublicPool_ThenPrivateHidesIt()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var content = QuestionsApi.UniqueContent();
        var id = await QuestionsApi.CreateAsync(
            teacher,
            subjectId,
            "SingleChoice",
            content: content
        );

        var search = $"?searchTerm={Uri.EscapeDataString(content)}";

        // Initially Private → not in the public pool.
        var before = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/questions/public{search}")
        );
        before.GetProperty("totalCount").GetInt32().Should().Be(0);

        // Flip Public.
        var patch = await teacher.PatchAsJsonAsync(
            $"/api/teacher/questions/{id}/visibility",
            new { visibility = "Public" }
        );
        patch.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterPublic = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/questions/public{search}")
        );
        afterPublic.GetProperty("totalCount").GetInt32().Should().Be(1);

        // Flip back to Private → gone from the public pool.
        var patchBack = await teacher.PatchAsJsonAsync(
            $"/api/teacher/questions/{id}/visibility",
            new { visibility = "Private" }
        );
        patchBack.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterPrivate = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/questions/public{search}")
        );
        afterPrivate.GetProperty("totalCount").GetInt32().Should().Be(0);
    }
}
