namespace ClassManagement.Domain.Modules.Exams.Entities;

/// <summary>
/// A free-form tag on an exam (normalized lowercase, <c>[a-z0-9-]</c>) for search/filtering. Append-only
/// child of <see cref="Exam"/> (cascade-deleted, replaced wholesale on edit). Metadata only — exam tags
/// do not flow into assignment snapshots.
/// </summary>
public sealed class ExamTag : BaseEntity
{
    public long ExamId { get; set; }
    public string Tag { get; set; } = string.Empty;
}
