using ClassManagement.Domain.Modules.Assignments.Enums;

namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// One student run of an <see cref="Assignment"/> (MVP-5). Created on Start; reads questions from the
/// assignment snapshot in <see cref="QuestionOrder"/> (the per-attempt shuffle result, fixed at
/// Start). <see cref="DeadlineAt"/> is the server-computed hard stop (started_at + time limit, capped
/// at the assignment close). Submit (manual or auto via the lifecycle job) runs auto-grading and sets
/// the scores. Not soft-deletable (kept for history/evidence). A partial unique index enforces at most
/// one <c>InProgress</c> attempt per (assignment, student).
/// </summary>
public sealed class Attempt : AuditableEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public long AssignmentId { get; set; }
    public long StudentId { get; set; }
    public int AttemptNumber { get; set; }

    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public bool AutoSubmitted { get; set; }

    // Ordered snapshot-question ids as shown to this student (display position = list index + 1).
    public List<long> QuestionOrder { get; set; } = [];

    public decimal? TotalAutoScore { get; set; }
    public decimal? TotalManualScore { get; set; }
    public decimal? TotalScore { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
