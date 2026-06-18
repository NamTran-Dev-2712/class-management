using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

public sealed class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.ToTable(
            "attempt_answers",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_attempt_answers_text_length",
                    "text_answer IS NULL OR length(text_answer) <= 50000"
                );
                t.HasCheckConstraint(
                    "chk_attempt_answers_auto_score",
                    "auto_score IS NULL OR auto_score >= 0"
                );
            }
        );

        builder.HasKey(a => a.Id);

        builder
            .Property(a => a.SelectedOptionIds)
            .HasColumnType("jsonb")
            .HasConversion(LongListJsonConverters.Nullable)
            .Metadata.SetValueComparer(LongListJsonConverters.NullableComparer);

        builder.Property(a => a.AutoScore).HasColumnType("numeric(8,2)");
        builder.Property(a => a.IsAutoGraded).HasDefaultValue(false);
        builder.Property(a => a.LastSavedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql("now()");

        // Snapshot question — RESTRICT (the snapshot is immutable; answers reference it directly).
        builder
            .HasOne<SnapshotQuestion>()
            .WithMany()
            .HasForeignKey(a => a.SnapshotQuestionId)
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
            .HasIndex(a => new { a.AttemptId, a.SnapshotQuestionId })
            .HasDatabaseName("uq_attempt_answers_unique")
            .IsUnique();
        builder.HasIndex(a => a.AttemptId).HasDatabaseName("idx_attempt_answers_attempt");
    }
}
