namespace ClassManagement.Domain.Modules.Questions.Entities;

/// <summary>
/// An answer choice for a choice-type question (SingleChoice / MultipleChoice / TrueFalse).
/// Append-only child of <see cref="Question"/> (cascade-deleted with its parent); on edit the parent
/// replaces the whole set rather than mutating rows, so no UpdatedAt is needed.
/// </summary>
public sealed class QuestionOption : BaseEntity
{
    public long QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
}
