using ClassManagement.Application.Common.Constants;

namespace ClassManagement.IntegrationTests.Media;

public class DeleteMediaTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<HttpClient> TeacherAsync() =>
        await AuthHelper.CreateRoleClientAsync(_factory, ApplicationRoles.Teacher);

    [Fact]
    public async Task Delete_AsOwner_Returns200AndRemovesFromLibrary()
    {
        var client = await TeacherAsync();
        var (publicId, _) = await MediaHelper.UploadAsync(client);

        var delete = await client.DeleteAsync($"/api/teacher/media/{publicId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await client.GetAsync("/api/teacher/media?pageSize=50");
        var data = await AuthHelper.ReadDataAsync(list);
        data.GetProperty("items")
            .EnumerateArray()
            .Should()
            .NotContain(m => m.GetProperty("publicId").GetGuid() == publicId);
    }

    [Fact]
    public async Task Delete_AsNonOwnerTeacher_Returns403()
    {
        var owner = await TeacherAsync();
        var (publicId, _) = await MediaHelper.UploadAsync(owner);

        var other = await TeacherAsync();
        var delete = await other.DeleteAsync($"/api/teacher/media/{publicId}");
        delete.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_Returns200()
    {
        var owner = await TeacherAsync();
        var (publicId, _) = await MediaHelper.UploadAsync(owner);

        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var delete = await admin.DeleteAsync($"/api/admin/media/{publicId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Usage_ReflectsUploadedBytes()
    {
        var client = await TeacherAsync();
        await MediaHelper.UploadAsync(client, MediaHelper.SamplePng);

        var usage = await client.GetAsync("/api/teacher/media/usage");
        usage.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(usage);
        data.GetProperty("usedBytes").GetInt64().Should().BeGreaterThan(0);
        data.GetProperty("limitBytes").GetInt64().Should().BeGreaterThan(0);
    }
}
