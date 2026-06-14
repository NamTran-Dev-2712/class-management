// Single-user detail (powers the Admin edit page). Resolved via IUserAdminRepository over
// UserManager so role membership is always fresh.
public sealed record UserDetailDto
{
    public Guid PublicId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public bool IsLocked { get; init; }
    public DateTime? LockedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}
