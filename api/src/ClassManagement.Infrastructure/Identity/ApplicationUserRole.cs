using Microsoft.AspNetCore.Identity;

namespace ClassManagement.Infrastructure.Identity;

// Maps to table "user_roles" — adds assigned_at + assigned_by to Identity junction
public sealed class ApplicationUserRole : IdentityUserRole<long>
{
    public DateTime AssignedAt { get; set; }
    public long? AssignedBy { get; set; }
}
