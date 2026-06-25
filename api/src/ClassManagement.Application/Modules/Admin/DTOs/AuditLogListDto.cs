namespace ClassManagement.Application.Modules.Admin.DTOs;

/// <summary>
/// One audit log row for the Admin audit view (MVP-7). No internal ids are exposed; the list is
/// read-only and immutable, so the client keys rows by position. <see cref="Metadata"/> is raw JSON.
/// </summary>
public sealed record AuditLogListDto(
    string Action,
    Guid? ActorPublicId,
    string? ActorName,
    string? ActorEmail,
    string? ActorRole,
    string? TargetType,
    Guid? TargetPublicId,
    string? Metadata,
    string? IpAddress,
    DateTime CreatedAt
);
