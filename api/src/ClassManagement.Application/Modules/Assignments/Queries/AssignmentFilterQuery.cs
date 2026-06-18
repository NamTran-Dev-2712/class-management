using ClassManagement.Domain.Modules.Assignments.Enums;

// Shared filter surface for teacher/admin assignment lists: status + class (by public id), plus the
// inherited paging/sort/search. Concrete queries differ only by their scope.
public abstract record AssignmentFilterQuery : BaseFilterQuery
{
    public AssignmentStatus? Status { get; init; }
    public Guid? ClassId { get; init; }
}
