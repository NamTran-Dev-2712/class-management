using ClassManagement.Application.Exceptions;

// Marks all of the current user's Unread notifications as Read (MVP-7) via a single set-based update.
public record MarkAllNotificationsReadCommand : IRequest;

public class MarkAllNotificationsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkAllNotificationsReadCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var userId =
            _currentUser.UserId ?? throw new UnauthorizedException("Auth.NotAuthenticated");
        await _unitOfWork.Notifications.MarkAllReadAsync(userId, ct);
    }
}
