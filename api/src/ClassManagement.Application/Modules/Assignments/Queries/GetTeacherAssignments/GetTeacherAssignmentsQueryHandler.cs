public class GetTeacherAssignmentsQueryHandler
    : AssignmentListHandlerBase<GetTeacherAssignmentsQuery>
{
    private readonly ICurrentUserService _currentUser;

    public GetTeacherAssignmentsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    // Scoped to the calling teacher's own assignments (a non-owner sees an empty page).
    protected override IQueryable<AssignmentView> GetBaseQuery(
        IGenericRepository<AssignmentView> repo
    )
    {
        var teacherId = _currentUser.UserId ?? -1;
        return repo.Query().Where(a => a.TeacherId == teacherId);
    }
}
