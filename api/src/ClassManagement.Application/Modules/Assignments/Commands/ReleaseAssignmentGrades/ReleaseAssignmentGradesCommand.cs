// Teacher publishes the grades of an assignment (MVP-6, BR-6-04). Sets grades_released_at, which gates
// student score visibility under the Manual grade-publish policy. Idempotent (a no-op once released).
public sealed record ReleaseAssignmentGradesCommand(Guid PublicId) : IRequest;
