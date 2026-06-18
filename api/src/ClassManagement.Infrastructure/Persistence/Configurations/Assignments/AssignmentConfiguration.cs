using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable(
            "assignments",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_assignments_title_length",
                    "length(title) >= 3 AND length(title) <= 300"
                );
                t.HasCheckConstraint(
                    "chk_assignments_description_length",
                    "description IS NULL OR length(description) <= 1000"
                );
                t.HasCheckConstraint(
                    "chk_assignments_window",
                    "opens_at IS NULL OR closes_at IS NULL OR closes_at > opens_at"
                );
                t.HasCheckConstraint(
                    "chk_assignments_time_limit",
                    "time_limit_minutes IS NULL OR (time_limit_minutes > 0 AND time_limit_minutes <= 1440)"
                );
                t.HasCheckConstraint(
                    "chk_assignments_max_attempts",
                    "max_attempts >= 1 AND max_attempts <= 100"
                );
                t.HasCheckConstraint(
                    "chk_assignments_score_policy",
                    "score_policy IN ('Highest', 'Latest')"
                );
                t.HasCheckConstraint(
                    "chk_assignments_grade_publish_policy",
                    "grade_publish_policy IN ('Immediate', 'AfterDeadline', 'Manual')"
                );
                t.HasCheckConstraint(
                    "chk_assignments_status",
                    "status IN ('Draft', 'Scheduled', 'Open', 'Closed', 'Archived')"
                );
            }
        );

        builder.HasKey(a => a.Id);

        builder
            .Property(a => a.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(a => a.PublicId).HasDatabaseName("uq_assignments_public_id").IsUnique();

        builder.Property(a => a.Title).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(1000);

        builder.Property(a => a.MaxAttempts).HasDefaultValue(1);
        builder.Property(a => a.ScorePolicy).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.GradePublishPolicy).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(10);

        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql("now()");

        // Exam — never delete an exam while an assignment references it.
        builder
            .HasOne<Exam>()
            .WithMany()
            .HasForeignKey(a => a.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Class — never delete a class while an assignment references it.
        builder
            .HasOne<Class>()
            .WithMany()
            .HasForeignKey(a => a.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teacher (owner).
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // 1-1 owned snapshot (built on publish). RESTRICT: the snapshot is immutable and must outlive
        // any direct deletes; assignments are soft-deleted so this never actually fires.
        builder
            .HasOne(a => a.Snapshot)
            .WithOne()
            .HasForeignKey<AssignmentSnapshot>(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(a => new
            {
                a.ClassId,
                a.Status,
                a.OpensAt,
            })
            .HasDatabaseName("idx_assignments_class_status_opens")
            .HasFilter("deleted_at IS NULL");
        builder
            .HasIndex(a => new { a.TeacherId, a.Status })
            .HasDatabaseName("idx_assignments_teacher")
            .HasFilter("deleted_at IS NULL");
        builder
            .HasIndex(a => a.ExamId)
            .HasDatabaseName("idx_assignments_exam")
            .HasFilter("deleted_at IS NULL");
        builder
            .HasIndex(a => a.OpensAt)
            .HasDatabaseName("idx_assignments_scheduled")
            .HasFilter("status = 'Scheduled' AND deleted_at IS NULL");
        builder
            .HasIndex(a => a.ClosesAt)
            .HasDatabaseName("idx_assignments_closes_at")
            .HasFilter("status = 'Open' AND deleted_at IS NULL");
    }
}
