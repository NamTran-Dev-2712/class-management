using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Interfaces.Identity;
using ClassManagement.Application.Modules.Media.DTOs;
using ClassManagement.Application.Modules.Payment.Interfaces;

public sealed class GetStorageUsageQueryHandler
    : IRequestHandler<GetStorageUsageQuery, StorageUsageDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceLimitService _resourceLimits;

    public GetStorageUsageQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IResourceLimitService resourceLimits
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _resourceLimits = resourceLimits;
    }

    public async Task<StorageUsageDto> Handle(
        GetStorageUsageQuery request,
        CancellationToken cancellationToken
    )
    {
        var ownerId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var used = await _unitOfWork.Media.SumBytesByOwnerAsync(ownerId, cancellationToken);
        var limits = await _resourceLimits.GetEffectiveLimitsAsync(ownerId, cancellationToken);

        return new StorageUsageDto(used, limits.MaxStorageBytes, limits.IsPro, limits.PlanName);
    }
}
