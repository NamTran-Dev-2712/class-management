namespace ClassManagement.Infrastructure.Persistence.Configurations.Questions;

public sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable(
            "question_options",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_question_options_content_length",
                    "length(content) >= 1 AND length(content) <= 2000"
                );
                t.HasCheckConstraint("chk_question_options_display_order", "display_order >= 0");
            }
        );

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Content).IsRequired().HasMaxLength(2000);
        builder.Property(o => o.IsCorrect).HasDefaultValue(false);
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");

        // Unique display order within a question (also serves as the load-order index).
        builder
            .HasIndex(o => new { o.QuestionId, o.DisplayOrder })
            .HasDatabaseName("uq_question_options_order")
            .IsUnique();
    }
}
