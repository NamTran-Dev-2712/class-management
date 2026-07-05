using ClassManagement.Domain.Modules.Assignments.Enums;

// Teacher creates a Draft assignment from one of their exams, targeting one of their classes (BR-5-01).
// The exam/class link is fixed at create; timing + policy can be edited while Draft. Returns the new
// PublicId. No snapshot is built until publish.
public record CreateAssignmentCommand(
    Guid ExamId,
    Guid ClassId,
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
) : IRequest<Guid>, IAssignmentWriteCommand;
