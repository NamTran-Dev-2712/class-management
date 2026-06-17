namespace ClassManagement.Domain.Modules.Exams.Entities;

/// <summary>
/// One question placed in an <see cref="Exam"/> with its own display order and point (overrides the
/// question's suggested point). Append-only child (cascade-deleted with its parent); on edit the
/// parent replaces the whole set rather than mutating rows, so no UpdatedAt is needed.
/// </summary>
public sealed class ExamQuestion : BaseEntity
{
    public long ExamId { get; set; }
    public long QuestionId { get; set; }
    public int DisplayOrder { get; set; }
    public decimal Point { get; set; }
}
