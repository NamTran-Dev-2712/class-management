namespace ClassManagement.Infrastructure.Configuration;

// Object-storage provider configuration (MVP-9). Secrets come from config/env — never hard-coded, never
// in the DB (BR-9-10). When UseFakeProvider is true (dev/test) the LocalStorageProvider stands in for R2
// so the whole upload flow is exercisable offline; production sets it false and fills the R2 credentials.
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Use the local-disk simulator instead of the cloud provider (dev/test). Production: false.</summary>
    public bool UseFakeProvider { get; init; } = true;

    /// <summary>How long a presigned upload URL stays valid.</summary>
    public int PresignTtlMinutes { get; init; } = 15;

    public LocalOptions Local { get; init; } = new();
    public R2Options R2 { get; init; } = new();

    // Local-disk backend for dev/test. The presign "URL" points at the app's own local-upload endpoint
    // (guarded by a short-lived HMAC token); confirmed assets are served as static files from RootPath.
    public sealed class LocalOptions
    {
        /// <summary>Filesystem root (relative to the content root, or absolute) where objects are written.</summary>
        public string RootPath { get; init; } = "media-storage";

        /// <summary>Public base URL that maps (via static files) to <see cref="RootPath"/>.</summary>
        public string PublicBaseUrl { get; init; } = "http://localhost:5209/media-files";

        /// <summary>Base URL of the app's local-upload endpoint (dev-only bytes proxy).</summary>
        public string UploadUrlBase { get; init; } = "http://localhost:5209/api/media/local";

        /// <summary>HMAC key signing the local presign token. Override in real deployments.</summary>
        public string SigningKey { get; init; } = "dev-local-storage-signing-key-change-me";
    }

    // Cloudflare R2 via the AWS S3 SDK (R2 is S3-compatible). Endpoint is the account-specific R2 URL.
    public sealed class R2Options
    {
        public string Endpoint { get; init; } = string.Empty;
        public string AccessKey { get; init; } = string.Empty;
        public string SecretKey { get; init; } = string.Empty;
        public string Bucket { get; init; } = string.Empty;

        /// <summary>Public CDN base URL for served objects (egress-free R2 domain).</summary>
        public string PublicBaseUrl { get; init; } = string.Empty;
    }
}
