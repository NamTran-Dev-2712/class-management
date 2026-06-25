using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Admin.DTOs;

// Lists the current user's notifications. Scoped to the caller in GetBaseQuery so search/filter/count
// all honour the ownership boundary; archived items are included only when explicitly filtered.
public class GetMyNotificationsQueryHandler
    : BaseGetQueryHandler<GetMyNotificationsQuery, Notification, NotificationDto>
{
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    protected override IQueryable<Notification> GetBaseQuery(IGenericRepository<Notification> repo)
    {
        var userId =
            _currentUser.UserId ?? throw new UnauthorizedException("Auth.NotAuthenticated");
        return repo.Query().Where(n => n.UserId == userId);
    }

    protected override IQueryable<Notification> ApplyFilter(
        IQueryable<Notification> query,
        GetMyNotificationsQuery request
    )
    {
        // Default view hides archived; an explicit status filter can surface them.
        if (request.Status.HasValue)
            return query.Where(n => n.Status == request.Status.Value);

        return query.Where(n => n.Status != NotificationStatus.Archived);
    }

    protected override IQueryable<NotificationDto> ApplyProjection(
        IQueryable<Notification> query
    ) =>
        query.Select(n => new NotificationDto(
            n.PublicId,
            n.EventType,
            n.Title,
            n.Body,
            n.Link,
            n.Payload,
            n.Status,
            n.CreatedAt,
            n.ReadAt
        ));
}
