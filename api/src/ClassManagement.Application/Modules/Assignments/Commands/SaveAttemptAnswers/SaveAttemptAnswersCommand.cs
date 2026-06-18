using ClassManagement.Application.Modules.Assignments.DTOs;

// Student auto-saves a batch of draft answers for an InProgress attempt (BR-5-11 — saving is not
// submitting). Upserts one row per question. AttemptId comes from the route.
public record SaveAttemptAnswersCommand(List<AttemptAnswerInput> Answers) : IRequest
{
    public Guid AttemptId { get; init; }
}
