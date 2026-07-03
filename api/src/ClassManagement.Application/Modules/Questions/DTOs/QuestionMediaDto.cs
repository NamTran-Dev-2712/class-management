namespace ClassManagement.Application.Modules.Questions.DTOs;

/// <summary>A question-level media attachment as returned in question detail (MVP-9).</summary>
public sealed record QuestionMediaDto
{
    public Guid MediaPublicId { get; init; }
    public string Url { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
