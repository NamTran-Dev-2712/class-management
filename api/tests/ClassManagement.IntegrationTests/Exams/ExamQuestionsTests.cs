namespace ClassManagement.IntegrationTests.Exams;

public class ExamQuestionsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task SaveQuestions_AddsOrdersAndComputesTotals_BumpsVersion()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);

        var q1 = await ExamsApi.CreateQuestionAsync(teacher, subjectId);
        var q2 = await ExamsApi.CreateQuestionAsync(teacher, subjectId);

        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(q1, 2m), (q2, 3m)]);

        var data = await ExamsApi.GetAsync(teacher, examId);
        data.GetProperty("totalQuestions").GetInt32().Should().Be(2);
        data.GetProperty("totalPoint").GetDecimal().Should().Be(5m);
        data.GetProperty("version").GetInt32().Should().Be(2, "one question-list save = +1");

        var questions = data.GetProperty("questions");
        questions.GetArrayLength().Should().Be(2);
        questions[0].GetProperty("displayOrder").GetInt32().Should().Be(1);
        questions[0].GetProperty("questionPublicId").GetString().Should().Be(q1);
        questions[0].GetProperty("isAvailable").GetBoolean().Should().BeTrue();
        questions[1].GetProperty("displayOrder").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task SaveQuestions_Reorder_PersistsNewOrder()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var q1 = await ExamsApi.CreateQuestionAsync(teacher, subjectId);
        var q2 = await ExamsApi.CreateQuestionAsync(teacher, subjectId);

        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(q1, 1m), (q2, 1m)]);
        // Swap order.
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(q2, 1m), (q1, 1m)]);

        var data = await ExamsApi.GetAsync(teacher, examId);
        var questions = data.GetProperty("questions");
        questions[0].GetProperty("questionPublicId").GetString().Should().Be(q2);
        questions[1].GetProperty("questionPublicId").GetString().Should().Be(q1);
        data.GetProperty("version").GetInt32().Should().Be(3, "two saves after create");
    }

    [Fact]
    public async Task SaveQuestions_DuplicateQuestion_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var q1 = await ExamsApi.CreateQuestionAsync(teacher, subjectId);

        var resp = await ExamsApi.SaveQuestionsRawAsync(teacher, examId, [(q1, 1m), (q1, 2m)]);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaveQuestions_PointZero_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var q1 = await ExamsApi.CreateQuestionAsync(teacher, subjectId);

        var resp = await ExamsApi.SaveQuestionsRawAsync(teacher, examId, [(q1, 0m)]);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaveQuestions_OtherTeachersPrivateQuestion_Returns400()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacherA);

        var examId = await ExamsApi.CreateAsync(teacherA, subjectId);
        // A private question owned by B cannot be added to A's exam.
        var privateOfB = await ExamsApi.CreateQuestionAsync(teacherB, subjectId, "Private");

        var resp = await ExamsApi.SaveQuestionsRawAsync(teacherA, examId, [(privateOfB, 1m)]);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaveQuestions_PublicQuestionOfOtherTeacher_Succeeds()
    {
        var teacherA = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherB = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacherA);

        var examId = await ExamsApi.CreateAsync(teacherA, subjectId);
        var publicOfB = await ExamsApi.CreateQuestionAsync(teacherB, subjectId, "Public");

        await ExamsApi.SaveQuestionsAsync(teacherA, examId, [(publicOfB, 4m)]);

        var data = await ExamsApi.GetAsync(teacherA, examId);
        data.GetProperty("totalQuestions").GetInt32().Should().Be(1);
        data.GetProperty("totalPoint").GetDecimal().Should().Be(4m);
    }

    [Fact]
    public async Task Preview_HidesCorrectAnswerFlags()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await ExamsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var q1 = await ExamsApi.CreateQuestionAsync(teacher, subjectId);
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(q1, 5m)]);

        var preview = await ExamsApi.PreviewAsync(teacher, examId);

        var question = preview.GetProperty("questions")[0];
        question.GetProperty("point").GetDecimal().Should().Be(5m);
        var option = question.GetProperty("options")[0];
        option.TryGetProperty("isCorrect", out _).Should().BeFalse("preview never leaks answers");
        option.GetProperty("content").GetString().Should().NotBeNull();
    }
}
