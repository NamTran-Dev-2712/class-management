using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Enums;

// Archives one of the current user's notifications (MVP-7) — hides it from the default list. Idempotent.
public record ArchiveNotificationCommand(Guid PublicId) : IRequest;

public class ArchiveNotificationCommandHandler : IRequestHandler<ArchiveNotificationCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ArchiveNotificationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ArchiveNotificationCommand request, CancellationToken ct)
    {
        var userId =
            _currentUser.UserId ?? throw new UnauthorizedException("Auth.NotAuthenticated");

        var notification =
            await _unitOfWork.Notifications.GetByPublicIdForUserAsync(request.PublicId, userId, ct)
            ?? throw new NotFoundException("Notification.NotFound");

        if (notification.Status != NotificationStatus.Archived)
        {
            // Reading-then-archiving keeps read_at meaningful for an unread item being archived.
            notification.ReadAt ??= DateTime.UtcNow;
            notification.Status = NotificationStatus.Archived;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
