using ClassManagement.Application.Modules.Assignments.DTOs;

// Student views their own attempt history, optionally filtered to one assignment (S5-08).
public record GetMyAttemptsQuery : BaseFilterQuery, IRequest<PaginatedResult<AttemptListDto>>
{
    public Guid? AssignmentId { get; init; }
}
