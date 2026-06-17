namespace ClassManagement.Application.Modules.Exams.DTOs;

/// <summary>
/// Exam rendered as a student would see it (T4-08): ordered questions with their options but
/// <b>without</b> the correct-answer flags or explanations. Unavailable (soft-deleted) questions are
/// flagged so the teacher knows to fix them before publishing.
/// </summary>
public sealed record ExamPreviewDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal TotalPoint { get; init; }
    public int TotalQuestions { get; init; }
    public IReadOnlyList<ExamPreviewQuestionDto> Questions { get; init; } = [];
}

public sealed record ExamPreviewQuestionDto
{
    public int DisplayOrder { get; init; }
    public decimal Point { get; init; }
    public bool IsAvailable { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public IReadOnlyList<ExamPreviewOptionDto> Options { get; init; } = [];
}

/// <summary>An answer choice as shown to a student — no <c>IsCorrect</c> leak.</summary>
public sealed record ExamPreviewOptionDto
{
    public string Content { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
