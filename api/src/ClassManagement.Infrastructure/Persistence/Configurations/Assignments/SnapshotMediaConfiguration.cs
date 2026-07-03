namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// snapshot_media — frozen media references captured at publish (MVP-9). Append-only, immutable (RESTRICT
// on the parent question) like the rest of the snapshot. Carries media_public_id (the cleanup guard) +
// the frozen_url as it was at publish, so old attempts keep working even if the source media is deleted.
public sealed class SnapshotMediaConfiguration : IEntityTypeConfiguration<SnapshotMedia>
{
    public void Configure(EntityTypeBuilder<SnapshotMedia> builder)
    {
        builder.ToTable(
            "snapshot_media",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_snapshot_media_kind",
                    "kind IN ('Image', 'Audio', 'Video')"
                );
                t.HasCheckConstraint("chk_snapshot_media_role", "role IN ('Inline', 'Attachment')");
                t.HasCheckConstraint("chk_snapshot_media_display_order", "display_order >= 0");
            }
        );

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Kind).HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.FrozenUrl).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.SnapshotCreatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<SnapshotQuestion>()
            .WithMany(q => q.Media)
            .HasForeignKey(m => m.SnapshotQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<SnapshotOption>()
            .WithMany()
            .HasForeignKey(m => m.SnapshotOptionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.SnapshotQuestionId).HasDatabaseName("idx_snapshot_media_question");
        // Cleanup guard lookup: is a soft-deleted media asset still pinned by any snapshot?
        builder.HasIndex(m => m.MediaPublicId).HasDatabaseName("idx_snapshot_media_public_id");
    }
}
