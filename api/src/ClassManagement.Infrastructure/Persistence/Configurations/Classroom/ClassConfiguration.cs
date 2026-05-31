using ClassManagement.Domain.Modules.Classroom.Enums;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Classroom;

public sealed class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable(
            "classes",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_classes_name_length",
                    "length(name) >= 2 AND length(name) <= 200"
                );
                t.HasCheckConstraint(
                    "chk_classes_description_length",
                    "description IS NULL OR length(description) <= 1000"
                );
                t.HasCheckConstraint("chk_classes_invite_code", "invite_code ~ '^[A-Z0-9]{6,8}$'");
                t.HasCheckConstraint("chk_classes_status", "status IN ('Active', 'Archived')");
                t.HasCheckConstraint(
                    "chk_classes_cover_image_url",
                    "cover_image_url IS NULL OR cover_image_url ~ '^https?://'"
                );
            }
        );

        builder.HasKey(c => c.Id);

        builder
            .Property(c => c.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(c => c.PublicId).HasDatabaseName("uq_classes_public_id").IsUnique();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.InviteCode).IsRequired().HasMaxLength(8);

        builder.HasIndex(c => c.InviteCode).HasDatabaseName("uq_classes_invite_code").IsUnique();

        builder
            .HasIndex(c => new { c.Name, c.OwnerId })
            .HasDatabaseName("uq_classes_name_owner_active")
            .HasFilter("deleted_at IS NULL")
            .IsUnique();

        builder.Property(c => c.Status).HasConversion<string>().HasDefaultValue(ClassStatus.Active);

        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Subject>()
            .WithMany()
            .HasForeignKey(c => c.SubjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.OwnerId).HasDatabaseName("idx_classes_owner");
        builder
            .HasIndex(c => c.SubjectId)
            .HasDatabaseName("idx_classes_subject")
            .HasFilter("subject_id IS NOT NULL");
        builder.HasIndex(c => new { c.Status, c.OwnerId }).HasDatabaseName("idx_classes_status");

        builder
            .HasMany(c => c.Memberships)
            .WithOne()
            .HasForeignKey(m => m.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
