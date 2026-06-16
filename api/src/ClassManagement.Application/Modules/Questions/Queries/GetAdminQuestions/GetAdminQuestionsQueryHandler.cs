public class GetAdminQuestionsQueryHandler : QuestionListHandlerBase<GetAdminQuestionsQuery>
{
    public GetAdminQuestionsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // No scope — admin sees all questions (the base query already excludes soft-deleted rows).
}
