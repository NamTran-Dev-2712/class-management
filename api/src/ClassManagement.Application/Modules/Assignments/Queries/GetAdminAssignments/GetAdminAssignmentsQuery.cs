using ClassManagement.Application.Modules.Assignments.DTOs;

// Admin lists all assignments across teachers (read-only oversight).
public record GetAdminAssignmentsQuery
    : AssignmentFilterQuery,
        IRequest<PaginatedResult<AssignmentListDto>>;
