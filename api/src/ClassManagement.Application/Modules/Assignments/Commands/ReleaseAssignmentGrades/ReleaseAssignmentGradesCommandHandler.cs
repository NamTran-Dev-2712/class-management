public class ReleaseAssignmentGradesCommandHandler : IRequestHandler<ReleaseAssignmentGradesCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ReleaseAssignmentGradesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ReleaseAssignmentGradesCommand request, CancellationToken ct)
    {
        var assignment = AssignmentGuard.EnsureOwned(
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // Idempotent: publishing again is a no-op (the first release time stands).
        if (assignment.GradesReleasedAt is null)
        {
            assignment.GradesReleasedAt = DateTime.UtcNow;
            _unitOfWork.Assignments.Update(assignment);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
