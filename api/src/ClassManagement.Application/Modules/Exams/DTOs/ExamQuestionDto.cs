namespace ClassManagement.Application.Modules.Exams.DTOs;

/// <summary>
/// One question placed in an exam, for the builder/detail screen: the per-exam order + point plus the
/// referenced question's display fields. <see cref="IsAvailable"/> is false when the referenced
/// question was soft-deleted (e.g. a Public question removed by its owner) — the teacher must replace
/// it before publishing (BR-4 edge case); display fields are then empty.
/// </summary>
public sealed record ExamQuestionDto
{
    public Guid QuestionPublicId { get; init; }
    public int DisplayOrder { get; init; }
    public decimal Point { get; init; }
    public bool IsAvailable { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public int OptionCount { get; init; }
    public Guid? SubjectPublicId { get; init; }
    public string? SubjectName { get; init; }
}
