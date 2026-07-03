// Small helpers for building storage keys (MVP-9). The key is opaque and derived from the asset's public
// id (a random Guid) so it exposes nothing internal even though it becomes part of the public CDN URL.
internal static class MediaAssembler
{
    // media/{mediaPublicId}.{ext} — flat, unguessable, no owner id leaked into the public URL.
    public static string BuildStorageKey(Guid mediaPublicId, string extension) =>
        $"media/{mediaPublicId:N}.{extension}";

    // A safe lowercase file extension, derived from content-type first, then the original file name.
    public static string ResolveExtension(string? fileName, string contentType)
    {
        var fromType = FromContentType(contentType);
        if (fromType is not null)
            return fromType;

        var ext = Path.GetExtension(fileName ?? string.Empty).TrimStart('.').ToLowerInvariant();
        if (ext.Length is > 0 and <= 5 && ext.All(char.IsLetterOrDigit))
            return ext;

        return "bin";
    }

    private static string? FromContentType(string contentType) =>
        contentType.Trim().ToLowerInvariant() switch
        {
            "image/png" => "png",
            "image/jpeg" => "jpg",
            "image/gif" => "gif",
            "image/webp" => "webp",
            "audio/mpeg" => "mp3",
            "audio/mp4" => "m4a",
            "audio/ogg" => "ogg",
            "audio/wav" => "wav",
            "audio/webm" => "weba",
            "video/mp4" => "mp4",
            "video/webm" => "webm",
            "video/ogg" => "ogv",
            _ => null,
        };
}
