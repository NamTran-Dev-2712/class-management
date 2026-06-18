using ClassManagement.Application.Modules.Assignments.DTOs;

// Teacher lists their own assignments (any status), with optional status/class filters + search.
public record GetTeacherAssignmentsQuery
    : AssignmentFilterQuery,
        IRequest<PaginatedResult<AssignmentListDto>>;
