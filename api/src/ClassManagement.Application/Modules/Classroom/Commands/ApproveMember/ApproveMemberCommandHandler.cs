using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Enums;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class ApproveMemberCommandHandler : IRequestHandler<ApproveMemberCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IClassroomPolicy _policy;

    public ApproveMemberCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notifications,
        IClassroomPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notifications = notifications;
        _policy = policy;
    }

    public async Task Handle(ApproveMemberCommand request, CancellationToken ct)
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

        // Live cap on approved students per class (0 = unlimited; system_settings-backed).
        var maxStudents = await _policy.GetMaxStudentsPerClassAsync(ct);
        if (maxStudents > 0)
        {
            var approved = await _unitOfWork.ClassMemberships.CountApprovedAsync(cls.Id, ct);
            if (approved >= maxStudents)
                throw new BadException("Class.MaxStudentsReached");
        }

        membership.Status = MembershipStatus.Approved;
        membership.ProcessedAt = DateTime.UtcNow;
        membership.ProcessedBy = _currentUser.UserId;

        _unitOfWork.ClassMemberships.Update(membership);

        await _notifications.NotifyAsync(
            membership.StudentId,
            new NotificationContent
            {
                EventType = NotificationEventType.ClassJoinApproved,
                Title = "Class join request approved",
                Body = $"You have been approved to join {cls.Name}.",
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
