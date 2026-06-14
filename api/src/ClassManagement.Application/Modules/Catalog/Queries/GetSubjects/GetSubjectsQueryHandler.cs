using System.Linq.Expressions;

public class GetSubjectsQueryHandler : BaseGetQueryHandler<GetSubjectsQuery, Subject, SubjectDto>
{
    public GetSubjectsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<Subject> ApplySearch(IQueryable<Subject> query, string searchTerm)
    {
        var keyword = searchTerm.ToLower();
        return query.Where(s =>
            s.Name.ToLower().Contains(keyword)
            || (s.Description != null && s.Description.ToLower().Contains(keyword))
        );
    }

    protected override IQueryable<Subject> ApplyFilter(
        IQueryable<Subject> query,
        GetSubjectsQuery request
    )
    {
        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);

        return query;
    }

    protected override Dictionary<string, Expression<Func<Subject, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SubjectSortKeys.Name] = s => s.Name,
            [SubjectSortKeys.DisplayOrder] = s => s.DisplayOrder,
            [SubjectSortKeys.CreatedAt] = s => s.CreatedAt,
        };

    protected override IQueryable<SubjectDto> ApplyProjection(IQueryable<Subject> query) =>
        query.Select(s => new SubjectDto
        {
            PublicId = s.PublicId,
            Name = s.Name,
            Description = s.Description,
            IsActive = s.IsActive,
            DisplayOrder = s.DisplayOrder,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
        });
}

public static class SubjectSortKeys
{
    public const string Name = "name";
    public const string DisplayOrder = "displayOrder";
    public const string CreatedAt = "createdAt";
}
