namespace ClassManagement.Infrastructure.Persistence.Configurations.Classroom;

/// <summary>
/// Maps the read-only <see cref="ClassView"/> projection to the <c>vw_classes</c> view. The view is
/// created via raw SQL in the <c>create_classroom_views_and_subject_name</c> migration; EF only needs
/// its shape. No table is generated (<c>ToView</c>); the view itself filters out soft-deleted rows.
/// </summary>
public sealed class ClassViewConfiguration : IEntityTypeConfiguration<ClassView>
{
    public void Configure(EntityTypeBuilder<ClassView> builder)
    {
        builder.ToView("vw_classes");
        builder.HasKey(c => c.Id);
    }
}
