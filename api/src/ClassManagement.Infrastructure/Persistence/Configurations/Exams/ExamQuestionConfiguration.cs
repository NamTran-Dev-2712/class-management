using ClassManagement.Domain.Modules.Exams.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Exams;

public sealed class ExamQuestionConfiguration : IEntityTypeConfiguration<ExamQuestion>
{
    public void Configure(EntityTypeBuilder<ExamQuestion> builder)
    {
        builder.ToTable(
            "exam_questions",
            t =>
            {
                t.HasCheckConstraint("chk_exam_questions_display_order", "display_order >= 1");
                t.HasCheckConstraint("chk_exam_questions_point", "point > 0 AND point <= 100");
            }
        );

        builder.HasKey(eq => eq.Id);

        builder.Property(eq => eq.Point).HasColumnType("numeric(8,2)");
        builder.Property(eq => eq.CreatedAt).HasDefaultValueSql("now()");

        // Question — RESTRICT: a question cannot be hard-deleted while referenced by an exam. (Soft
        // delete is an UPDATE, so the app additionally blocks soft-deleting an in-use own question.)
        builder
            .HasOne<Question>()
            .WithMany()
            .HasForeignKey(eq => eq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // No duplicate order within an exam (also the load-order index).
        builder
            .HasIndex(eq => new { eq.ExamId, eq.DisplayOrder })
            .HasDatabaseName("uq_exam_questions_order")
            .IsUnique();
        // No duplicate question within an exam.
        builder
            .HasIndex(eq => new { eq.ExamId, eq.QuestionId })
            .HasDatabaseName("uq_exam_questions_unique")
            .IsUnique();
        // "Which exams use this question?" — used before soft-deleting a question.
        builder.HasIndex(eq => eq.QuestionId).HasDatabaseName("idx_exam_questions_question");
    }
}
