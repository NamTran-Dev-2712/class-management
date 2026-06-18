namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// Frozen copy of one answer choice at publish time (MVP-5). Holds <see cref="IsCorrect"/> so
/// auto-grading reads correctness from the snapshot, never the live question. Append-only (no
/// UpdatedAt); <see cref="OriginalOptionId"/> is SET NULL on delete so the snapshot stays independent.
/// </summary>
public sealed class SnapshotOption : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public long SnapshotQuestionId { get; set; }
    public long? OriginalOptionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime SnapshotCreatedAt { get; set; }
}
