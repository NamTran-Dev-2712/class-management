using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;
using Microsoft.AspNetCore.Identity;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthRepository(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository
    )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
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

        return new AuthResult(
            PublicId: user.PublicId.ToString(),
            DisplayName: user.DisplayName,
            Email: user.Email ?? string.Empty,
            AccessToken: tokenResult.AccessToken,
            RefreshToken: tokenResult.RefreshToken,
            ExpiresAt: tokenResult.AccessTokenExpiry,
            RefreshTokenExpiresAt: tokenResult.RefreshTokenExpiry,
            Roles: [.. roles],
            EmailConfirmed: user.EmailConfirmed
        );
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
}
