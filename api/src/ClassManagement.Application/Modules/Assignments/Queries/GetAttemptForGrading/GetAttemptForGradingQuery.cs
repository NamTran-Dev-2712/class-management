using ClassManagement.Application.Modules.Assignments.DTOs;

// Owning teacher opens one attempt to grade its writing answers (T6-03). Returns the full per-question
// breakdown with the student's answers; the teacher always sees real scores regardless of grade release.
public sealed record GetAttemptForGradingQuery(Guid AttemptId) : IRequest<AttemptGradingDto>;
