namespace ClassManagement.Application.Modules.Classroom.DTOs;

/// <summary>Roster/pending row for a class. Read from <c>vw_class_members</c>.</summary>
public sealed record ClassMemberDto
{
    public Guid PublicId { get; init; }
    public Guid StudentPublicId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? RejectionReason { get; init; }
}
