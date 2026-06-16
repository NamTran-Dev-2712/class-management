using ClassManagement.Domain.Modules.Questions.Enums;

public class GetPublicQuestionsQueryHandler : QuestionListHandlerBase<GetPublicQuestionsQuery>
{
    private static readonly string PublicVisibility = QuestionVisibility.Public.ToString();

    public GetPublicQuestionsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // Scoped to Public questions only (any teacher). Same data for everyone → Shared output cache.
    protected override IQueryable<QuestionView> GetBaseQuery(
        IGenericRepository<QuestionView> repo
    ) => repo.Query().Where(q => q.Visibility == PublicVisibility);
}
