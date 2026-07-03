namespace ClassManagement.Infrastructure.Persistence.Configurations.Media;

// media_assets — uploaded image/audio/video (MVP-9). storage_key is internal (never exposed by the API);
// clients see public_id + url only. Soft-deletable; the physical object is removed later by the cleanup
// job (which skips assets still pinned by a snapshot). owner_id RESTRICT; created_by/updated_by SET NULL.
public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable(
            "media_assets",
            t =>
            {
                t.HasCheckConstraint("chk_media_assets_provider", "provider IN ('Local', 'R2')");
                t.HasCheckConstraint(
                    "chk_media_assets_kind",
                    "kind IN ('Image', 'Audio', 'Video')"
                );
                t.HasCheckConstraint(
                    "chk_media_assets_status",
                    "status IN ('Pending', 'Confirmed')"
                );
                t.HasCheckConstraint("chk_media_assets_byte_size", "byte_size > 0");
                t.HasCheckConstraint(
                    "chk_media_assets_dimensions",
                    "(width IS NULL OR width > 0) AND (height IS NULL OR height > 0) "
                        + "AND (duration_seconds IS NULL OR duration_seconds >= 0)"
                );
            }
        );

        builder.HasKey(m => m.Id);

        builder
            .Property(m => m.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(m => m.PublicId).HasDatabaseName("uq_media_assets_public_id").IsUnique();

        builder.Property(m => m.Provider).HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.StorageKey).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Url).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.Kind).HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.ContentType).IsRequired().HasMaxLength(100);
        builder
            .Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Modules.Media.Enums.MediaStatus.Pending);

        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(m => m.UpdatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Owner's confirmed library + quota sum.
        builder
            .HasIndex(m => m.OwnerId)
            .HasDatabaseName("idx_media_assets_owner")
            .HasFilter("deleted_at IS NULL");
        // Cleanup job: Pending-expired sweep.
        builder
            .HasIndex(m => new { m.Status, m.CreatedAt })
            .HasDatabaseName("idx_media_assets_status_created");
        // Cleanup job: soft-deleted objects awaiting physical removal.
        builder
            .HasIndex(m => m.DeletedAt)
            .HasDatabaseName("idx_media_assets_deleted")
            .HasFilter("deleted_at IS NOT NULL");
    }
}
