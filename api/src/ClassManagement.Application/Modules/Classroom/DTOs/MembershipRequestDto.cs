namespace ClassManagement.Application.Modules.Classroom.DTOs;

/// <summary>A student's join request and its current status. Read from <c>vw_class_members</c>.</summary>
public sealed record MembershipRequestDto
{
    public Guid PublicId { get; init; }
    public Guid ClassPublicId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public string? SubjectName { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? RejectionReason { get; init; }
}
