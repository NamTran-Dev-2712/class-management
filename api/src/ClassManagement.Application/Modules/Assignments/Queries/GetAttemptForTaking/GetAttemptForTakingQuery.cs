using ClassManagement.Application.Modules.Assignments.DTOs;

// Student loads (or reloads) the live test-taking session for their attempt: questions in this
// attempt's order, current draft answers, and server-computed remaining time.
public record GetAttemptForTakingQuery(Guid AttemptId) : IRequest<AttemptTakingDto>;
