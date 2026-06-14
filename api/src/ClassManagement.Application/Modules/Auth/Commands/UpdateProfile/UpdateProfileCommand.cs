public record UpdateProfileCommand(
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    string? PhoneNumber
) : IRequest<UserProfileDto>;
