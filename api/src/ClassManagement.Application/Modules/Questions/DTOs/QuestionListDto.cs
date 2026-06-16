namespace ClassManagement.Application.Modules.Questions.DTOs;

/// <summary>Row for teacher/public/admin question lists. Read from <c>vw_questions</c>.</summary>
public sealed record QuestionListDto
{
    public Guid PublicId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public decimal SuggestedPoint { get; init; }
    public string Visibility { get; init; } = string.Empty;
    public Guid? SubjectPublicId { get; init; }
    public string? SubjectName { get; init; }
    public Guid TeacherPublicId { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public int OptionCount { get; init; }
    public List<string> Tags { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
