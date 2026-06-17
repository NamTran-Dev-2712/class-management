using ClassManagement.Domain.Modules.Exams.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Exams;

/// <summary>
/// Maps the read-only <see cref="ExamView"/> projection to the <c>vw_exams</c> view. The view is
/// created via raw SQL in the <c>create_exams_and_views</c> migration; EF only needs its shape. No
/// table is generated (<c>ToView</c>); the view itself filters out soft-deleted rows and joins the
/// owner's display name + the live subject name.
/// </summary>
public sealed class ExamViewConfiguration : IEntityTypeConfiguration<ExamView>
{
    public void Configure(EntityTypeBuilder<ExamView> builder)
    {
        builder.ToView("vw_exams");
        builder.HasKey(e => e.Id);
    }
}
