using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Enums;

// Marks one of the current user's notifications as Read (MVP-7). Idempotent.
public record MarkNotificationReadCommand(Guid PublicId) : IRequest;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var userId =
            _currentUser.UserId ?? throw new UnauthorizedException("Auth.NotAuthenticated");

        var notification =
            await _unitOfWork.Notifications.GetByPublicIdForUserAsync(request.PublicId, userId, ct)
            ?? throw new NotFoundException("Notification.NotFound");

        if (notification.Status == NotificationStatus.Unread)
        {
            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
