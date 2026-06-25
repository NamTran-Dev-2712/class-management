using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Admin.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Admin;

// audit_logs — append-only (no PublicId, no updated_at). Polymorphic actor/target: NO foreign keys.
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        var actions = string.Join(", ", AuditActions.All.Select(a => $"'{a}'"));
        var targets = string.Join(", ", AuditTargetTypes.All.Select(t => $"'{t}'"));
        var roles = string.Join(", ", AuditActorRoles.All.Select(r => $"'{r}'"));

        builder.ToTable(
            "audit_logs",
            t =>
            {
                t.HasCheckConstraint("chk_audit_logs_action", $"action IN ({actions})");
                t.HasCheckConstraint(
                    "chk_audit_logs_actor_role",
                    $"actor_role IS NULL OR actor_role IN ({roles})"
                );
                t.HasCheckConstraint(
                    "chk_audit_logs_target_type",
                    $"target_type IS NULL OR target_type IN ({targets})"
                );
            }
        );

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.ActorRole).HasMaxLength(20);
        builder.Property(a => a.TargetType).HasMaxLength(30);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");

        builder
            .Property(a => a.Metadata)
            .HasColumnType("jsonb")
            .HasConversion(JsonDictionaryConverters.Nullable)
            .Metadata.SetValueComparer(JsonDictionaryConverters.NullableComparer);

        builder
            .HasIndex(a => new { a.ActorId, a.CreatedAt })
            .HasDatabaseName("idx_audit_logs_actor_created")
            .IsDescending(false, true);
        builder
            .HasIndex(a => new { a.Action, a.CreatedAt })
            .HasDatabaseName("idx_audit_logs_action_created")
            .IsDescending(false, true);
        builder
            .HasIndex(a => new { a.TargetType, a.TargetId })
            .HasDatabaseName("idx_audit_logs_target");
        builder
            .HasIndex(a => a.CreatedAt)
            .HasDatabaseName("idx_audit_logs_created_at")
            .IsDescending(true);
    }
}
