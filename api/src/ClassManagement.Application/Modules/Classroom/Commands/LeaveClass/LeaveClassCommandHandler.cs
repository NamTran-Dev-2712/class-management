using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class LeaveClassCommandHandler : IRequestHandler<LeaveClassCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public LeaveClassCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(LeaveClassCommand request, CancellationToken ct)
    {
        var studentId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var cls =
            await _unitOfWork.Classes.GetByPublicIdAsync(request.ClassPublicId, ct)
            ?? throw new NotFoundException("Class.NotFound");

        var membership = await _unitOfWork.ClassMemberships.GetActiveForStudentAsync(
            cls.Id,
            studentId,
            ct
        );

        if (membership is null || membership.Status != MembershipStatus.Approved)
            throw new BadException("Membership.NotApproved");

        membership.Status = MembershipStatus.Left;
        membership.ProcessedAt = DateTime.UtcNow;

        _unitOfWork.ClassMemberships.Update(membership);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
