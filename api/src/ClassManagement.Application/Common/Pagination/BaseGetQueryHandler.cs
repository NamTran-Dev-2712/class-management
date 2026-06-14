using System.Linq.Expressions;
using SortOrderValues = ClassManagement.Application.Common.Constants.SortOrder;

namespace ClassManagement.Application.Common.Pagination;

/// <summary>
/// Reusable handler for paginated list queries. Subclasses implement the entity-specific
/// search/filter/projection; this orchestrates the optimized pipeline:
/// search → filter → COUNT (before sort) → sort → project → page → materialize.
/// Soft-deleted rows are excluded automatically by the global query filter, and the
/// repository's <c>Query()</c> is AsNoTracking — no over-fetch, projection runs in SQL.
/// </summary>
public abstract class BaseGetQueryHandler<TQuery, TEntity, TDto>
    : IRequestHandler<TQuery, PaginatedResult<TDto>>
    where TQuery : BaseFilterQuery, IRequest<PaginatedResult<TDto>>
    where TEntity : BaseEntity
{
    protected readonly IUnitOfWork UnitOfWork;

    protected BaseGetQueryHandler(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<TDto>> Handle(
        TQuery request,
        CancellationToken cancellationToken
    )
    {
        var repo = UnitOfWork.Repository<TEntity>();
        var query = GetBaseQuery(repo);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = ApplySearch(query, request.SearchTerm.Trim());

        query = ApplyFilter(query, request);

        // Count before sorting so the COUNT query carries no ORDER BY.
        var totalCount = await repo.CountAsync(query, cancellationToken);

        query = ApplySorting(query, request);

        var dtoQuery = ApplyProjection(query)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var items = await repo.ToListAsync(dtoQuery, cancellationToken);

        return new PaginatedResult<TDto>(items, totalCount, request.PageNumber, request.PageSize);
    }

    protected virtual IQueryable<TEntity> GetBaseQuery(IGenericRepository<TEntity> repo) =>
        repo.Query();

    protected virtual IQueryable<TEntity> ApplySearch(
        IQueryable<TEntity> query,
        string searchTerm
    ) => query;

    protected abstract IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, TQuery request);

    protected virtual Dictionary<string, Expression<Func<TEntity, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase) { ["createdat"] = e => e.CreatedAt };

    protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TQuery request)
    {
        var map = SortKeyMap;
        var key = request.SortBy?.ToLowerInvariant() ?? string.Empty;
        var isDescending = string.Equals(
            request.SortOrder,
            SortOrderValues.Descending,
            StringComparison.OrdinalIgnoreCase
        );

        if (key.Length > 0 && map.TryGetValue(key, out var sortExpr))
            return isDescending ? query.OrderByDescending(sortExpr) : query.OrderBy(sortExpr);

        // Default: newest first.
        return query.OrderByDescending(e => e.CreatedAt);
    }

    protected abstract IQueryable<TDto> ApplyProjection(IQueryable<TEntity> query);
}
