using ClassManagement.Application.Modules.Assignments.DTOs;

// Admin views any assignment's full detail (read-only oversight).
public record GetAdminAssignmentDetailQuery(Guid PublicId) : IRequest<AssignmentDetailDto>;
