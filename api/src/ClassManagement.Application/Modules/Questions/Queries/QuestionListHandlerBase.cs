using System.Linq.Expressions;
using ClassManagement.Application.Modules.Questions.DTOs;

// Shared search/filter/sort/projection pipeline for the question list reads. Concrete handlers only
// override GetBaseQuery to scope the rows (own bank / public pool / all). Keyword search uses plain
// LINQ (lower(content) LIKE) so the Application layer stays free of any EF dependency; the matching
// functional GIN trigram index on lower(content) keeps it fast.
public abstract class QuestionListHandlerBase<TQuery>
    : BaseGetQueryHandler<TQuery, QuestionView, QuestionListDto>
    where TQuery : QuestionFilterQuery, IRequest<PaginatedResult<QuestionListDto>>
{
    protected QuestionListHandlerBase(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<QuestionView> ApplySearch(
        IQueryable<QuestionView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(q => q.Content.ToLower().Contains(keyword));
    }

    protected override IQueryable<QuestionView> ApplyFilter(
        IQueryable<QuestionView> query,
        TQuery request
    )
    {
        if (request.SubjectId.HasValue)
            query = query.Where(q => q.SubjectPublicId == request.SubjectId);

        if (request.Type.HasValue)
        {
            var type = request.Type.Value.ToString();
            query = query.Where(q => q.Type == type);
        }

        if (request.Difficulty.HasValue)
        {
            var difficulty = request.Difficulty.Value.ToString();
            query = query.Where(q => q.Difficulty == difficulty);
        }

        if (request.Visibility.HasValue)
        {
            var visibility = request.Visibility.Value.ToString();
            query = query.Where(q => q.Visibility == visibility);
        }

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = request.Tag.Trim().ToLower();
            query = query.Where(q => q.Tags.Contains(tag));
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<QuestionView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [QuestionSortKeys.CreatedAt] = q => q.CreatedAt,
            [QuestionSortKeys.UpdatedAt] = q => q.UpdatedAt,
            [QuestionSortKeys.Difficulty] = q => q.Difficulty,
            [QuestionSortKeys.SuggestedPoint] = q => q.SuggestedPoint,
        };

    protected override IQueryable<QuestionListDto> ApplyProjection(
        IQueryable<QuestionView> query
    ) => query.Select(QuestionProjections.ToListDto);
}
