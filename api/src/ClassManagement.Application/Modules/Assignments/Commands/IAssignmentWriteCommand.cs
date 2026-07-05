using ClassManagement.Domain.Modules.Assignments.Enums;

// Shared config surface for the create/update assignment commands. The validator base
// (AssignmentWriteValidator) enforces the common rules (title length, time window, attempts).
public interface IAssignmentWriteCommand
{
    string Title { get; }
    string? Description { get; }
    DateTime? OpensAt { get; }
    DateTime? ClosesAt { get; }
    int? TimeLimitMinutes { get; }
    int MaxAttempts { get; }
    ScorePolicy ScorePolicy { get; }
    bool AllowLate { get; }
    GradePublishPolicy GradePublishPolicy { get; }
    bool ShuffleQuestions { get; }
    bool ShuffleOptions { get; }
    bool ShowAnswersAfterGrade { get; }

    // Proctoring config (MVP-10). Default OFF preserves MVP-5 behavior.
    bool RequireFullscreen { get; }
    bool DetectTabSwitch { get; }
    bool BlockCopyPaste { get; }
    int MaxViolations { get; }
    ViolationAction ViolationAction { get; }
}
