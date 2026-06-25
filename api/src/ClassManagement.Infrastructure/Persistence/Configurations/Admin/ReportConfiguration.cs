using ClassManagement.Domain.Modules.Admin.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Admin;

// reports — mutable while under review. reporter_id RESTRICT, admin_id SET NULL. updated_at via trigger.
public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable(
            "reports",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_reports_status",
                    "status IN ('Pending', 'Reviewing', 'Resolved', 'Rejected')"
                );
                t.HasCheckConstraint(
                    "chk_reports_target_type",
                    "target_type IN ('Question', 'Exam', 'Assignment', 'Class', 'User')"
                );
                t.HasCheckConstraint(
                    "chk_reports_reason",
                    "reason IN ('InappropriateContent', 'Spam', 'Copyright', 'IncorrectAnswer', 'Other')"
                );
                t.HasCheckConstraint(
                    "chk_reports_admin_action",
                    "admin_action IS NULL OR admin_action IN ('Dismiss', 'WarnUser', 'HideContent', 'DeleteContent', 'BanUser')"
                );
                t.HasCheckConstraint(
                    "chk_reports_description_length",
                    "description IS NULL OR length(description) <= 2000"
                );
                t.HasCheckConstraint(
                    "chk_reports_admin_note_length",
                    "admin_note IS NULL OR length(admin_note) <= 1000"
                );
            }
        );

        builder.HasKey(r => r.Id);

        builder
            .Property(r => r.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(r => r.PublicId).HasDatabaseName("uq_reports_public_id").IsUnique();

        builder.Property(r => r.TargetType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Reason).HasConversion<string>().HasMaxLength(30);
        builder
            .Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Modules.Admin.Enums.ReportStatus.Pending);
        builder.Property(r => r.AdminAction).HasConversion<string>().HasMaxLength(20);

        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.AdminId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Idempotent: one active report per (reporter, target) while Pending or Reviewing.
        builder
            .HasIndex(r => new
            {
                r.ReporterId,
                r.TargetType,
                r.TargetId,
            })
            .HasDatabaseName("uq_reports_idempotent")
            .IsUnique()
            .HasFilter("status IN ('Pending', 'Reviewing')");

        builder
            .HasIndex(r => new { r.Status, r.CreatedAt })
            .HasDatabaseName("idx_reports_status")
            .IsDescending(false, true);
        builder
            .HasIndex(r => new { r.TargetType, r.TargetId })
            .HasDatabaseName("idx_reports_target");
        builder.HasIndex(r => r.ReporterId).HasDatabaseName("idx_reports_reporter");
        builder
            .HasIndex(r => r.AdminId)
            .HasDatabaseName("idx_reports_admin")
            .HasFilter("admin_id IS NOT NULL");
    }
}
