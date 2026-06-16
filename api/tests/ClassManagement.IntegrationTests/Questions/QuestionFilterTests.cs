namespace ClassManagement.IntegrationTests.Questions;

public class QuestionFilterTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task TeacherBank_FiltersByType_Difficulty_Tag_AndKeyword()
    {
        // Fresh teacher → their bank contains exactly the questions created here.
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);

        var keyword = $"ZEBRA{Guid.NewGuid():N}";

        await QuestionsApi.CreateAsync(
            teacher,
            subjectId,
            "SingleChoice",
            content: $"A {keyword} riddle that is long enough.",
            difficulty: "Easy",
            tags: ["geometry"]
        );
        await QuestionsApi.CreateAsync(
            teacher,
            subjectId,
            "MultipleChoice",
            difficulty: "Hard",
            tags: ["algebra"]
        );
        await QuestionsApi.CreateAsync(
            teacher,
            subjectId,
            "ShortWriting",
            difficulty: "Medium",
            tags: ["algebra"]
        );

        // Type filter.
        (await QuestionsApi.ListAsync(teacher, "?type=SingleChoice"))
            .GetProperty("totalCount")
            .GetInt32()
            .Should()
            .Be(1);

        // Difficulty filter.
        (await QuestionsApi.ListAsync(teacher, "?difficulty=Hard"))
            .GetProperty("totalCount")
            .GetInt32()
            .Should()
            .Be(1);

        // Tag filter.
        (await QuestionsApi.ListAsync(teacher, "?tag=algebra"))
            .GetProperty("totalCount")
            .GetInt32()
            .Should()
            .Be(2);

        // Keyword (content substring) search.
        (await QuestionsApi.ListAsync(teacher, $"?searchTerm={keyword}"))
            .GetProperty("totalCount")
            .GetInt32()
            .Should()
            .Be(1);

        // Subject filter (all three share the same subject).
        (await QuestionsApi.ListAsync(teacher, $"?subjectId={subjectId}"))
            .GetProperty("totalCount")
            .GetInt32()
            .Should()
            .Be(3);
    }
}
