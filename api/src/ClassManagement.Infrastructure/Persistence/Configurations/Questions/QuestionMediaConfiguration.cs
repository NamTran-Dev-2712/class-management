namespace ClassManagement.Infrastructure.Persistence.Configurations.Questions;

// question_media — first-class media attachments on a question (MVP-9). Append-only child of Question
// (cascade-deleted, replaced wholesale on update). The linked media asset is RESTRICT so it can't be
// hard-deleted while referenced. Unique (question_id, display_order) keeps the attachment order stable.
public sealed class QuestionMediaConfiguration : IEntityTypeConfiguration<QuestionMedia>
{
    public void Configure(EntityTypeBuilder<QuestionMedia> builder)
    {
        builder.ToTable(
            "question_media",
            t =>
            {
                t.HasCheckConstraint("chk_question_media_role", "role IN ('Inline', 'Attachment')");
                t.HasCheckConstraint("chk_question_media_display_order", "display_order >= 0");
            }
        );

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(m => m.MediaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(m => new { m.QuestionId, m.DisplayOrder })
            .HasDatabaseName("uq_question_media_order")
            .IsUnique();
        builder.HasIndex(m => m.MediaId).HasDatabaseName("idx_question_media_media");
    }
}
