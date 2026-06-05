using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthRepository _authRepository;
    private readonly ICacheService _cache;

    public UpdateProfileCommandHandler(
        ICurrentUserService currentUser,
        IAuthRepository authRepository,
        ICacheService cache
    )
    {
        _currentUser = currentUser;
        _authRepository = authRepository;
        _cache = cache;
    }

    public async Task<UserProfileDto> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");

        var profile = await _authRepository.UpdateProfileAsync(
            userId,
            request.DisplayName,
            request.Bio,
            request.AvatarUrl,
            request.PhoneNumber,
            cancellationToken
        );

        await _cache.RemoveAsync(CacheKeys.UserProfile(userId), cancellationToken);

        return profile;
    }
}
