using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Interfaces.Identity;
using ClassManagement.Application.Modules.Media.DTOs;

// Verifies the object exists on storage before marking the asset Confirmed (BR-9-03). If the object is
// absent the asset stays Pending so the client can retry. Uses the storage-reported size (best-effort) as
// the authoritative byte size and re-checks it against the per-file cap.
public sealed class ConfirmUploadCommandHandler
    : IRequestHandler<ConfirmUploadCommand, MediaAssetDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageProviderResolver _storage;
    private readonly IMediaPolicy _mediaPolicy;

    public ConfirmUploadCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IStorageProviderResolver storage,
        IMediaPolicy mediaPolicy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _storage = storage;
        _mediaPolicy = mediaPolicy;
    }

    public async Task<MediaAssetDto> Handle(
        ConfirmUploadCommand request,
        CancellationToken cancellationToken
    )
    {
        var asset = MediaGuard.EnsureOwned(
            await _unitOfWork.Media.GetByPublicIdAsync(request.PublicId, cancellationToken),
            _currentUser.UserId
        );

        if (asset.Status == MediaStatus.Confirmed)
            return Map(asset);

        var metadata = await _storage
            .Resolve(asset.Provider)
            .HeadAsync(asset.StorageKey, cancellationToken);
        if (metadata is null)
            throw new BadException("Media.ObjectNotFound");

        // Trust the stored object's real size for quota accuracy; re-check the per-file cap (BR-9-03).
        if (metadata.ByteSize > 0)
        {
            var maxBytes = await _mediaPolicy.GetMaxBytesForKindAsync(
                asset.Kind,
                cancellationToken
            );
            if (metadata.ByteSize > maxBytes)
            {
                await _storage
                    .Resolve(asset.Provider)
                    .DeleteAsync(asset.StorageKey, cancellationToken);
                _unitOfWork.Media.Remove(asset);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new BadException("Media.FileTooLarge");
            }
            asset.ByteSize = metadata.ByteSize;
        }

        asset.Status = MediaStatus.Confirmed;
        _unitOfWork.Media.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(asset);
    }

    private static MediaAssetDto Map(MediaAsset a) =>
        new(
            a.PublicId,
            a.Url,
            a.Kind.ToString(),
            a.ContentType,
            a.ByteSize,
            a.Width,
            a.Height,
            a.DurationSeconds,
            a.Status.ToString(),
            a.CreatedAt
        );
}
