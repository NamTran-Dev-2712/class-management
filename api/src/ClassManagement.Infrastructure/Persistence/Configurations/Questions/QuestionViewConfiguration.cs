namespace ClassManagement.Infrastructure.Persistence.Configurations.Questions;

/// <summary>
/// Maps the read-only <see cref="QuestionView"/> projection to the <c>vw_questions</c> view. The view
/// is created via raw SQL in the <c>create_questions_and_views</c> migration; EF only needs its shape.
/// No table is generated (<c>ToView</c>); the view itself filters out soft-deleted rows. The
/// aggregated <c>tags</c> column is a Postgres <c>text[]</c> mapped to <see cref="List{T}"/>.
/// </summary>
public sealed class QuestionViewConfiguration : IEntityTypeConfiguration<QuestionView>
{
    public void Configure(EntityTypeBuilder<QuestionView> builder)
    {
        builder.ToView("vw_questions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Tags).HasColumnType("text[]");
    }
}
