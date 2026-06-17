using ClassManagement.Domain.Modules.Exams.Enums;

public class GetPublicExamsQueryHandler : ExamListHandlerBase<GetPublicExamsQuery>
{
    private static readonly string PublicVisibility = ExamVisibility.Public.ToString();

    public GetPublicExamsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // Scoped to Public exams only (any teacher). Same data for everyone → Shared output cache.
    protected override IQueryable<ExamView> GetBaseQuery(IGenericRepository<ExamView> repo) =>
        repo.Query().Where(e => e.Visibility == PublicVisibility);
}
