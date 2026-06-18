using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Domain.Modules.Assignments.Enums;

// Owning teacher views the attempts (submission roster) of one of their assignments (T5-04).
public record GetAssignmentAttemptsQuery
    : BaseFilterQuery,
        IRequest<PaginatedResult<AttemptListDto>>
{
    public Guid AssignmentId { get; init; }
    public AttemptStatus? Status { get; init; }
}
