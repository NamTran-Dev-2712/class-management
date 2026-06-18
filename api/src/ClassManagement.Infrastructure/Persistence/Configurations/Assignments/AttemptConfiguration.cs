using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

public sealed class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.ToTable(
            "attempts",
            t =>
            {
                t.HasCheckConstraint("chk_attempts_number", "attempt_number >= 1");
                t.HasCheckConstraint(
                    "chk_attempts_submitted_after_started",
                    "submitted_at IS NULL OR submitted_at >= started_at"
                );
                t.HasCheckConstraint(
                    "chk_attempts_total_auto_score",
                    "total_auto_score IS NULL OR total_auto_score >= 0"
                );
                t.HasCheckConstraint(
                    "chk_attempts_total_manual_score",
                    "total_manual_score IS NULL OR total_manual_score >= 0"
                );
                t.HasCheckConstraint(
                    "chk_attempts_total_score",
                    "total_score IS NULL OR total_score >= 0"
                );
                t.HasCheckConstraint(
                    "chk_attempts_status",
                    "status IN ('InProgress', 'Submitted', 'AutoGraded', 'NeedManualGrading', 'Graded')"
                );
            }
        );

        builder.HasKey(a => a.Id);

        builder
            .Property(a => a.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(a => a.PublicId).HasDatabaseName("uq_attempts_public_id").IsUnique();

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.StartedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.AutoSubmitted).HasDefaultValue(false);

        builder
            .Property(a => a.QuestionOrder)
            .HasColumnType("jsonb")
            .HasConversion(LongListJsonConverters.NonNull)
            .Metadata.SetValueComparer(LongListJsonConverters.NonNullComparer);

        builder.Property(a => a.TotalAutoScore).HasColumnType("numeric(8,2)");
        builder.Property(a => a.TotalManualScore).HasColumnType("numeric(8,2)");
        builder.Property(a => a.TotalScore).HasColumnType("numeric(8,2)");

        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<Assignment>()
            .WithMany()
            .HasForeignKey(a => a.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.StudentId)
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

        builder
            .HasIndex(a => new
            {
                a.AssignmentId,
                a.StudentId,
                a.AttemptNumber,
            })
            .HasDatabaseName("uq_attempts_sequence")
            .IsUnique();
        // At most one InProgress attempt per (assignment, student) — the DB race backstop (BR-5-06).
        builder
            .HasIndex(a => new { a.AssignmentId, a.StudentId })
            .HasDatabaseName("uq_attempts_one_in_progress")
            .IsUnique()
            .HasFilter("status = 'InProgress'");
        builder
            .HasIndex(a => new { a.AssignmentId, a.Status })
            .HasDatabaseName("idx_attempts_assignment_status");
        builder
            .HasIndex(a => new { a.StudentId, a.AssignmentId })
            .HasDatabaseName("idx_attempts_student_assignment");
        // Auto-submit sweep.
        builder
            .HasIndex(a => a.DeadlineAt)
            .HasDatabaseName("idx_attempts_deadline")
            .HasFilter("status = 'InProgress'");

        builder
            .HasMany(a => a.Answers)
            .WithOne()
            .HasForeignKey(x => x.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
