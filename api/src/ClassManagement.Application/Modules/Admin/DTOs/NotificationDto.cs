using ClassManagement.Domain.Modules.Admin.Enums;

namespace ClassManagement.Application.Modules.Admin.DTOs;

/// <summary>
/// One in-app notification for the current user (MVP-7). The client localizes the display text from
/// <see cref="EventType"/> + <see cref="Payload"/>, falling back to the stored <see cref="Title"/>/
/// <see cref="Body"/>.
/// </summary>
public sealed record NotificationDto(
    Guid PublicId,
    NotificationEventType EventType,
    string Title,
    string? Body,
    string? Link,
    Dictionary<string, object?>? Payload,
    NotificationStatus Status,
    DateTime CreatedAt,
    DateTime? ReadAt
);
