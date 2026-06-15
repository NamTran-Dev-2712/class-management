namespace ClassManagement.Application.Modules.Classroom.DTOs;

/// <summary>Row for teacher/admin class lists. Read from <c>vw_classes</c>.</summary>
public sealed record ClassListDto
{
    public Guid PublicId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SubjectName { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CoverImageUrl { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public int ApprovedMemberCount { get; init; }
    public int PendingCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
