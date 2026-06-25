using ClassManagement.Application.Exceptions;

// The current user's unread notification count (MVP-7) — drives the topbar bell badge.
public record GetUnreadNotificationCountQuery : IRequest<int>;

public class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadNotificationCountQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        var userId =
            _currentUser.UserId ?? throw new UnauthorizedException("Auth.NotAuthenticated");
        return _unitOfWork.Notifications.CountUnreadAsync(userId, ct);
    }
}
