using ClassManagement.Domain.Modules.Classroom.Enums;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Classroom;

public sealed class ClassMembershipConfiguration : IEntityTypeConfiguration<ClassMembership>
{
    public void Configure(EntityTypeBuilder<ClassMembership> builder)
    {
        builder.ToTable(
            "class_memberships",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_memberships_status",
                    "status IN ('Pending', 'Approved', 'Rejected', 'Removed', 'Left')"
                );
                t.HasCheckConstraint(
                    "chk_memberships_rejection_reason",
                    "rejection_reason IS NULL OR length(rejection_reason) <= 500"
                );
            }
        );

        builder.HasKey(m => m.Id);

        builder
            .Property(m => m.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(m => m.PublicId).HasDatabaseName("uq_memberships_public_id").IsUnique();

        builder
            .Property(m => m.Status)
            .HasConversion<string>()
            .HasDefaultValue(MembershipStatus.Pending);

        builder.Property(m => m.JoinedAt).HasDefaultValueSql("now()");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");

        builder
            .HasIndex(m => new { m.ClassId, m.StudentId })
            .HasDatabaseName("uq_memberships_class_student_active")
            .HasFilter("status IN ('Pending', 'Approved')")
            .IsUnique();

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.ProcessedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(m => new { m.ClassId, m.Status })
            .HasDatabaseName("idx_memberships_class_status");
        builder
            .HasIndex(m => new { m.StudentId, m.Status })
            .HasDatabaseName("idx_memberships_student_approved")
            .HasFilter("status = 'Approved'");
        builder
            .HasIndex(m => m.ClassId)
            .HasDatabaseName("idx_memberships_class_approved")
            .HasFilter("status = 'Approved'");
    }
}
