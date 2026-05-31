namespace ClassManagement.Infrastructure.Persistence.Configurations.Auth;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable(
            "users",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_users_display_name_length",
                    "length(display_name) >= 2 AND length(display_name) <= 100"
                );
                t.HasCheckConstraint(
                    "chk_users_avatar_url",
                    "avatar_url IS NULL OR avatar_url ~ '^https?://'"
                );
                t.HasCheckConstraint("chk_users_bio_length", "bio IS NULL OR length(bio) <= 500");
            }
        );

        builder
            .Property(u => u.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(u => u.PublicId).HasDatabaseName("uq_users_public_id").IsUnique();

        builder.Property(u => u.Email).HasColumnType("citext");

        builder
            .HasIndex(u => u.Email)
            .HasDatabaseName("uq_users_email_active")
            .HasFilter("deleted_at IS NULL")
            .IsUnique();

        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(u => u.LockedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(u => u.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(u => u.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(u => u.DeletedAt).HasDatabaseName("idx_users_deleted_at");
        builder
            .HasIndex(u => u.IsLocked)
            .HasDatabaseName("idx_users_is_locked")
            .HasFilter("is_locked = true");
    }
}
