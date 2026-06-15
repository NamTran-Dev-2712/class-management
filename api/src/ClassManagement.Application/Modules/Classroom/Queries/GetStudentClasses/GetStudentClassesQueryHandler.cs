using System.Linq.Expressions;
using ClassManagement.Application.Modules.Classroom.DTOs;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class GetStudentClassesQueryHandler
    : BaseGetQueryHandler<GetStudentClassesQuery, ClassMemberView, StudentClassDto>
{
    private readonly ICurrentUserService _currentUser;

    public GetStudentClassesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    protected override IQueryable<ClassMemberView> GetBaseQuery(
        IGenericRepository<ClassMemberView> repo
    )
    {
        var studentId = _currentUser.UserId ?? -1;
        var approved = MembershipStatus.Approved.ToString();
        return repo.Query().Where(m => m.StudentId == studentId && m.Status == approved);
    }

    protected override IQueryable<ClassMemberView> ApplySearch(
        IQueryable<ClassMemberView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(m =>
            m.ClassName.ToLower().Contains(keyword)
            || m.OwnerName.ToLower().Contains(keyword)
            || (m.SubjectName != null && m.SubjectName.ToLower().Contains(keyword))
        );
    }

    protected override IQueryable<ClassMemberView> ApplyFilter(
        IQueryable<ClassMemberView> query,
        GetStudentClassesQuery request
    ) => query;

    protected override Dictionary<string, Expression<Func<ClassMemberView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["classname"] = m => m.ClassName,
            ["joinedat"] = m => m.JoinedAt,
            [ClassSortKeys.CreatedAt] = m => m.CreatedAt,
        };

    protected override IQueryable<StudentClassDto> ApplyProjection(
        IQueryable<ClassMemberView> query
    ) =>
        query.Select(m => new StudentClassDto
        {
            ClassPublicId = m.ClassPublicId,
            ClassName = m.ClassName,
            SubjectName = m.SubjectName,
            OwnerName = m.OwnerName,
            ClassStatus = m.ClassStatus,
            JoinedAt = m.JoinedAt,
        });
}
