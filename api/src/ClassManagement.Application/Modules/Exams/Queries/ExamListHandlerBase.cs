using System.Linq.Expressions;
using ClassManagement.Application.Modules.Exams.DTOs;

// Shared search/filter/sort/projection pipeline for the exam list reads. Concrete handlers only
// override GetBaseQuery to scope the rows (own bank / public pool / all). Keyword search uses plain
// LINQ (lower(title) LIKE) so the Application layer stays free of any EF dependency.
public abstract class ExamListHandlerBase<TQuery>
    : BaseGetQueryHandler<TQuery, ExamView, ExamListDto>
    where TQuery : ExamFilterQuery, IRequest<PaginatedResult<ExamListDto>>
{
    protected ExamListHandlerBase(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<ExamView> ApplySearch(
        IQueryable<ExamView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(e => e.Title.ToLower().Contains(keyword));
    }

    protected override IQueryable<ExamView> ApplyFilter(IQueryable<ExamView> query, TQuery request)
    {
        if (request.SubjectId.HasValue)
            query = query.Where(e => e.SubjectPublicId == request.SubjectId);

        if (request.Visibility.HasValue)
        {
            var visibility = request.Visibility.Value.ToString();
            query = query.Where(e => e.Visibility == visibility);
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<ExamView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [ExamSortKeys.CreatedAt] = e => e.CreatedAt,
            [ExamSortKeys.UpdatedAt] = e => e.UpdatedAt,
            [ExamSortKeys.Title] = e => e.Title,
            [ExamSortKeys.TotalPoint] = e => e.TotalPoint,
            [ExamSortKeys.TotalQuestions] = e => e.TotalQuestions,
        };

    protected override IQueryable<ExamListDto> ApplyProjection(IQueryable<ExamView> query) =>
        query.Select(ExamProjections.ToListDto);
}
