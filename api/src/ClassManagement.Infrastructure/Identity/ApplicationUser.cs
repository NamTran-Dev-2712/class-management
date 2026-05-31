using Microsoft.AspNetCore.Identity;

namespace ClassManagement.Infrastructure.Identity;

// Maps to table "users" — extends Identity with all columns from docs/database/01-schema-auth.md
public sealed class ApplicationUser
    : IdentityUser<long>,
        IHasPublicId,
        ISoftDeletable,
        IFullyAuditable
{
    public Guid PublicId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public long? LockedBy { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}
