public class GetTeacherQuestionsQueryHandler : QuestionListHandlerBase<GetTeacherQuestionsQuery>
{
    private readonly ICurrentUserService _currentUser;

    public GetTeacherQuestionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    // Scoped to the calling teacher's own questions, applied to the base query so search/filter/COUNT
    // all respect it (a non-owner naturally sees an empty page).
    protected override IQueryable<QuestionView> GetBaseQuery(IGenericRepository<QuestionView> repo)
    {
        var teacherId = _currentUser.UserId ?? -1;
        return repo.Query().Where(q => q.TeacherId == teacherId);
    }
}
