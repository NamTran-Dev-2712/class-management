using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Domain.Modules.Assignments.Enums;

// Student lists assignments from the classes they're an approved member of (published only), with
// their own attempt stats. Optional status/class filters + search.
public record GetStudentAssignmentsQuery
    : BaseFilterQuery,
        IRequest<PaginatedResult<StudentAssignmentListDto>>
{
    public AssignmentStatus? Status { get; init; }
    public Guid? ClassId { get; init; }
}
