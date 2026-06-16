namespace ClassManagement.IntegrationTests.Questions;

public class QuestionCrudTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Theory]
    [InlineData("SingleChoice")]
    [InlineData("MultipleChoice")]
    [InlineData("TrueFalse")]
    [InlineData("ShortWriting")]
    [InlineData("LongWriting")]
    public async Task Create_AsTeacher_AllTypes_Returns201_AndAppearsInList(string type)
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);

        var id = await QuestionsApi.CreateAsync(teacher, subjectId, type);

        Guid.TryParse(id, out _).Should().BeTrue();

        var detail = await teacher.GetAsync($"/api/teacher/questions/{id}");
        detail.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(detail);
        data.GetProperty("type").GetString().Should().Be(type);
    }

    [Fact]
    public async Task Create_SingleChoice_WithoutCorrectAnswer_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);

        var resp = await QuestionsApi.CreateRawAsync(
            teacher,
            QuestionsApi.BuildPayload(
                subjectId,
                "SingleChoice",
                options:
                [
                    new { content = "A", isCorrect = false },
                    new { content = "B", isCorrect = false },
                ]
            )
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_MultipleChoice_WithNoCorrectAnswer_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);

        var resp = await QuestionsApi.CreateRawAsync(
            teacher,
            QuestionsApi.BuildPayload(
                subjectId,
                "MultipleChoice",
                options:
                [
                    new { content = "A", isCorrect = false },
                    new { content = "B", isCorrect = false },
                ]
            )
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_TrueFalse_WithThreeOptions_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);

        var resp = await QuestionsApi.CreateRawAsync(
            teacher,
            QuestionsApi.BuildPayload(
                subjectId,
                "TrueFalse",
                options:
                [
                    new { content = "True", isCorrect = true },
                    new { content = "False", isCorrect = false },
                    new { content = "Maybe", isCorrect = false },
                ]
            )
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithTooShortContent_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);

        var resp = await QuestionsApi.CreateRawAsync(
            teacher,
            QuestionsApi.BuildPayload(subjectId, "ShortWriting", content: "short")
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_NormalizesTags_LowercaseAndDeduped()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);

        var id = await QuestionsApi.CreateAsync(
            teacher,
            subjectId,
            "ShortWriting",
            tags: ["  Algebra Basics ", "ALGEBRA-BASICS", "Hard!!"]
        );

        var data = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/questions/{id}")
        );
        var tags = data.GetProperty("tags").EnumerateArray().Select(t => t.GetString()).ToList();
        tags.Should().Contain("algebra-basics");
        tags.Should().Contain("hard");
        // "Algebra Basics" and "ALGEBRA-BASICS" both normalize to the same slug → deduped.
        tags.Count(t => t == "algebra-basics").Should().Be(1);
    }

    [Fact]
    public async Task Update_AsOwner_ReplacesOptionsAndPersists()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var id = await QuestionsApi.CreateAsync(teacher, subjectId, "SingleChoice");

        var newContent = QuestionsApi.UniqueContent();
        var update = await teacher.PutAsJsonAsync(
            $"/api/teacher/questions/{id}",
            QuestionsApi.BuildPayload(
                subjectId,
                "SingleChoice",
                content: newContent,
                difficulty: "Hard",
                options:
                [
                    new { content = "X", isCorrect = false },
                    new { content = "Y", isCorrect = true },
                    new { content = "Z", isCorrect = false },
                ]
            )
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/questions/{id}")
        );
        data.GetProperty("content").GetString().Should().Be(newContent);
        data.GetProperty("difficulty").GetString().Should().Be("Hard");
        data.GetProperty("options").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Delete_AsOwner_SoftDeletes_AndRemovesFromList()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var content = QuestionsApi.UniqueContent();
        var id = await QuestionsApi.CreateAsync(
            teacher,
            subjectId,
            "ShortWriting",
            content: content
        );

        var del = await teacher.DeleteAsync($"/api/teacher/questions/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await QuestionsApi.ListAsync(
            teacher,
            $"?searchTerm={Uri.EscapeDataString(content)}"
        );
        list.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Create_WithoutSubject_Returns201_AndSubjectIsNull()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        // Subject is optional — a teacher can create questions before any catalog subject exists.
        var resp = await QuestionsApi.CreateRawAsync(
            teacher,
            QuestionsApi.BuildPayload(subjectId: null, type: "ShortWriting")
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = (await AuthHelper.ReadDataAsync(resp)).GetProperty("publicId").GetString()!;

        var data = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/questions/{id}")
        );
        // Null props are omitted from the response envelope (ignore-null serialization).
        var hasSubject =
            data.TryGetProperty("subjectPublicId", out var sp)
            && sp.ValueKind != JsonValueKind.Null;
        hasSubject.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ClearingSubject_DetachesIt()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var id = await QuestionsApi.CreateAsync(teacher, subjectId, "SingleChoice");

        // Re-save with no subject.
        var update = await teacher.PutAsJsonAsync(
            $"/api/teacher/questions/{id}",
            QuestionsApi.BuildPayload(subjectId: null, type: "SingleChoice")
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/questions/{id}")
        );
        var hasSubject =
            data.TryGetProperty("subjectPublicId", out var sp)
            && sp.ValueKind != JsonValueKind.Null;
        hasSubject.Should().BeFalse();
    }
}
