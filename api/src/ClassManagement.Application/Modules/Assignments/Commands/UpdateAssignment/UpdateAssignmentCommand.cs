using ClassManagement.Domain.Modules.Assignments.Enums;

// Teacher edits a Draft assignment's metadata + timing/policy. Only allowed while Draft (the exam/class
// link and the published snapshot are immutable). PublicId comes from the route.
public record UpdateAssignmentCommand(
    string Title,
    string? Description,
    DateTime? OpensAt,
    DateTime? ClosesAt,
    int? TimeLimitMinutes,
    int MaxAttempts,
    ScorePolicy ScorePolicy,
    bool AllowLate,
    GradePublishPolicy GradePublishPolicy,
    bool ShuffleQuestions,
    bool ShuffleOptions,
    bool ShowAnswersAfterGrade,
    bool RequireFullscreen,
    bool DetectTabSwitch,
    bool BlockCopyPaste,
    int MaxViolations,
    ViolationAction ViolationAction
) : IRequest, IAssignmentWriteCommand
{
    public Guid PublicId { get; init; }
}
