namespace ClassManagement.Infrastructure.Persistence.Configurations.Classroom;

/// <summary>
/// Maps the read-only <see cref="ClassMemberView"/> projection to the <c>vw_class_members</c> view.
/// The view is created via raw SQL in the <c>create_classroom_views_and_subject_name</c> migration;
/// EF only needs its shape. No table is generated (<c>ToView</c>).
/// </summary>
public sealed class ClassMemberViewConfiguration : IEntityTypeConfiguration<ClassMemberView>
{
    public void Configure(EntityTypeBuilder<ClassMemberView> builder)
    {
        builder.ToView("vw_class_members");
        builder.HasKey(m => m.Id);
    }
}
