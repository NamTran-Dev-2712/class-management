using ClassManagement.Domain.Modules.Questions.Enums;

namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// Frozen copy of one exam question at publish time (MVP-5). <see cref="OriginalQuestionId"/> points
/// back to the source question for audit but uses SET NULL on delete, so the snapshot survives even
/// if the source question is later removed. Append-only (no UpdatedAt). Owns <see cref="Options"/>.
/// </summary>
public sealed class SnapshotQuestion : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public long SnapshotId { get; set; }
    public long? OriginalQuestionId { get; set; }
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public decimal Point { get; set; }
    public int DisplayOrder { get; set; }
    public string? Explanation { get; set; }
    public DateTime SnapshotCreatedAt { get; set; }

    public ICollection<SnapshotOption> Options { get; set; } = [];

    // Frozen media references captured at publish (MVP-9); immutable like the rest of the snapshot.
    public ICollection<SnapshotMedia> Media { get; set; } = [];
}
