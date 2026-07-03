using ClassManagement.Application.Common.Constants;

namespace ClassManagement.IntegrationTests.Media;

public class PresignTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<HttpClient> TeacherAsync() =>
        await AuthHelper.CreateRoleClientAsync(_factory, ApplicationRoles.Teacher);

    [Fact]
    public async Task Presign_WithValidImage_Returns200AndUploadUrl()
    {
        var client = await TeacherAsync();

        var response = await client.PostAsJsonAsync(
            "/api/teacher/media/presign",
            new
            {
                fileName = "diagram.png",
                contentType = "image/png",
                byteSize = 1024,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.GetProperty("mediaPublicId").GetGuid().Should().NotBeEmpty();
        data.GetProperty("uploadUrl").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("httpMethod").GetString().Should().Be("PUT");
    }

    [Fact]
    public async Task Presign_WithDisallowedMime_Returns400()
    {
        var client = await TeacherAsync();

        var response = await client.PostAsJsonAsync(
            "/api/teacher/media/presign",
            new
            {
                fileName = "malware.exe",
                contentType = "application/x-msdownload",
                byteSize = 1024,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Presign_WithFileOverKindCap_Returns400()
    {
        var client = await TeacherAsync();

        // Image cap default is 5 MB; 10 MB exceeds it.
        var response = await client.PostAsJsonAsync(
            "/api/teacher/media/presign",
            new
            {
                fileName = "huge.png",
                contentType = "image/png",
                byteSize = 10L * 1024 * 1024,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Presign_ExceedingTeacherQuota_Returns403()
    {
        var client = await TeacherAsync();

        // Free quota default is 500 MB; a single 600 MB file (also over per-file cap) → reject. Use audio
        // whose cap is 20 MB so we isolate the quota path from the per-file path by picking a size within
        // no single cap but over total quota is hard; instead assert the quota guard triggers with a file
        // just under the video per-file cap repeated conceptually — here we simply request > quota via a
        // video within its 100MB cap but exceeding remaining quota after filling it is covered elsewhere.
        // Simplest deterministic check: request a video of 100MB six times → the 6th exceeds 500MB quota.
        for (var i = 0; i < 5; i++)
        {
            var ok = await client.PostAsJsonAsync(
                "/api/teacher/media/presign",
                new
                {
                    fileName = $"v{i}.mp4",
                    contentType = "video/mp4",
                    byteSize = 100L * 1024 * 1024,
                }
            );
            ok.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var overflow = await client.PostAsJsonAsync(
            "/api/teacher/media/presign",
            new
            {
                fileName = "v5.mp4",
                contentType = "video/mp4",
                byteSize = 100L * 1024 * 1024,
            }
        );
        overflow.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Presign_AsStudent_Returns403()
    {
        var client = await AuthHelper.CreateRoleClientAsync(_factory, ApplicationRoles.Student);

        var response = await client.PostAsJsonAsync(
            "/api/teacher/media/presign",
            new
            {
                fileName = "x.png",
                contentType = "image/png",
                byteSize = 1024,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
