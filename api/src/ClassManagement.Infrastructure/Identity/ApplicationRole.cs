using Microsoft.AspNetCore.Identity;

namespace ClassManagement.Infrastructure.Identity;

// Maps to table "roles" — Student | Teacher | Admin (seeded, not CRUD via API)
public sealed class ApplicationRole : IdentityRole<long>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
