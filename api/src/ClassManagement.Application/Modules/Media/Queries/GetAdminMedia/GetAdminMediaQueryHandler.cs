// Admin sees every teacher's media (unscoped), optionally filtered to one owner.
public sealed class GetAdminMediaQueryHandler : MediaListHandlerBase<GetAdminMediaQuery>
{
    public GetAdminMediaQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<MediaView> ApplyFilter(
        IQueryable<MediaView> query,
        GetAdminMediaQuery request
    )
    {
        query = base.ApplyFilter(query, request);
        if (request.OwnerPublicId is { } owner)
            query = query.Where(m => m.OwnerPublicId == owner);
        return query;
    }
}
