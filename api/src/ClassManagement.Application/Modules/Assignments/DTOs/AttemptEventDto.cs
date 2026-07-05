namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>One proctoring event on the teacher/admin attempt timeline (MVP-10). Read-only evidence.</summary>
public sealed record AttemptEventDto
{
    public string EventType { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public Dictionary<string, object?>? Metadata { get; init; }
}
