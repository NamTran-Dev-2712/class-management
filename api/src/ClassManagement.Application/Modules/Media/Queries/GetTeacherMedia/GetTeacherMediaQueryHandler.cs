using ClassManagement.Application.Interfaces.Identity;

// Scopes the media list to the current teacher's own assets (a non-owner sees an empty page).
public sealed class GetTeacherMediaQueryHandler : MediaListHandlerBase<GetTeacherMediaQuery>
{
    private readonly ICurrentUserService _currentUser;

    public GetTeacherMediaQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    protected override IQueryable<MediaView> GetBaseQuery(IGenericRepository<MediaView> repo)
    {
        var ownerId = _currentUser.UserId ?? -1;
        return repo.Query().Where(m => m.OwnerId == ownerId);
    }
}
