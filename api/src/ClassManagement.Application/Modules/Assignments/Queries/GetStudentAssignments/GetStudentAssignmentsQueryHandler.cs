using System.Linq.Expressions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetStudentAssignmentsQueryHandler
    : BaseGetQueryHandler<
        GetStudentAssignmentsQuery,
        StudentAssignmentView,
        StudentAssignmentListDto
    >
{
    private readonly ICurrentUserService _currentUser;

    public GetStudentAssignmentsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    // The view already restricts to published assignments of classes the student is approved in; scope
    // to the current student so search/filter/COUNT all honour it.
    protected override IQueryable<StudentAssignmentView> GetBaseQuery(
        IGenericRepository<StudentAssignmentView> repo
    )
    {
        var studentId = _currentUser.UserId ?? -1;
        return repo.Query().Where(a => a.StudentId == studentId);
    }

    protected override IQueryable<StudentAssignmentView> ApplySearch(
        IQueryable<StudentAssignmentView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(a => a.Title.ToLower().Contains(keyword));
    }

    protected override IQueryable<StudentAssignmentView> ApplyFilter(
        IQueryable<StudentAssignmentView> query,
        GetStudentAssignmentsQuery request
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

    protected override Dictionary<
        string,
        Expression<Func<StudentAssignmentView, object>>
    > SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [AssignmentSortKeys.CreatedAt] = a => a.CreatedAt,
            [AssignmentSortKeys.Title] = a => a.Title,
            [AssignmentSortKeys.OpensAt] = a => a.OpensAt!,
            [AssignmentSortKeys.ClosesAt] = a => a.ClosesAt!,
        };

    protected override IQueryable<StudentAssignmentListDto> ApplyProjection(
        IQueryable<StudentAssignmentView> query
    ) =>
        query.Select(a => new StudentAssignmentListDto
        {
            PublicId = a.PublicId,
            Title = a.Title,
            Status = a.Status,
            OpensAt = a.OpensAt,
            ClosesAt = a.ClosesAt,
            TimeLimitMinutes = a.TimeLimitMinutes,
            MaxAttempts = a.MaxAttempts,
            AllowLate = a.AllowLate,
            ClassPublicId = a.ClassPublicId,
            ClassName = a.ClassName,
            TeacherName = a.TeacherName,
            TotalPoint = a.TotalPoint,
            TotalQuestions = a.TotalQuestions,
            UsedAttempts = a.UsedAttempts,
            AttemptsLeft = a.MaxAttempts - a.UsedAttempts < 0 ? 0 : a.MaxAttempts - a.UsedAttempts,
            HasInProgress = a.HasInProgress,
            InProgressAttemptPublicId = a.InProgressAttemptPublicId,
            BestScore = a.BestScore,
        });
}
