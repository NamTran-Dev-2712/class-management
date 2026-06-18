using ClassManagement.IntegrationTests.Classroom;
using ClassManagement.IntegrationTests.Questions;

namespace ClassManagement.IntegrationTests.Assignments;

public class AssignmentPermissionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<(HttpClient Teacher, string AssignmentId)> CreateAssignmentAsync()
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
        return (teacher, assignmentId);
    }

    [Fact]
    public async Task OtherTeacher_CannotViewAssignment_Returns403()
    {
        var (_, assignmentId) = await CreateAssignmentAsync();
        var otherTeacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");

        var resp = await otherTeacher.GetAsync($"/api/teacher/assignments/{assignmentId}");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Student_CannotCreateAssignment_Returns403()
    {
        var student = await AuthHelper.CreateRoleClientAsync(_factory, "Student");

        var resp = await student.PostAsJsonAsync(
            "/api/teacher/assignments",
            AssignmentsApi.BuildPayload(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CanListAllAssignments_Returns200()
    {
        await CreateAssignmentAsync();
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);

        var resp = await admin.GetAsync("/api/admin/assignments?pageSize=50");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Teacher_CannotUseStudentStartEndpoint_Returns403()
    {
        var (teacher, assignmentId) = await CreateAssignmentAsync();
        await AssignmentsApi.PublishAsync(teacher, assignmentId);

        // Teacher hitting the student-only start endpoint is rejected by the role guard.
        var resp = await teacher.PostAsync(
            $"/api/student/assignments/{assignmentId}/attempts",
            null
        );

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
