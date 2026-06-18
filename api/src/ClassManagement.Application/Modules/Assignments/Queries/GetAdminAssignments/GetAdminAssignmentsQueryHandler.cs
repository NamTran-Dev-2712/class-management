public class GetAdminAssignmentsQueryHandler : AssignmentListHandlerBase<GetAdminAssignmentsQuery>
{
    public GetAdminAssignmentsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // No scope — admins see every (non-deleted) assignment; the base pipeline handles the rest.
}
