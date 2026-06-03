using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthRepository _authRepository;
    private readonly ICacheService _cache;

    public GetProfileQueryHandler(
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
        GetProfileQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");

        var cacheKey = CacheKeys.UserProfile(userId);

        var cached = await _cache.GetAsync<UserProfileDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var profile = await _authRepository.GetProfileAsync(userId, cancellationToken);

        await _cache.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(15), cancellationToken);

        return profile;
    }
}
