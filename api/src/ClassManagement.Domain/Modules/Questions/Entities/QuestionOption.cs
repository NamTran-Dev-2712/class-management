namespace ClassManagement.Domain.Modules.Questions.Entities;

/// <summary>
/// An answer choice for a choice-type question (SingleChoice / MultipleChoice / TrueFalse).
/// Append-only child of <see cref="Question"/> (cascade-deleted with its parent); on edit the parent
/// replaces the whole set rather than mutating rows, so no UpdatedAt is needed. <see cref="MediaId"/>
/// optionally attaches a single image to the option (MVP-9, T9-04); SET NULL on media delete.
/// </summary>
public sealed class QuestionOption : BaseEntity
{
    public long QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }

    // Optional image for this option (MVP-9). NULL when the option has no attached media.
    public long? MediaId { get; set; }
}
