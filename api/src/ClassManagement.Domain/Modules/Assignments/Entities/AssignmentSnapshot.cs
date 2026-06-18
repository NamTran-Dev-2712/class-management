namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// Immutable container (1-1 with <see cref="Assignment"/>) holding the frozen copy of an exam at the
/// moment of publish (MVP-5). Append-only: created once, never updated or soft-deleted, so it has no
/// <c>UpdatedAt</c>/<c>DeletedAt</c>. Owns <see cref="Questions"/> (and they own their options).
/// </summary>
public sealed class AssignmentSnapshot : BaseEntity
{
    public long AssignmentId { get; set; }
    public decimal TotalPoint { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime SnapshotCreatedAt { get; set; }

    public ICollection<SnapshotQuestion> Questions { get; set; } = [];
}
