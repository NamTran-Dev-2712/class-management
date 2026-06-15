using System.Linq.Expressions;
using ClassManagement.Application.Modules.Classroom.DTOs;

public class GetStudentMembershipRequestsQueryHandler
    : BaseGetQueryHandler<GetStudentMembershipRequestsQuery, ClassMemberView, MembershipRequestDto>
{
    private readonly ICurrentUserService _currentUser;

    public GetStudentMembershipRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    protected override IQueryable<ClassMemberView> GetBaseQuery(
        IGenericRepository<ClassMemberView> repo
    )
    {
        var studentId = _currentUser.UserId ?? -1;
        return repo.Query().Where(m => m.StudentId == studentId);
    }

    protected override IQueryable<ClassMemberView> ApplySearch(
        IQueryable<ClassMemberView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(m =>
            m.ClassName.ToLower().Contains(keyword) || m.OwnerName.ToLower().Contains(keyword)
        );
    }

    protected override IQueryable<ClassMemberView> ApplyFilter(
        IQueryable<ClassMemberView> query,
        GetStudentMembershipRequestsQuery request
    )
    {
        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(m => m.Status == status);
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<ClassMemberView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase) { ["joinedat"] = m => m.JoinedAt };

    protected override IQueryable<MembershipRequestDto> ApplyProjection(
        IQueryable<ClassMemberView> query
    ) =>
        query.Select(m => new MembershipRequestDto
        {
            PublicId = m.PublicId,
            ClassPublicId = m.ClassPublicId,
            ClassName = m.ClassName,
            SubjectName = m.SubjectName,
            OwnerName = m.OwnerName,
            Status = m.Status,
            JoinedAt = m.JoinedAt,
            ProcessedAt = m.ProcessedAt,
            RejectionReason = m.RejectionReason,
        });
}
