namespace ClassManagement.Infrastructure.Persistence.Configurations.Auth;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable(
            "refresh_tokens",
            t => t.HasCheckConstraint("chk_refresh_tokens_expires", "expires_at > created_at")
        );

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash).IsRequired();
        builder.HasIndex(rt => rt.TokenHash).HasDatabaseName("uq_refresh_tokens_hash").IsUnique();

        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(rt => rt.ReplacedByTokenId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(rt => new { rt.UserId, rt.RevokedAt })
            .HasDatabaseName("idx_refresh_tokens_user_active")
            .HasFilter("revoked_at IS NULL");

        builder.HasIndex(rt => rt.ExpiresAt).HasDatabaseName("idx_refresh_tokens_expires");
    }
}
