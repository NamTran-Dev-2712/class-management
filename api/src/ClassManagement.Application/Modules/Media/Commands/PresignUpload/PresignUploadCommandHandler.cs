using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Interfaces.Identity;
using ClassManagement.Application.Modules.Media.DTOs;
using ClassManagement.Application.Modules.Payment.Interfaces;

// Validates kind/MIME/size + total quota (BR-9-03/04/05) at presign time, stages a Pending MediaAsset,
// and returns the presigned PUT URL. The asset counts toward quota immediately (reserved) until cleanup
// removes it if never confirmed.
public sealed class PresignUploadCommandHandler
    : IRequestHandler<PresignUploadCommand, PresignResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediaPolicy _mediaPolicy;
    private readonly IResourceLimitService _resourceLimits;
    private readonly IStorageProviderResolver _storage;

    public PresignUploadCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMediaPolicy mediaPolicy,
        IResourceLimitService resourceLimits,
        IStorageProviderResolver storage
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mediaPolicy = mediaPolicy;
        _resourceLimits = resourceLimits;
        _storage = storage;
    }

    public async Task<PresignResultDto> Handle(
        PresignUploadCommand request,
        CancellationToken cancellationToken
    )
    {
        var ownerId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        // 1) MIME allowlist → kind (BR-9-05).
        var kind =
            _mediaPolicy.ResolveKind(request.ContentType)
            ?? throw new BadException("Media.UnsupportedType");

        // 2) Per-file size cap for the kind.
        var maxBytes = await _mediaPolicy.GetMaxBytesForKindAsync(kind, cancellationToken);
        if (request.ByteSize <= 0 || request.ByteSize > maxBytes)
            throw new BadException("Media.FileTooLarge");

        // 3) Total storage quota by plan (BR-9-04). 0 = unlimited.
        var quota = await _resourceLimits.GetMaxStorageBytesAsync(ownerId, cancellationToken);
        if (quota > 0)
        {
            var used = await _unitOfWork.Media.SumBytesByOwnerAsync(ownerId, cancellationToken);
            if (used + request.ByteSize > quota)
                throw new ForbiddenException("Media.QuotaExceeded");
        }

        // 4) Stage a Pending asset with a computed key + public URL.
        var provider = _storage.ActiveProvider;
        var mediaPublicId = Guid.NewGuid();
        var extension = MediaAssembler.ResolveExtension(request.FileName, request.ContentType);
        var storageKey = MediaAssembler.BuildStorageKey(mediaPublicId, extension);
        var url = _storage.Resolve(provider).BuildPublicUrl(storageKey);

        var asset = new MediaAsset
        {
            PublicId = mediaPublicId,
            OwnerId = ownerId,
            Provider = provider,
            StorageKey = storageKey,
            Url = url,
            Kind = kind,
            ContentType = request.ContentType,
            ByteSize = request.ByteSize,
            Width = request.Width,
            Height = request.Height,
            DurationSeconds = request.DurationSeconds,
            Status = MediaStatus.Pending,
        };
        await _unitOfWork.Media.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5) Mint the presigned upload URL.
        var presign = await _storage
            .Resolve(provider)
            .PresignPutAsync(
                new PresignUploadRequest(storageKey, request.ContentType, request.ByteSize),
                cancellationToken
            );

        return new PresignResultDto(
            mediaPublicId,
            presign.UploadUrl,
            presign.HttpMethod,
            presign.Headers,
            presign.ExpiresAt
        );
    }
}
