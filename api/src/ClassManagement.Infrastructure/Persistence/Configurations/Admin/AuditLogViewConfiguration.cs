using ClassManagement.Domain.Modules.Admin.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Admin;

// Maps the read-only AuditLogView to vw_audit_logs (created via raw SQL in the migration).
public sealed class AuditLogViewConfiguration : IEntityTypeConfiguration<AuditLogView>
{
    public void Configure(EntityTypeBuilder<AuditLogView> builder)
    {
        builder.ToView("vw_audit_logs");
        builder.HasKey(a => a.Id);
    }
}
