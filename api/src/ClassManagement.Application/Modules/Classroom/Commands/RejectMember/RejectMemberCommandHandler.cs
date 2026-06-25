using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Enums;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class RejectMemberCommandHandler : IRequestHandler<RejectMemberCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public RejectMemberCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notifications
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(RejectMemberCommand request, CancellationToken ct)
    {
        var cls = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.ClassPublicId, ct),
            _currentUser.UserId
        );

        var membership = ClassroomGuard.EnsureInClass(
            await _unitOfWork.ClassMemberships.GetByPublicIdAsync(request.MembershipPublicId, ct),
            cls.Id
        );

        if (membership.Status != MembershipStatus.Pending)
            throw new BadException("Membership.NotPending");

        membership.Status = MembershipStatus.Rejected;
        membership.ProcessedAt = DateTime.UtcNow;
        membership.ProcessedBy = _currentUser.UserId;
        membership.RejectionReason = request.RejectionReason?.Trim();

        _unitOfWork.ClassMemberships.Update(membership);

        await _notifications.NotifyAsync(
            membership.StudentId,
            new NotificationContent
            {
                EventType = NotificationEventType.ClassJoinRejected,
                Title = "Class join request rejected",
                Body = $"Your request to join {cls.Name} was not approved.",
                Link = "/student/classes",
                ReferenceId = membership.PublicId.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["className"] = cls.Name,
                    ["classPublicId"] = cls.PublicId.ToString(),
                },
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
