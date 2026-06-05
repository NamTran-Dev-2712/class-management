using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;
using Microsoft.AspNetCore.Identity;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenHasher _tokenHasher;

    public AuthRepository(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenHasher tokenHasher
    )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenHasher = tokenHasher;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            throw new UnauthorizedException("Invalid credentials.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            throw new UnauthorizedException("Invalid credentials.");

        // Account-state gate — checked only after password is verified
        if (user.IsDeleted)
            throw new UnauthorizedException("Account has been deleted.");
        if (!user.IsActive)
            throw new UnauthorizedException("Account is disabled.");
        if (user.IsLocked)
            throw new UnauthorizedException("Account is locked.");

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResult = await _tokenService.GenerateTokensAsync(
            new UserTokenData
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                Roles = roles,
            }
        );

        await _refreshTokenRepository.UpdateRefreshTokenAsync(
            user.Id,
            tokenResult.RefreshTokenHash,
            tokenResult.RefreshTokenExpiry
        );

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return BuildAuthResult(user, tokenResult, roles);
    }

    public async Task<AuthResult> RefreshTokenAsync(
        string rawToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default
    )
    {
        var hash = _tokenHasher.Hash(rawToken);

        var token =
            await _refreshTokenRepository.FindActiveByHashAsync(hash, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        var user =
            await _userManager.FindByIdAsync(token.UserId.ToString())
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // Account-state checks — same generic message to avoid info leak
        if (user.IsDeleted || !user.IsActive || user.IsLocked)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResult = await _tokenService.GenerateTokensAsync(
            new UserTokenData
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                Roles = roles,
            }
        );

        await _refreshTokenRepository.RotateAsync(
            token.Id,
            user.Id,
            tokenResult.RefreshTokenHash,
            tokenResult.RefreshTokenExpiry,
            ipAddress,
            userAgent,
            cancellationToken
        );

        return BuildAuthResult(user, tokenResult, roles);
    }

    public async Task<long> RegisterAsync(
        string fullName,
        string email,
        string password,
        string role = ApplicationRoles.Student,
        CancellationToken cancellationToken = default
    )
    {
        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
            throw new ConflictException("Email is already registered.");

        var user = new ApplicationUser
        {
            DisplayName = fullName,
            Email = email,
            UserName = email,
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new BadException("Registration failed.");

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            throw new BadException("Failed to assign role.");
        }

        return user.Id;
    }

    public async Task<UserProfileDto> GetProfileAsync(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        var user =
            await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return ToProfileDto(user, roles);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        long userId,
        string displayName,
        string? bio,
        string? avatarUrl,
        string? phoneNumber,
        CancellationToken cancellationToken = default
    )
    {
        var user =
            await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        user.DisplayName = displayName;
        user.Bio = bio;
        user.AvatarUrl = avatarUrl;

        if (phoneNumber != user.PhoneNumber)
            await _userManager.SetPhoneNumberAsync(user, phoneNumber);
        else
            await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        return ToProfileDto(user, roles);
    }

    public async Task ChangePasswordAsync(
        long userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default
    )
    {
        var user =
            await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            throw new BadException("Current password is incorrect.");

        // Revoke all active refresh tokens — forces re-login on all devices
        await _refreshTokenRepository.RevokeAllForUserAsync(userId, cancellationToken);
    }

    // Shared builder — keeps all AuthResult construction in one place
    private static AuthResult BuildAuthResult(
        ApplicationUser user,
        TokenResult tokenResult,
        IList<string> roles
    ) =>
        new(
            PublicId: user.PublicId.ToString(),
            DisplayName: user.DisplayName,
            Email: user.Email ?? string.Empty,
            EmailConfirmed: user.EmailConfirmed,
            AvatarUrl: user.AvatarUrl,
            Bio: user.Bio,
            PhoneNumber: user.PhoneNumber,
            LastLoginAt: user.LastLoginAt,
            CreatedAt: user.CreatedAt,
            AccessToken: tokenResult.AccessToken,
            RefreshToken: tokenResult.RefreshToken,
            ExpiresAt: tokenResult.AccessTokenExpiry,
            RefreshTokenExpiresAt: tokenResult.RefreshTokenExpiry,
            Roles: [.. roles]
        );

    private static UserProfileDto ToProfileDto(ApplicationUser user, IList<string> roles) =>
        new(
            PublicId: user.PublicId.ToString(),
            DisplayName: user.DisplayName,
            PhoneNumber: user.PhoneNumber,
            Email: user.Email ?? string.Empty,
            EmailConfirmed: user.EmailConfirmed,
            AvatarUrl: user.AvatarUrl,
            Bio: user.Bio,
            LastLoginAt: user.LastLoginAt,
            CreatedAt: user.CreatedAt,
            Roles: [.. roles]
        );
}
