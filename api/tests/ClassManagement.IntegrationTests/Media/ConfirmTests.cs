using ClassManagement.Application.Common.Constants;

namespace ClassManagement.IntegrationTests.Media;

public class ConfirmTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<HttpClient> TeacherAsync() =>
        await AuthHelper.CreateRoleClientAsync(_factory, ApplicationRoles.Teacher);

    [Fact]
    public async Task Confirm_AfterUpload_MarksConfirmedAndReturnsUrl()
    {
        var client = await TeacherAsync();

        var (publicId, url) = await MediaHelper.UploadAsync(client);

        publicId.Should().NotBeEmpty();
        url.Should().Contain(publicId.ToString("N"));

        // Confirm is reflected in the library list with Confirmed status.
        var list = await client.GetAsync("/api/teacher/media?pageSize=50");
        var data = await AuthHelper.ReadDataAsync(list);
        var items = data.GetProperty("items");
        items
            .EnumerateArray()
            .Should()
            .Contain(m =>
                m.GetProperty("publicId").GetGuid() == publicId
                && m.GetProperty("status").GetString() == "Confirmed"
            );
    }

    [Fact]
    public async Task Confirm_WithoutUploadedObject_Returns400AndStaysPending()
    {
        var client = await TeacherAsync();

        var presign = await MediaHelper.PresignAsync(client, "x.png", "image/png", 1024);
        var mediaPublicId = presign.GetProperty("mediaPublicId").GetGuid();

        // Confirm without PUTting bytes → object missing → 400.
        var confirm = await MediaHelper.ConfirmAsync(client, mediaPublicId);
        confirm.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Still listed as Pending (retryable).
        var list = await client.GetAsync("/api/teacher/media?status=Pending&pageSize=50");
        var data = await AuthHelper.ReadDataAsync(list);
        data.GetProperty("items")
            .EnumerateArray()
            .Should()
            .Contain(m => m.GetProperty("publicId").GetGuid() == mediaPublicId);
    }

    [Fact]
    public async Task Confirm_IsIdempotent()
    {
        var client = await TeacherAsync();
        var (publicId, _) = await MediaHelper.UploadAsync(client);

        var again = await MediaHelper.ConfirmAsync(client, publicId);
        again.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
