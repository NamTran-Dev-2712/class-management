// Dev/test-only bytes sink for the LocalStorageProvider (MVP-9). In production uploads go straight to R2
// via a presigned URL and never touch the API; the Local backend can't mint a real presigned URL, so its
// presign points back at the app's local-upload endpoint which streams the bytes through this store. The
// token (HMAC over key+expiry) is the trust boundary — mirrors the presigned URL's signature.
public interface ILocalUploadStore
{
    // Validate the signed token, then persist the stream under the key. Returns false when the token is
    // invalid or expired (the controller answers 403), true on a successful write.
    Task<bool> TrySaveAsync(
        string key,
        long expiresAtUnix,
        string signature,
        Stream content,
        CancellationToken cancellationToken = default
    );
}
