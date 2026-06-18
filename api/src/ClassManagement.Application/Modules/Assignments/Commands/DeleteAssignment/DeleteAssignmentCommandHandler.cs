using ClassManagement.Application.Exceptions;

public class DeleteAssignmentCommandHandler : IRequestHandler<DeleteAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteAssignmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteAssignmentCommand request, CancellationToken ct)
    {
        var assignment = AssignmentGuard.EnsureOwned(
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // Preserve history: an assignment that students have attempted cannot be deleted.
        if (await _unitOfWork.Assignments.HasAttemptsAsync(assignment.Id, ct))
            throw new ConflictException("Assignment.HasAttempts");

        _unitOfWork.Assignments.Remove(assignment); // soft-delete via interceptor
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
