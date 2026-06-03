public record AuthResult(
    string PublicId,
    string DisplayName,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt,
    IReadOnlyList<string> Roles,
    bool EmailConfirmed
);
