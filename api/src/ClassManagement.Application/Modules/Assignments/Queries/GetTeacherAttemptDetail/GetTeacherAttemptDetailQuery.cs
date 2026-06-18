using ClassManagement.Application.Modules.Assignments.DTOs;

// Owning teacher views one attempt's result (scores always visible to the teacher).
public record GetTeacherAttemptDetailQuery(Guid AttemptId) : IRequest<AttemptResultDto>;
