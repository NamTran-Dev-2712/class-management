using ClassManagement.Application.Common.Constants;

public interface IAuthRepository
{
    Task<AuthResult> LoginAsync(string email, string password);

    Task<long> RegisterAsync(
        string fullName,
        string email,
        string password,
        string role = ApplicationRoles.Student,
        CancellationToken cancellationToken = default
    );

    Task<UserProfileDto> GetProfileAsync(
        long userId,
        CancellationToken cancellationToken = default
    );

    Task<AuthResult> RefreshTokenAsync(
        string rawToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default
    );

    Task<UserProfileDto> UpdateProfileAsync(
        long userId,
        string displayName,
        string? bio,
        string? avatarUrl,
        string? phoneNumber,
        CancellationToken cancellationToken = default
    );

    Task ChangePasswordAsync(
        long userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default
    );
}
