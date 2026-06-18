using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class ArchiveAssignmentCommandHandler : IRequestHandler<ArchiveAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ArchiveAssignmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ArchiveAssignmentCommand request, CancellationToken ct)
    {
        var assignment = AssignmentGuard.EnsureOwned(
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        if (assignment.Status == AssignmentStatus.Archived)
            throw new BadException("Assignment.AlreadyArchived");

        assignment.Status = AssignmentStatus.Archived;
        _unitOfWork.Assignments.Update(assignment);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
