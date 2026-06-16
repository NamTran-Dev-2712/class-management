namespace ClassManagement.IntegrationTests.Questions;

public class QuestionPublicBrowseTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task PublicQuestion_IsVisibleToAnotherTeacher_AndDuplicateCreatesIndependentPrivateCopy()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacherA);

        var content = QuestionsApi.UniqueContent();
        var originalId = await QuestionsApi.CreateAsync(
            teacherA,
            subjectId,
            "SingleChoice",
            content: content,
            visibility: "Public"
        );

        var search = $"?searchTerm={Uri.EscapeDataString(content)}";

        // Teacher B sees it in the public pool…
        var publicList = await AuthHelper.ReadDataAsync(
            await teacherB.GetAsync($"/api/teacher/questions/public{search}")
        );
        publicList.GetProperty("totalCount").GetInt32().Should().Be(1);

        // …but not in their own bank yet.
        var ownBefore = await QuestionsApi.ListAsync(teacherB, search);
        ownBefore.GetProperty("totalCount").GetInt32().Should().Be(0);

        // Teacher B duplicates it.
        var dupResp = await teacherB.PostAsync(
            $"/api/teacher/questions/{originalId}/duplicate",
            null
        );
        dupResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var copyId = (await AuthHelper.ReadDataAsync(dupResp)).GetProperty("publicId").GetString()!;
        copyId.Should().NotBe(originalId);

        // The copy is in B's bank and is Private.
        var copy = await AuthHelper.ReadDataAsync(
            await teacherB.GetAsync($"/api/teacher/questions/{copyId}")
        );
        copy.GetProperty("visibility").GetString().Should().Be("Private");
        copy.GetProperty("content").GetString().Should().Be(content);

        // Editing the copy does not affect Teacher A's original.
        var edited = QuestionsApi.UniqueContent();
        var update = await teacherB.PutAsJsonAsync(
            $"/api/teacher/questions/{copyId}",
            QuestionsApi.BuildPayload(subjectId, "SingleChoice", content: edited)
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var original = await AuthHelper.ReadDataAsync(
            await teacherA.GetAsync($"/api/teacher/questions/{originalId}")
        );
        original.GetProperty("content").GetString().Should().Be(content);
    }

    [Fact]
    public async Task TeacherCannotSeeAnotherTeachersPrivateQuestion()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacherA);
        var id = await QuestionsApi.CreateAsync(teacherA, subjectId, "ShortWriting");

        // Private question — B can't read it via the public detail endpoint (404, no leak).
        var resp = await teacherB.GetAsync($"/api/teacher/questions/public/{id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
