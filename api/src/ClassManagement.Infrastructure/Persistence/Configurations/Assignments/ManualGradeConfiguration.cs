using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

public sealed class ManualGradeConfiguration : IEntityTypeConfiguration<ManualGrade>
{
    public void Configure(EntityTypeBuilder<ManualGrade> builder)
    {
        builder.ToTable(
            "manual_grades",
            t =>
            {
                t.HasCheckConstraint("chk_manual_grades_score", "score >= 0");
                t.HasCheckConstraint(
                    "chk_manual_grades_feedback_length",
                    "feedback IS NULL OR length(feedback) <= 5000"
                );
            }
        );

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Score).HasColumnType("numeric(8,2)");
        builder.Property(g => g.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(g => g.UpdatedAt).HasDefaultValueSql("now()");

        // Attempt — CASCADE (deleting the attempt removes its grades).
        builder
            .HasOne<Attempt>()
            .WithMany()
            .HasForeignKey(g => g.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        // Snapshot question — RESTRICT (the immutable snapshot is referenced directly).
        builder
            .HasOne<SnapshotQuestion>()
            .WithMany()
            .HasForeignKey(g => g.SnapshotQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Grader / last editor (auditable columns).
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(g => g.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(g => g.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(g => new { g.AttemptId, g.SnapshotQuestionId })
            .HasDatabaseName("uq_manual_grades_attempt_question")
            .IsUnique();
        builder.HasIndex(g => g.AttemptId).HasDatabaseName("idx_manual_grades_attempt");
        builder.HasIndex(g => g.CreatedBy).HasDatabaseName("idx_manual_grades_graded_by");
    }
}
