using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// Maps the read-only AssignmentView to vw_assignments (created via raw SQL in the migration). EF only
// needs its shape; the view filters out soft-deleted rows and joins class/exam/owner + snapshot totals
// + attempt counts.
public sealed class AssignmentViewConfiguration : IEntityTypeConfiguration<AssignmentView>
{
    public void Configure(EntityTypeBuilder<AssignmentView> builder)
    {
        builder.ToView("vw_assignments");
        builder.HasKey(a => a.Id);
    }
}
