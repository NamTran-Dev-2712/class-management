public class GetAdminExamsQueryHandler : ExamListHandlerBase<GetAdminExamsQuery>
{
    public GetAdminExamsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // No scope — admin sees all exams (the base query already excludes soft-deleted rows).
}
