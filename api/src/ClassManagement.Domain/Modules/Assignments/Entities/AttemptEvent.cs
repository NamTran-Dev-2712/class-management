namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// One integrity signal recorded during an <see cref="Attempt"/> (MVP-10): tab switch, focus loss,
/// fullscreen exit, copy/paste, right-click, etc. Append-only evidence (<see cref="BaseEntity"/> — no
/// update/soft-delete, BR-10-02). <see cref="EventType"/> is one of
/// <see cref="Constants.AttemptEventTypes"/>; <see cref="OccurredAt"/> is the client-reported time (server
/// stamps <c>CreatedAt</c> as the authoritative received-at). <see cref="Metadata"/> holds optional jsonb
/// context (e.g. hidden duration, key combo). Owned by the attempt aggregate.
/// </summary>
public sealed class AttemptEvent : BaseEntity
{
    public long AttemptId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
}
