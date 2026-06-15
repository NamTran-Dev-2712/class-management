using System.Linq.Expressions;
using ClassManagement.Application.Modules.Classroom.DTOs;

public class GetAdminClassesQueryHandler
    : BaseGetQueryHandler<GetAdminClassesQuery, ClassView, ClassListDto>
{
    public GetAdminClassesQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<ClassView> ApplySearch(
        IQueryable<ClassView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(c =>
            c.Name.ToLower().Contains(keyword)
            || c.OwnerName.ToLower().Contains(keyword)
            || (c.SubjectName != null && c.SubjectName.ToLower().Contains(keyword))
        );
    }

    protected override IQueryable<ClassView> ApplyFilter(
        IQueryable<ClassView> query,
        GetAdminClassesQuery request
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
