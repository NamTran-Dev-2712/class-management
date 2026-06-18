using System.Linq.Expressions;
using ClassManagement.Application.Modules.Assignments.DTOs;

// Shared search/filter/sort/projection pipeline for teacher/admin assignment lists. Concrete handlers
// override GetBaseQuery to scope the rows (own / all). Search uses plain LINQ so Application stays
// EF-free.
public abstract class AssignmentListHandlerBase<TQuery>
    : BaseGetQueryHandler<TQuery, AssignmentView, AssignmentListDto>
    where TQuery : AssignmentFilterQuery, IRequest<PaginatedResult<AssignmentListDto>>
{
    protected AssignmentListHandlerBase(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<AssignmentView> ApplySearch(
        IQueryable<AssignmentView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(a => a.Title.ToLower().Contains(keyword));
    }

    protected override IQueryable<AssignmentView> ApplyFilter(
        IQueryable<AssignmentView> query,
        TQuery request
    )
    {
        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(a => a.Status == status);
        }

        if (request.ClassId.HasValue)
            query = query.Where(a => a.ClassPublicId == request.ClassId);

        return query;
    }

    protected override Dictionary<string, Expression<Func<AssignmentView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [AssignmentSortKeys.CreatedAt] = a => a.CreatedAt,
            [AssignmentSortKeys.UpdatedAt] = a => a.UpdatedAt,
            [AssignmentSortKeys.Title] = a => a.Title,
            [AssignmentSortKeys.OpensAt] = a => a.OpensAt!,
            [AssignmentSortKeys.ClosesAt] = a => a.ClosesAt!,
            [AssignmentSortKeys.Status] = a => a.Status,
        };

    protected override IQueryable<AssignmentListDto> ApplyProjection(
        IQueryable<AssignmentView> query
    ) => query.Select(AssignmentProjections.ToListDto);
}
