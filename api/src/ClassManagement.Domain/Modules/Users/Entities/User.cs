namespace ClassManagement.Domain.Modules.Users.Entities;

/// <summary>
/// Read-only projection of an application user for Admin user management. Mapped to the
/// <c>vw_admin_users</c> database view (users + aggregated role names, excluding soft-deleted
/// rows) so list queries can reuse <c>BaseGetQueryHandler</c> without leaking the Identity
/// <c>ApplicationUser</c> type into Application/Domain. Never written through — all mutations go
/// via <c>IUserAdminRepository</c> over <c>UserManager</c>.
/// </summary>
public sealed class User : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Aggregated role names from <c>user_roles</c> (Postgres <c>text[]</c>).</summary>
    public string[] Roles { get; set; } = [];
}
