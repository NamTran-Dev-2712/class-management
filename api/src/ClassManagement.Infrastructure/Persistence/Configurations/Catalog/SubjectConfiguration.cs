namespace ClassManagement.Infrastructure.Persistence.Configurations.Catalog;

public sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable(
            "subjects",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_subjects_name_length",
                    "length(name) >= 2 AND length(name) <= 100"
                );
                t.HasCheckConstraint(
                    "chk_subjects_description_length",
                    "description IS NULL OR length(description) <= 500"
                );
            }
        );

        builder.HasKey(s => s.Id);

        builder
            .Property(s => s.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(s => s.PublicId).HasDatabaseName("uq_subjects_public_id").IsUnique();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

        builder
            .HasIndex(s => s.Name)
            .HasDatabaseName("uq_subjects_name_active")
            .HasFilter("deleted_at IS NULL")
            .IsUnique();

        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.DisplayOrder).HasDefaultValue(0);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("now()");

        builder
            .HasIndex(s => s.IsActive)
            .HasDatabaseName("idx_subjects_is_active")
            .HasFilter("is_active = true");
        builder.HasIndex(s => s.DisplayOrder).HasDatabaseName("idx_subjects_display_order");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
