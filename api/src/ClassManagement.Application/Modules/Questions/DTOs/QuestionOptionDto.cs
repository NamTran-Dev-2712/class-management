namespace ClassManagement.Application.Modules.Questions.DTOs;

/// <summary>An answer choice as returned in question detail. Ordered by <see cref="DisplayOrder"/>.</summary>
public sealed record QuestionOptionDto
{
    public string Content { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public int DisplayOrder { get; init; }
}
