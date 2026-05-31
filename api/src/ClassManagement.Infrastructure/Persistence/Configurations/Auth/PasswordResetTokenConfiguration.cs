namespace ClassManagement.Infrastructure.Persistence.Configurations.Auth;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable(
            "password_reset_tokens",
            t => t.HasCheckConstraint("chk_prt_expires", "expires_at > created_at")
        );

        builder.HasKey(prt => prt.Id);

        builder.Property(prt => prt.TokenHash).IsRequired();
        builder.HasIndex(prt => prt.TokenHash).HasDatabaseName("uq_password_reset_hash").IsUnique();

        builder.Property(prt => prt.CreatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(prt => prt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Note: NOW() cannot be used in a partial index predicate (not IMMUTABLE in PostgreSQL).
        // Filter only on used_at; expires_at is always checked at query time.
        builder
            .HasIndex(prt => prt.UserId)
            .HasDatabaseName("idx_prt_user_active")
            .HasFilter("used_at IS NULL");
    }
}
