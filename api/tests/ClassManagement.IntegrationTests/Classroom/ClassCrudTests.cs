namespace ClassManagement.IntegrationTests.Classroom;

public class ClassCrudTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Create_AsTeacher_WithFreeTextSubject_Returns201_AndAppearsInList()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var name = ClassroomApi.UniqueName();

        var classId = await ClassroomApi.CreateClassAsync(teacher, name, subjectName: "Astronomy");

        Guid.TryParse(classId, out _).Should().BeTrue();

        var list = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/classes?searchTerm={name}")
        );
        list.GetProperty("totalCount").GetInt32().Should().Be(1);
        var item = list.GetProperty("items")[0];
        item.GetProperty("name").GetString().Should().Be(name);
        item.GetProperty("subjectName").GetString().Should().Be("Astronomy");
        item.GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task Create_WithCatalogSubject_SnapshotsNameAndLinksId()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var (subjectId, subjectName) = await ClassroomApi.GetAnActiveSubjectAsync(teacher);

        var classId = await ClassroomApi.CreateClassAsync(
            teacher,
            subjectName: null,
            subjectId: subjectId
        );

        var detail = await ClassroomApi.GetTeacherClassAsync(teacher, classId);
        detail.GetProperty("subjectName").GetString().Should().Be(subjectName);
        detail.GetProperty("subjectId").GetString().Should().Be(subjectId);
    }

    [Fact]
    public async Task Create_DuplicateNamePerOwner_Returns409()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var name = ClassroomApi.UniqueName();
        await ClassroomApi.CreateClassAsync(teacher, name);

        var resp = await teacher.PostAsJsonAsync(
            "/api/teacher/classes",
            new { name, subjectName = "Other" }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithInvalidName_Returns400()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var resp = await teacher.PostAsJsonAsync(
            "/api/teacher/classes",
            new { name = "A", subjectName = "X" }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithInactiveCatalogSubject_Returns400()
    {
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var create = await admin.PostAsJsonAsync(
            "/api/subjects",
            new
            {
                name = $"Inactive_{Guid.NewGuid():N}",
                description = (string?)null,
                isActive = false,
                displayOrder = 0,
            }
        );
        var subjectId = (await AuthHelper.ReadDataAsync(create))
            .GetProperty("publicId")
            .GetString();

        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var resp = await teacher.PostAsJsonAsync(
            "/api/teacher/classes",
            new { name = ClassroomApi.UniqueName(), subjectId }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_AsOwner_PersistsChanges()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacher);
        var newName = ClassroomApi.UniqueName();

        var update = await teacher.PutAsJsonAsync(
            $"/api/teacher/classes/{classId}",
            new
            {
                name = newName,
                description = "Updated",
                subjectName = "Physics",
            }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await ClassroomApi.GetTeacherClassAsync(teacher, classId);
        detail.GetProperty("name").GetString().Should().Be(newName);
        detail.GetProperty("subjectName").GetString().Should().Be("Physics");
    }

    [Fact]
    public async Task Update_AsNonOwnerTeacher_Returns403()
    {
        var owner = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(owner);

        var otherTeacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var resp = await otherTeacher.PutAsJsonAsync(
            $"/api/teacher/classes/{classId}",
            new { name = ClassroomApi.UniqueName(), subjectName = "X" }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDetail_AsNonOwnerTeacher_Returns403()
    {
        var owner = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(owner);

        var otherTeacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var resp = await otherTeacher.GetAsync($"/api/teacher/classes/{classId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ArchiveThenUnarchive_TogglesStatus()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var classId = await ClassroomApi.CreateClassAsync(teacher);

        (await teacher.PostAsync($"/api/teacher/classes/{classId}/archive", null))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);
        (await ClassroomApi.GetTeacherClassAsync(teacher, classId))
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("Archived");

        (await teacher.PostAsync($"/api/teacher/classes/{classId}/unarchive", null))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);
        (await ClassroomApi.GetTeacherClassAsync(teacher, classId))
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("Active");
    }

    [Fact]
    public async Task Archived_ListFilterByStatus_Works()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var name = ClassroomApi.UniqueName();
        var classId = await ClassroomApi.CreateClassAsync(teacher, name);
        await teacher.PostAsync($"/api/teacher/classes/{classId}/archive", null);

        var archived = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/classes?status=Archived&searchTerm={name}")
        );
        archived.GetProperty("totalCount").GetInt32().Should().Be(1);

        var active = await AuthHelper.ReadDataAsync(
            await teacher.GetAsync($"/api/teacher/classes?status=Active&searchTerm={name}")
        );
        active.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Admin_GetAllClasses_SeesTeacherClass()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var name = ClassroomApi.UniqueName();
        await ClassroomApi.CreateClassAsync(teacher, name);

        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var data = await AuthHelper.ReadDataAsync(
            await admin.GetAsync($"/api/admin/classes?searchTerm={name}")
        );
        data.GetProperty("totalCount").GetInt32().Should().Be(1);
        data.GetProperty("items")[0]
            .GetProperty("ownerName")
            .GetString()
            .Should()
            .NotBeNullOrEmpty();
    }
}
