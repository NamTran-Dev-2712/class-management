public class GetTeacherExamsQueryHandler : ExamListHandlerBase<GetTeacherExamsQuery>
{
    private readonly ICurrentUserService _currentUser;

    public GetTeacherExamsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    // Scoped to the calling teacher's own exams, applied to the base query so search/filter/COUNT all
    // respect it (a non-owner naturally sees an empty page).
    protected override IQueryable<ExamView> GetBaseQuery(IGenericRepository<ExamView> repo)
    {
        var teacherId = _currentUser.UserId ?? -1;
        return repo.Query().Where(e => e.TeacherId == teacherId);
    }
}
