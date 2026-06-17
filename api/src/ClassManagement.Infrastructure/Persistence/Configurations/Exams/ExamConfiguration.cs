using ClassManagement.Domain.Modules.Exams.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Exams;

public sealed class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable(
            "exams",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_exams_title_length",
                    "length(title) >= 3 AND length(title) <= 300"
                );
                t.HasCheckConstraint(
                    "chk_exams_description_length",
                    "description IS NULL OR length(description) <= 1000"
                );
                t.HasCheckConstraint("chk_exams_visibility", "visibility IN ('Private', 'Public')");
                t.HasCheckConstraint("chk_exams_version", "version >= 1");
                t.HasCheckConstraint("chk_exams_total_point", "total_point >= 0");
                t.HasCheckConstraint("chk_exams_total_questions", "total_questions >= 0");
            }
        );

        builder.HasKey(e => e.Id);

        builder
            .Property(e => e.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(e => e.PublicId).HasDatabaseName("uq_exams_public_id").IsUnique();

        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        // Enum written explicitly by the handlers, so no DB-generated default (see QuestionConfiguration).
        builder.Property(e => e.Visibility).HasConversion<string>().HasMaxLength(10);

        builder.Property(e => e.Version).HasDefaultValue(1);
        builder.Property(e => e.TotalPoint).HasColumnType("numeric(8,2)").HasDefaultValue(0m);
        builder.Property(e => e.TotalQuestions).HasDefaultValue(0);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

        // Teacher (owner) — never delete a teacher who still owns exams.
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Subject — optional link; deleting the subject nulls the FK (exam survives).
        builder
            .HasOne<Subject>()
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(e => e.TeacherId)
            .HasDatabaseName("idx_exams_teacher")
            .HasFilter("deleted_at IS NULL");
        builder
            .HasIndex(e => e.SubjectId)
            .HasDatabaseName("idx_exams_subject")
            .HasFilter("subject_id IS NOT NULL AND deleted_at IS NULL");
        builder
            .HasIndex(e => e.Visibility)
            .HasDatabaseName("idx_exams_visibility_public")
            .HasFilter("visibility = 'Public' AND deleted_at IS NULL");
        builder
            .HasIndex(e => new { e.TeacherId, e.Visibility })
            .HasDatabaseName("idx_exams_teacher_visibility")
            .HasFilter("deleted_at IS NULL");

        builder
            .HasMany(e => e.Questions)
            .WithOne()
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
