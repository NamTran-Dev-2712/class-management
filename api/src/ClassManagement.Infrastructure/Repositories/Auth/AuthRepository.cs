using System.Security.Cryptography;
using System.Text;
using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailQueueService _emailQueue;
    private readonly ITokenHasher _tokenHasher;
    private readonly PasswordResetOptions _passwordResetOptions;
    private readonly ClientAppOptions _clientAppOptions;

    public AuthRepository(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailQueueService emailQueue,
        ITokenHasher tokenHasher,
        IOptions<PasswordResetOptions> passwordResetOptions,
        IOptions<ClientAppOptions> clientAppOptions
    )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailQueue = emailQueue;
        _tokenHasher = tokenHasher;
        _passwordResetOptions = passwordResetOptions.Value;
        _clientAppOptions = clientAppOptions.Value;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            throw new UnauthorizedException("Auth.InvalidCredentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            throw new UnauthorizedException("Auth.InvalidCredentials");

        // Account-state gate — checked only after password is verified
        if (user.IsDeleted)
            throw new UnauthorizedException("Auth.AccountDeleted");
        if (!user.IsActive)
            throw new UnauthorizedException("Auth.AccountDisabled");
        if (user.IsLocked)
            throw new UnauthorizedException("Auth.AccountLocked");

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
            ?? throw new UnauthorizedException("Auth.RefreshTokenInvalid");

        var user =
            await _userManager.FindByIdAsync(token.UserId.ToString())
            ?? throw new UnauthorizedException("Auth.RefreshTokenInvalid");

        // Account-state checks — same generic message to avoid info leak
        if (user.IsDeleted || !user.IsActive || user.IsLocked)
            throw new UnauthorizedException("Auth.RefreshTokenInvalid");

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
            throw new ConflictException("Auth.EmailAlreadyRegistered");

        var user = new ApplicationUser
        {
            DisplayName = fullName,
            Email = email,
            UserName = email,
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new BadException("Auth.RegistrationFailed");

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            throw new BadException("Auth.RoleAssignFailed");
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
            ?? throw new NotFoundException("User.NotFound");

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
            ?? throw new NotFoundException("User.NotFound");

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
            ?? throw new NotFoundException("User.NotFound");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            throw new BadException("Auth.CurrentPasswordIncorrect");

        // Revoke all active refresh tokens — forces re-login on all devices
        await _refreshTokenRepository.RevokeAllForUserAsync(userId, cancellationToken);
    }

    public async Task ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByEmailAsync(email);

        // Silently no-op for unknown/unusable accounts — prevents account enumeration.
        if (user is null || user.IsDeleted || !user.IsActive || user.IsLocked)
            return;

        // Only the newest OTP should be valid.
        await _passwordResetTokenRepository.InvalidatePreviousAsync(user.Id, cancellationToken);

        var otp = GenerateOtp(_passwordResetOptions.OtpLength);
        var expiresAt = DateTime.UtcNow.AddMinutes(_passwordResetOptions.ExpiryMinutes);

        await _passwordResetTokenRepository.CreateAsync(
            user.Id,
            HashOtp(user.Id, otp),
            expiresAt,
            cancellationToken
        );

        var resetLink = BuildResetLink(email, otp);
        _emailQueue.EnqueuePasswordResetEmail(email, user.DisplayName, otp, resetLink);
    }

    public async Task ResetPasswordAsync(
        string email,
        string otp,
        string newPassword,
        CancellationToken cancellationToken = default
    )
    {
        // Single generic message key for every failure mode — avoids leaking which step failed.
        const string genericError = "Auth.ResetCodeInvalid";

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || user.IsDeleted || !user.IsActive || user.IsLocked)
            throw new BadException(genericError);

        var token = await _passwordResetTokenRepository.FindActiveByUserIdAsync(
            user.Id,
            cancellationToken
        );
        if (token is null || token.ExpiresAt <= DateTime.UtcNow)
            throw new BadException(genericError);

        // Too many wrong guesses — burn the token and reject.
        if (token.AttemptCount >= _passwordResetOptions.MaxAttempts)
        {
            await _passwordResetTokenRepository.MarkUsedAsync(token.Id, cancellationToken);
            throw new BadException(genericError);
        }

        if (!FixedTimeEquals(token.TokenHash, HashOtp(user.Id, otp)))
        {
            await _passwordResetTokenRepository.IncrementAttemptAsync(token.Id, cancellationToken);
            throw new BadException(genericError);
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!result.Succeeded)
            throw new BadException(
                result.Errors.FirstOrDefault()?.Description ?? "Auth.ResetPasswordFailed"
            );

        await _passwordResetTokenRepository.MarkUsedAsync(token.Id, cancellationToken);

        // Force re-login everywhere after a password reset.
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);
    }

    // OTP is salted with the user id so the stored hash isn't a plain digest of a 6-digit number.
    private string HashOtp(long userId, string otp) => _tokenHasher.Hash($"{userId}:{otp}");

    private static string GenerateOtp(int length)
    {
        var upperBound = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, upperBound);
        return value.ToString().PadLeft(length, '0');
    }

    private string BuildResetLink(string email, string otp)
    {
        var baseUrl = _clientAppOptions.BaseUrl.TrimEnd('/');
        var path = _clientAppOptions.ResetPasswordPath;
        if (!path.StartsWith('/'))
            path = "/" + path;

        return $"{baseUrl}{path}?email={Uri.EscapeDataString(email)}&otp={otp}";
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b)
        );

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
