using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Exams;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

public class AssignmentLifecycleTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Create_FromOwnExamAndClass_Returns201()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await AssignmentsApi.CreateExamWithQuestionsAsync(
            teacher,
            subjectId,
            ["SingleChoice"]
        );
        var classId = await ClassroomApi.CreateClassAsync(teacher);

        var resp = await AssignmentsApi.CreateRawAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId)
        );

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_FromExamWithNoQuestions_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var emptyExamId = await ExamsApi.CreateAsync(teacher, subjectId); // no questions saved
        var classId = await ClassroomApi.CreateClassAsync(teacher);

        var resp = await AssignmentsApi.CreateRawAsync(
            teacher,
            AssignmentsApi.BuildPayload(emptyExamId, classId)
        );

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Publish_BuildsSnapshot_AndOpens()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await AssignmentsApi.CreateExamWithQuestionsAsync(
            teacher,
            subjectId,
            ["SingleChoice", "TrueFalse"],
            point: 3m
        );
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId)
        );

        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var detail = await AssignmentsApi.GetTeacherAsync(teacher, assignmentId);
        detail.GetProperty("status").GetString().Should().Be("Open");
        detail.GetProperty("totalQuestions").GetInt32().Should().Be(2);
        detail.GetProperty("totalPoint").GetDecimal().Should().Be(6m);
        detail.GetProperty("questions").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Publish_Twice_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await AssignmentsApi.CreateExamWithQuestionsAsync(
            teacher,
            subjectId,
            ["SingleChoice"]
        );
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId)
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var second = await AssignmentsApi.PublishRawAsync(teacher, assignmentId);

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Snapshot_IsImmutable_WhenExamEditedAfterPublish()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await ExamsApi.CreateAsync(teacher, subjectId);
        var q1 = await QuestionsApi.CreateAsync(teacher, subjectId, "SingleChoice");
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(q1, 2m)]);

        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId)
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        // Edit the source exam after publish: add a second question + change points.
        var q2 = await QuestionsApi.CreateAsync(teacher, subjectId, "SingleChoice");
        await ExamsApi.SaveQuestionsAsync(teacher, examId, [(q1, 5m), (q2, 5m)]);

        // The assignment's snapshot is unchanged.
        var detail = await AssignmentsApi.GetTeacherAsync(teacher, assignmentId);
        detail.GetProperty("totalQuestions").GetInt32().Should().Be(1);
        detail.GetProperty("totalPoint").GetDecimal().Should().Be(2m);
    }

    [Fact]
    public async Task Delete_WithStudentAttempt_Returns409()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subjectId = await QuestionsApi.GetActiveSubjectIdAsync(teacher);
        var examId = await AssignmentsApi.CreateExamWithQuestionsAsync(
            teacher,
            subjectId,
            ["SingleChoice"]
        );
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var assignmentId = await AssignmentsApi.CreateAsync(
            teacher,
            AssignmentsApi.BuildPayload(examId, classId)
        );
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");
        await AssignmentsApi.ApproveStudentAsync(teacher, student, classId);
        await AssignmentsApi.StartAsync(student, assignmentId);

        var del = await teacher.DeleteAsync($"/api/teacher/assignments/{assignmentId}");

        del.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
