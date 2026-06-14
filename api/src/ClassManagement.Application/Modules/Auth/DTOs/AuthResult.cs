public record AuthResult(
    string PublicId,
    string DisplayName,
    string Email,
    bool EmailConfirmed,
    string? AvatarUrl,
    string? Bio,
    string? PhoneNumber,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt,
    IReadOnlyList<string> Roles
);
