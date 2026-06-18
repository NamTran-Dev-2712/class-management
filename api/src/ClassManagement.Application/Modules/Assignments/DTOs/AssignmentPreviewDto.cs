namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// Student-eye preview of a published assignment's snapshot (teacher "preview as student" / T5-06).
/// Questions are in canonical display order and options carry no correctness flag.
/// </summary>
public sealed record AssignmentPreviewDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? TotalPoint { get; init; }
    public int? TotalQuestions { get; init; }
    public IReadOnlyList<AssignmentPreviewQuestionDto> Questions { get; init; } = [];
}

public sealed record AssignmentPreviewQuestionDto
{
    public Guid PublicId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public decimal Point { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<AssignmentPreviewOptionDto> Options { get; init; } = [];
}

public sealed record AssignmentPreviewOptionDto
{
    public Guid PublicId { get; init; }
    public string Content { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
