using System.Linq.Expressions;
using ClassManagement.Application.Modules.Classroom.DTOs;

public class GetTeacherClassesQueryHandler
    : BaseGetQueryHandler<GetTeacherClassesQuery, ClassView, ClassListDto>
{
    private readonly ICurrentUserService _currentUser;

    public GetTeacherClassesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    // Scoped to the calling teacher's own classes (BR-2-04). Applied to the base query so
    // search/filter/COUNT all respect it.
    protected override IQueryable<ClassView> GetBaseQuery(IGenericRepository<ClassView> repo)
    {
        var ownerId = _currentUser.UserId ?? -1;
        return repo.Query().Where(c => c.OwnerId == ownerId);
    }

    protected override IQueryable<ClassView> ApplySearch(
        IQueryable<ClassView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(c =>
            c.Name.ToLower().Contains(keyword)
            || (c.SubjectName != null && c.SubjectName.ToLower().Contains(keyword))
        );
    }

    protected override IQueryable<ClassView> ApplyFilter(
        IQueryable<ClassView> query,
        GetTeacherClassesQuery request
    )
    {
        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(c => c.Status == status);
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<ClassView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [ClassSortKeys.Name] = c => c.Name,
            [ClassSortKeys.CreatedAt] = c => c.CreatedAt,
            [ClassSortKeys.UpdatedAt] = c => c.UpdatedAt,
        };

    protected override IQueryable<ClassListDto> ApplyProjection(IQueryable<ClassView> query) =>
        query.Select(ClassProjections.ToListDto);
}
