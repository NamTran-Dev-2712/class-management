using System.Net.Http.Headers;

namespace ClassManagement.IntegrationTests.Infrastructure;

// Drives the full MVP-9 upload flow against the Local storage backend: presign → PUT bytes to the
// local-upload endpoint → confirm. Returns the confirmed asset's public id + URL. Reusable by media,
// question-media, and snapshot-freeze tests.
public static class MediaHelper
{
    public static readonly byte[] SamplePng =
    [
        0x89,
        0x50,
        0x4E,
        0x47,
        0x0D,
        0x0A,
        0x1A,
        0x0A,
        0x00,
        0x01,
        0x02,
        0x03,
    ];

    public static async Task<JsonElement> PresignAsync(
        HttpClient client,
        string fileName,
        string contentType,
        long byteSize
    )
    {
        var response = await client.PostAsJsonAsync(
            "/api/teacher/media/presign",
            new
            {
                fileName,
                contentType,
                byteSize,
            }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(response);
    }

    public static async Task PutBytesAsync(
        HttpClient client,
        string uploadUrl,
        byte[] bytes,
        string contentType
    )
    {
        var relative = new Uri(uploadUrl).PathAndQuery;
        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var response = await client.PutAsync(relative, content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public static Task<HttpResponseMessage> ConfirmAsync(HttpClient client, Guid mediaPublicId) =>
        client.PostAsync($"/api/teacher/media/{mediaPublicId}/confirm", null);

    // Full happy-path upload; returns (publicId, url) of the Confirmed asset.
    public static async Task<(Guid PublicId, string Url)> UploadAsync(
        HttpClient client,
        byte[]? bytes = null,
        string contentType = "image/png",
        string fileName = "sample.png"
    )
    {
        bytes ??= SamplePng;
        var presign = await PresignAsync(client, fileName, contentType, bytes.Length);
        var mediaPublicId = presign.GetProperty("mediaPublicId").GetGuid();
        var uploadUrl = presign.GetProperty("uploadUrl").GetString()!;

        await PutBytesAsync(client, uploadUrl, bytes, contentType);

        var confirmResponse = await ConfirmAsync(client, mediaPublicId);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(confirmResponse);
        return (mediaPublicId, data.GetProperty("url").GetString()!);
    }
}
