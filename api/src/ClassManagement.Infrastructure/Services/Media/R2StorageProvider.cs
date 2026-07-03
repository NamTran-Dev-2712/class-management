using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Media;

// Cloudflare R2 storage backend for production (Storage:UseFakeProvider = false). R2 is S3-compatible, so
// this uses the AWS S3 SDK pointed at the account's R2 endpoint — meaning it also works with AWS S3 /
// Backblaze B2 / MinIO without code changes. Credentials come from config/env, never the DB (BR-9-10).
// The S3 client is created lazily so dev/test (which never resolves this adapter) needs no R2 config.
public sealed class R2StorageProvider : IStorageProvider, IDisposable
{
    private readonly StorageOptions _options;
    private readonly Lazy<IAmazonS3> _client;

    public R2StorageProvider(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        _client = new Lazy<IAmazonS3>(CreateClient);
    }

    public StorageProvider Provider => StorageProvider.R2;

    public async Task<PresignUploadResult> PresignPutAsync(
        PresignUploadRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.PresignTtlMinutes);
        var presign = new GetPreSignedUrlRequest
        {
            BucketName = _options.R2.Bucket,
            Key = request.StorageKey,
            Verb = HttpVerb.PUT,
            Expires = expiresAt,
            ContentType = request.ContentType,
        };

        var url = await _client.Value.GetPreSignedURLAsync(presign);
        var headers = new Dictionary<string, string> { ["Content-Type"] = request.ContentType };
        return new PresignUploadResult(url, "PUT", headers, expiresAt);
    }

    public async Task<StorageObjectMetadata?> HeadAsync(
        string storageKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var meta = await _client.Value.GetObjectMetadataAsync(
                _options.R2.Bucket,
                storageKey,
                cancellationToken
            );
            return new StorageObjectMetadata(meta.ContentLength, meta.Headers.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await _client.Value.DeleteObjectAsync(_options.R2.Bucket, storageKey, cancellationToken);
    }

    public string BuildPublicUrl(string storageKey) =>
        $"{_options.R2.PublicBaseUrl.TrimEnd('/')}/{storageKey}";

    private IAmazonS3 CreateClient()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = _options.R2.Endpoint,
            ForcePathStyle = true,
            // R2 has a single region; the SDK just needs a non-empty value for SigV4.
            AuthenticationRegion = "auto",
        };
        return new AmazonS3Client(_options.R2.AccessKey, _options.R2.SecretKey, config);
    }

    public void Dispose()
    {
        if (_client.IsValueCreated)
            _client.Value.Dispose();
    }
}
