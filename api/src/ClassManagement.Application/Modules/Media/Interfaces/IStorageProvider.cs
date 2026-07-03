// Object-storage abstraction (MVP-9). Keeps Application transport-agnostic; concrete adapters (Local disk
// for dev/test, Cloudflare R2 via the AWS S3 SDK for prod) live in Infrastructure. The provider never
// touches the DB — it only mints a presigned upload URL, checks object existence, deletes an object, and
// builds the public CDN URL for a key. Bytes flow client → storage directly, never through the API.
public interface IStorageProvider
{
    StorageProvider Provider { get; }

    // Mint a short-lived presigned URL the browser PUTs the file bytes to (BR-9-02). The key + content
    // type + size have already been validated by the handler.
    Task<PresignUploadResult> PresignPutAsync(
        PresignUploadRequest request,
        CancellationToken cancellationToken = default
    );

    // Return the stored object's metadata, or null when it does not exist yet (used to gate Confirm,
    // BR-9-03). Best-effort content-type/size for a re-check against the declared values.
    Task<StorageObjectMetadata?> HeadAsync(
        string storageKey,
        CancellationToken cancellationToken = default
    );

    // Remove the physical object (cleanup job). Idempotent — deleting a missing key is a no-op.
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    // Public CDN URL for a stored object (PublicBaseUrl + key). Never exposes the raw key semantics.
    string BuildPublicUrl(string storageKey);
}

// Resolves the storage adapter for a provider and reports which provider new uploads use (the Local
// simulator when Storage:UseFakeProvider is on, else the configured cloud provider). Mirrors
// IPaymentProviderResolver so swapping providers never touches Application.
public interface IStorageProviderResolver
{
    IStorageProvider Resolve(StorageProvider provider);

    // The provider assigned to new uploads (recorded on the MediaAsset at presign time).
    StorageProvider ActiveProvider { get; }
}

// What to presign: the target key, the declared MIME type, and the expected byte size.
public sealed record PresignUploadRequest(string StorageKey, string ContentType, long ByteSize);

// The presigned upload: the URL to PUT to, the HTTP method, any headers the client must echo, and when
// the URL expires. The client uploads with a bare HTTP client (no auth cookies).
public sealed record PresignUploadResult(
    string UploadUrl,
    string HttpMethod,
    IReadOnlyDictionary<string, string> Headers,
    DateTime ExpiresAt
);

// Minimal object metadata read back on Confirm (BR-9-03).
public sealed record StorageObjectMetadata(long ByteSize, string? ContentType);
