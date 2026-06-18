namespace ClassManagement.Domain.Modules.Assignments.Enums;

/// <summary>
/// Lifecycle of a student <see cref="Entities.Attempt"/> (MVP-5). <c>InProgress</c> while taking;
/// <c>Submitted</c> immediately after submit/auto-submit; auto-grading then moves it to
/// <c>AutoGraded</c> (only objective questions) or <c>NeedManualGrading</c> (has writing questions);
/// <c>Graded</c> is reached after MVP-6 manual grading.
/// </summary>
public enum AttemptStatus
{
    InProgress,
    Submitted,
    AutoGraded,
    NeedManualGrading,
    Graded,
}
