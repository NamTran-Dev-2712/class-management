using ClassManagement.Application.Modules.Admin.DTOs;

// The current user's notifications (MVP-7), newest first. Optional status filter (Unread/Read/Archived).
public record GetMyNotificationsQuery : BaseFilterQuery, IRequest<PaginatedResult<NotificationDto>>
{
    public NotificationStatus? Status { get; init; }
}
