using ClassManagement.Domain.Modules.Assignments.Enums;

namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// A published-or-draft test handed to one Class, built from one Exam (MVP-5). Carries the timing and
/// policy configuration. On publish it owns exactly one immutable <see cref="Snapshot"/> (the frozen
/// copy of the exam's questions/options); attempts read only from that snapshot, so later edits to the
/// source Exam never change a live Assignment. <see cref="ExamVersionAtPublish"/> records which Exam
/// version the snapshot was taken from (audit). Soft-deletable; deletion is blocked while any attempt
/// exists.
/// </summary>
public sealed class Assignment : AuditableEntity, IHasPublicId, ISoftDeletable
{
    public Guid PublicId { get; set; }
    public long ExamId { get; set; }
    public int? ExamVersionAtPublish { get; set; }
    public long ClassId { get; set; }
    public long TeacherId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime? OpensAt { get; set; }
    public DateTime? ClosesAt { get; set; }
    public int? TimeLimitMinutes { get; set; }

    public int MaxAttempts { get; set; } = 1;
    public ScorePolicy ScorePolicy { get; set; } = ScorePolicy.Highest;
    public bool AllowLate { get; set; }
    public GradePublishPolicy GradePublishPolicy { get; set; } = GradePublishPolicy.AfterDeadline;
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleOptions { get; set; }
    public bool ShowAnswersAfterGrade { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? GradesReleasedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Owned 1-1; null until the assignment is published.
    public AssignmentSnapshot? Snapshot { get; set; }
}
