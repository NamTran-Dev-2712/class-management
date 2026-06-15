namespace ClassManagement.Application.Modules.Classroom.DTOs;

/// <summary>A class the student is an approved member of. Read from <c>vw_class_members</c>.</summary>
public sealed record StudentClassDto
{
    public Guid ClassPublicId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public string? SubjectName { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public string ClassStatus { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
}
