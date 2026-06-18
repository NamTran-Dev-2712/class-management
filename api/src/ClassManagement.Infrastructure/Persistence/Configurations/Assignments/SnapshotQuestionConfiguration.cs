using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// Frozen copy of an exam question (append-only). original_question_id keeps a SET-NULL audit link.
public sealed class SnapshotQuestionConfiguration : IEntityTypeConfiguration<SnapshotQuestion>
{
    public void Configure(EntityTypeBuilder<SnapshotQuestion> builder)
    {
        builder.ToTable(
            "snapshot_questions",
            t =>
            {
                t.HasCheckConstraint("chk_snapshot_questions_point", "point > 0");
                t.HasCheckConstraint("chk_snapshot_questions_display_order", "display_order >= 1");
                t.HasCheckConstraint(
                    "chk_snapshot_questions_type",
                    "type IN ('SingleChoice', 'MultipleChoice', 'TrueFalse', 'ShortWriting', 'LongWriting')"
                );
            }
        );

        builder.HasKey(q => q.Id);

        builder
            .Property(q => q.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder
            .HasIndex(q => q.PublicId)
            .HasDatabaseName("uq_snapshot_questions_public_id")
            .IsUnique();

        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.Content).IsRequired();
        builder.Property(q => q.Point).HasColumnType("numeric(8,2)");
        builder.Property(q => q.SnapshotCreatedAt).HasDefaultValueSql("now()");

        // Audit link to the source question; SET NULL so the snapshot survives a question deletion.
        builder
            .HasOne<Question>()
            .WithMany()
            .HasForeignKey(q => q.OriginalQuestionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(q => new { q.SnapshotId, q.DisplayOrder })
            .HasDatabaseName("uq_snapshot_questions_order")
            .IsUnique();
        builder
            .HasIndex(q => q.OriginalQuestionId)
            .HasDatabaseName("idx_snapshot_questions_original");

        builder
            .HasMany(q => q.Options)
            .WithOne()
            .HasForeignKey(o => o.SnapshotQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
