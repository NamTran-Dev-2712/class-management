using System.Linq.Expressions;
using ClassManagement.Application.Modules.Media.DTOs;

// Shared list pipeline for media views (MVP-9). Concrete handlers override only GetBaseQuery for scope
// (teacher-own vs admin-all). Search matches content type; filters by kind/status. Projects in SQL.
public abstract class MediaListHandlerBase<TQuery>
    : BaseGetQueryHandler<TQuery, MediaView, MediaListDto>
    where TQuery : MediaFilterQuery
{
    protected MediaListHandlerBase(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<MediaView> ApplySearch(
        IQueryable<MediaView> query,
        string searchTerm
    )
    {
        var term = searchTerm.ToLower();
        return query.Where(m => m.ContentType.ToLower().Contains(term));
    }

    protected override IQueryable<MediaView> ApplyFilter(
        IQueryable<MediaView> query,
        TQuery request
    )
    {
        if (request.Kind is { } kind)
            query = query.Where(m => m.Kind == kind.ToString());
        if (request.Status is { } status)
            query = query.Where(m => m.Status == status.ToString());
        return query;
    }

    protected override Dictionary<string, Expression<Func<MediaView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdat"] = m => m.CreatedAt,
            ["bytesize"] = m => m.ByteSize,
        };

    protected override IQueryable<MediaListDto> ApplyProjection(IQueryable<MediaView> query) =>
        query.Select(m => new MediaListDto(
            m.PublicId,
            m.Url,
            m.Kind,
            m.ContentType,
            m.ByteSize,
            m.Width,
            m.Height,
            m.DurationSeconds,
            m.Status,
            m.CreatedAt,
            m.OwnerPublicId,
            m.OwnerName
        ));
}
