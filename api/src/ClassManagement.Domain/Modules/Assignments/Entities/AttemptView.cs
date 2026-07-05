namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// Read-only projection of an attempt for teacher submission rosters and student attempt history.
/// Mapped to <c>vw_attempts</c> (attempt + student display name + assignment title + snapshot total
/// point). Enum columns are exposed as plain <c>string</c>. Never written through.
/// </summary>
public sealed class AttemptView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public bool AutoSubmitted { get; set; }

    // Proctoring integrity summary (MVP-10) — surfaced on the teacher roster.
    public int ViolationCount { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsLocked { get; set; }

    public decimal? TotalAutoScore { get; set; }
    public decimal? TotalManualScore { get; set; }
    public decimal? TotalScore { get; set; }

    public long AssignmentId { get; set; }
    public Guid AssignmentPublicId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public decimal? TotalPoint { get; set; }

    public long StudentId { get; set; }
    public Guid StudentPublicId { get; set; }
    public string StudentName { get; set; } = string.Empty;
}
