namespace ClassManagement.Domain.Modules.Questions.Entities;

/// <summary>
/// A free-form tag on a question (normalized lowercase, <c>[a-z0-9-]</c>). Append-only child of
/// <see cref="Question"/> (cascade-deleted, replaced wholesale on edit).
/// </summary>
public sealed class QuestionTag : BaseEntity
{
    public long QuestionId { get; set; }
    public string Tag { get; set; } = string.Empty;
}
