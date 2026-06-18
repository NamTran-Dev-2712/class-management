using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// Maps the per-(assignment, approved-student) StudentAssignmentView to vw_student_assignments. The key
// is composite (assignment id + student id) because there is one row per student per assignment.
public sealed class StudentAssignmentViewConfiguration
    : IEntityTypeConfiguration<StudentAssignmentView>
{
    public void Configure(EntityTypeBuilder<StudentAssignmentView> builder)
    {
        builder.ToView("vw_student_assignments");
        builder.HasKey(a => new { a.Id, a.StudentId });
    }
}
