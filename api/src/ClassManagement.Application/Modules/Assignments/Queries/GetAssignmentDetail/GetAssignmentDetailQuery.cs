using ClassManagement.Application.Modules.Assignments.DTOs;

// Owning teacher views the full assignment (config + snapshot questions with correct flags).
public record GetAssignmentDetailQuery(Guid PublicId) : IRequest<AssignmentDetailDto>;
