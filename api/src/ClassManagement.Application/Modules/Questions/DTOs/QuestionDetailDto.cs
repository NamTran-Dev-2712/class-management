namespace ClassManagement.Application.Modules.Questions.DTOs;

/// <summary>
/// Full question for detail/edit/preview screens — the list fields plus the answer options and
/// explanation. Built from the loaded aggregate (Options + Tags), not from the read-model view.
/// </summary>
public sealed record QuestionDetailDto
{
    public Guid PublicId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public decimal SuggestedPoint { get; init; }
    public string Visibility { get; init; } = string.Empty;
    public string? Explanation { get; init; }
    public Guid? SubjectPublicId { get; init; }
    public string? SubjectName { get; init; }
    public Guid TeacherPublicId { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = [];
    public IReadOnlyList<QuestionOptionDto> Options { get; init; } = [];

    // Question-level media attachments (MVP-9).
    public IReadOnlyList<QuestionMediaDto> Media { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
