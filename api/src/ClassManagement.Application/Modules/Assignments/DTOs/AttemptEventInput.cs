namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// One proctoring signal reported by the client (MVP-10). <see cref="EventType"/> is one of
/// <c>AttemptEventTypes</c>. <see cref="OccurredAt"/> is the client clock (advisory; the server stamps its
/// own received-at). <see cref="Metadata"/> is optional context. Batched by the client (like auto-save).
/// </summary>
public sealed record AttemptEventInput
{
    public string EventType { get; init; } = string.Empty;
    public DateTime? OccurredAt { get; init; }
    public Dictionary<string, object?>? Metadata { get; init; }
}
