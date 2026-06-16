namespace ClassManagement.Infrastructure.Persistence.Configurations.Questions;

public sealed class QuestionTagConfiguration : IEntityTypeConfiguration<QuestionTag>
{
    public void Configure(EntityTypeBuilder<QuestionTag> builder)
    {
        builder.ToTable(
            "question_tags",
            t => t.HasCheckConstraint("chk_question_tags_format", "tag ~ '^[a-z0-9-]{1,50}$'")
        );

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Tag).IsRequired().HasMaxLength(50);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");

        // No duplicate tag within a question.
        builder
            .HasIndex(t => new { t.QuestionId, t.Tag })
            .HasDatabaseName("uq_question_tags_unique")
            .IsUnique();
        builder.HasIndex(t => t.Tag).HasDatabaseName("idx_question_tags_tag");
    }
}
