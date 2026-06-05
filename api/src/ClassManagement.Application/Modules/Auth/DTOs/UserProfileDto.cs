public record UserProfileDto(
    string PublicId,
    string DisplayName,
    string? PhoneNumber,
    string Email,
    bool EmailConfirmed,
    string? AvatarUrl,
    string? Bio,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    IReadOnlyList<string> Roles
);
