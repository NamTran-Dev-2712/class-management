namespace ClassManagement.Infrastructure.Persistence.Configurations.Questions;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable(
            "questions",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_questions_content_length",
                    "length(content) >= 10 AND length(content) <= 10000"
                );
                t.HasCheckConstraint(
                    "chk_questions_type",
                    "type IN ('SingleChoice', 'MultipleChoice', 'TrueFalse', 'ShortWriting', 'LongWriting')"
                );
                t.HasCheckConstraint(
                    "chk_questions_difficulty",
                    "difficulty IN ('Easy', 'Medium', 'Hard')"
                );
                t.HasCheckConstraint(
                    "chk_questions_suggested_point",
                    "suggested_point > 0 AND suggested_point <= 100"
                );
                t.HasCheckConstraint(
                    "chk_questions_visibility",
                    "visibility IN ('Private', 'Public')"
                );
                t.HasCheckConstraint(
                    "chk_questions_explanation_length",
                    "explanation IS NULL OR length(explanation) <= 2000"
                );
            }
        );

        builder.HasKey(q => q.Id);

        builder
            .Property(q => q.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(q => q.PublicId).HasDatabaseName("uq_questions_public_id").IsUnique();

        builder.Property(q => q.Content).IsRequired();
        builder.Property(q => q.Explanation).HasMaxLength(2000);

        // Enum columns are always written explicitly by the handlers (required in the commands), so
        // no DB-generated default is configured — a default whose value differs from the CLR default
        // (e.g. Medium vs Easy) would otherwise be silently substituted for genuine CLR-default picks.
        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.Difficulty).HasConversion<string>().HasMaxLength(10);
        builder.Property(q => q.Visibility).HasConversion<string>().HasMaxLength(10);

        builder.Property(q => q.SuggestedPoint).HasColumnType("numeric(8,2)");

        builder.Property(q => q.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(q => q.UpdatedAt).HasDefaultValueSql("now()");

        // Teacher (owner) — never delete a teacher who still owns questions.
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(q => q.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Subject — optional link; deleting the subject nulls the FK (question survives, BR-3 edge case).
        builder
            .HasOne<Subject>()
            .WithMany()
            .HasForeignKey(q => q.SubjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(q => q.CreatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(q => q.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(q => q.TeacherId)
            .HasDatabaseName("idx_questions_teacher")
            .HasFilter("deleted_at IS NULL");
        builder
            .HasIndex(q => q.SubjectId)
            .HasDatabaseName("idx_questions_subject")
            .HasFilter("subject_id IS NOT NULL");
        builder.HasIndex(q => q.Type).HasDatabaseName("idx_questions_type");
        builder.HasIndex(q => q.Difficulty).HasDatabaseName("idx_questions_difficulty");
        builder
            .HasIndex(q => new
            {
                q.TeacherId,
                q.Type,
                q.Difficulty,
            })
            .HasDatabaseName("idx_questions_teacher_type")
            .HasFilter("deleted_at IS NULL");
        builder
            .HasIndex(q => q.Visibility)
            .HasDatabaseName("idx_questions_visibility_public")
            .HasFilter("visibility = 'Public' AND deleted_at IS NULL");

        builder
            .HasMany(q => q.Options)
            .WithOne()
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasMany(q => q.Tags)
            .WithOne()
            .HasForeignKey(t => t.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
