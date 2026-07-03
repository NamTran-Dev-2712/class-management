using ClassManagement.Application.Exceptions;

// Ownership checks shared by media command handlers (MVP-9). Only the owning teacher or an Admin may
// confirm/delete a media asset (BR-9-07). Students never reach these (controllers are Teacher/Admin only).
internal static class MediaGuard
{
    public static MediaAsset EnsureOwned(MediaAsset? asset, long? currentUserId)
    {
        if (asset is null)
            throw new NotFoundException("Media.NotFound");

        if (currentUserId is null || asset.OwnerId != currentUserId.Value)
            throw new ForbiddenException("Media.NotOwner");

        return asset;
    }

    public static MediaAsset EnsureOwnedOrAdmin(
        MediaAsset? asset,
        long? currentUserId,
        bool isAdmin
    )
    {
        if (asset is null)
            throw new NotFoundException("Media.NotFound");

        if (isAdmin)
            return asset;

        if (currentUserId is null || asset.OwnerId != currentUserId.Value)
            throw new ForbiddenException("Media.NotOwner");

        return asset;
    }
}
