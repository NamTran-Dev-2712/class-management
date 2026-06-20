// Teacher manually grades the writing answers of an attempt (MVP-6). Batch upsert: one item per writing
// question being scored (grade one or all in a save). Setting all writing questions transitions the
// attempt to Graded; editing an already-graded attempt recomputes its total. AttemptId is bound from the
// route (the controller overrides it via `command with { AttemptId = ... }`); Grades from the body.
public sealed record GradeAttemptCommand(Guid AttemptId, IReadOnlyList<GradeItemInput> Grades)
    : IRequest;

public sealed record GradeItemInput(Guid QuestionPublicId, decimal Score, string? Feedback);
