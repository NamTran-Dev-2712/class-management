using ClassManagement.Application.Modules.Assignments.DTOs;

// Student views one of their own attempt's results (score visibility per grade-publish policy).
public record GetMyAttemptResultQuery(Guid AttemptId) : IRequest<AttemptResultDto>;
