using ClassManagement.Domain.Modules.Exams.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Exams;

public sealed class ExamTagConfiguration : IEntityTypeConfiguration<ExamTag>
{
    public void Configure(EntityTypeBuilder<ExamTag> builder)
    {
        builder.ToTable(
            "exam_tags",
            t => t.HasCheckConstraint("chk_exam_tags_format", "tag ~ '^[a-z0-9-]{1,50}$'")
        );

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Tag).IsRequired().HasMaxLength(50);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");

        // No duplicate tag within an exam.
        builder
            .HasIndex(t => new { t.ExamId, t.Tag })
            .HasDatabaseName("uq_exam_tags_unique")
            .IsUnique();
        builder.HasIndex(t => t.Tag).HasDatabaseName("idx_exam_tags_tag");
    }
}
