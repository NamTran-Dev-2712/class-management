using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class KickMemberCommandHandler : IRequestHandler<KickMemberCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public KickMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(KickMemberCommand request, CancellationToken ct)
    {
        var cls = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.ClassPublicId, ct),
            _currentUser.UserId
        );

        var membership = ClassroomGuard.EnsureInClass(
            await _unitOfWork.ClassMemberships.GetByPublicIdAsync(request.MembershipPublicId, ct),
            cls.Id
        );

        if (membership.Status != MembershipStatus.Approved)
            throw new BadException("Membership.NotApproved");

        membership.Status = MembershipStatus.Removed;
        membership.ProcessedAt = DateTime.UtcNow;
        membership.ProcessedBy = _currentUser.UserId;

        _unitOfWork.ClassMemberships.Update(membership);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
