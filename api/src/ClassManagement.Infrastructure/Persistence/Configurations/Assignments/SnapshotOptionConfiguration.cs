using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// Frozen copy of an answer option (append-only). original_option_id keeps a SET-NULL audit link.
public sealed class SnapshotOptionConfiguration : IEntityTypeConfiguration<SnapshotOption>
{
    public void Configure(EntityTypeBuilder<SnapshotOption> builder)
    {
        builder.ToTable(
            "snapshot_options",
            t =>
            {
                t.HasCheckConstraint("chk_snapshot_options_display_order", "display_order >= 0");
            }
        );

        builder.HasKey(o => o.Id);

        builder
            .Property(o => o.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder
            .HasIndex(o => o.PublicId)
            .HasDatabaseName("uq_snapshot_options_public_id")
            .IsUnique();

        builder.Property(o => o.Content).IsRequired();
        builder.Property(o => o.SnapshotCreatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<QuestionOption>()
            .WithMany()
            .HasForeignKey(o => o.OriginalOptionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(o => new { o.SnapshotQuestionId, o.DisplayOrder })
            .HasDatabaseName("uq_snapshot_options_order")
            .IsUnique();
    }
}
