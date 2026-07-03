using System.Security.Cryptography;
using System.Text;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Media;

// Local-disk storage backend for dev/test (Storage:UseFakeProvider = true). Since a local folder can't
// mint a real presigned URL, PresignPut returns a URL to the app's own local-upload endpoint carrying an
// HMAC token (key + expiry). Bytes are proxied through that endpoint (TrySaveAsync) — a deliberate dev-only
// exception to BR-9-02; in production the R2 adapter does true client→storage uploads. Confirmed objects
// are served as static files from RootPath (mapped to Local:PublicBaseUrl).
public sealed class LocalStorageProvider : IStorageProvider, ILocalUploadStore
{
    private readonly StorageOptions _options;
    private readonly string _root;

    public LocalStorageProvider(IOptions<StorageOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        var configured = _options.Local.RootPath;
        _root = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured);
    }

    public StorageProvider Provider => StorageProvider.Local;

    public Task<PresignUploadResult> PresignPutAsync(
        PresignUploadRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.PresignTtlMinutes);
        var exp = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();
        var signature = Sign(request.StorageKey, exp);

        var uploadUrl =
            $"{_options.Local.UploadUrlBase}"
            + $"?key={Uri.EscapeDataString(request.StorageKey)}"
            + $"&exp={exp}"
            + $"&sig={signature}";

        var headers = new Dictionary<string, string> { ["Content-Type"] = request.ContentType };
        return Task.FromResult(new PresignUploadResult(uploadUrl, "PUT", headers, expiresAt));
    }

    public Task<StorageObjectMetadata?> HeadAsync(
        string storageKey,
        CancellationToken cancellationToken = default
    )
    {
        var path = ResolvePath(storageKey);
        if (!File.Exists(path))
            return Task.FromResult<StorageObjectMetadata?>(null);

        var info = new FileInfo(path);
        return Task.FromResult<StorageObjectMetadata?>(
            new StorageObjectMetadata(info.Length, null)
        );
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    public string BuildPublicUrl(string storageKey) =>
        $"{_options.Local.PublicBaseUrl.TrimEnd('/')}/{storageKey}";

    // --- ILocalUploadStore (dev-only bytes sink) ---

    public async Task<bool> TrySaveAsync(
        string key,
        long expiresAtUnix,
        string signature,
        Stream content,
        CancellationToken cancellationToken = default
    )
    {
        if (DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix) < DateTimeOffset.UtcNow)
            return false;
        if (!FixedTimeEquals(signature, Sign(key, expiresAtUnix)))
            return false;

        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
        return true;
    }

    // Keys are app-generated (media/{guid}/{guid}.ext); still guard against path traversal.
    private string ResolvePath(string storageKey)
    {
        var safe = storageKey.Replace('\\', '/').TrimStart('/');
        var combined = Path.GetFullPath(Path.Combine(_root, safe));
        var rootFull = Path.GetFullPath(_root);
        if (!combined.StartsWith(rootFull, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid storage key.");
        return combined;
    }

    private string Sign(string key, long exp)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.Local.SigningKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{key}|{exp}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b)
        );
}
