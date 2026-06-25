using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Admin.Enums;

public class ReleaseAssignmentGradesCommandHandler : IRequestHandler<ReleaseAssignmentGradesCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notifications;

    public ReleaseAssignmentGradesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger,
        INotificationService notifications
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
        _notifications = notifications;
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

            await _auditLogger.LogAsync(
                new AuditEntry
                {
                    Action = AuditActions.GradePublished,
                    TargetType = AuditTargetTypes.Assignment,
                    TargetId = assignment.Id,
                    TargetPublicId = assignment.PublicId,
                },
                ct
            );

            // Notify every student who has an attempt in this assignment that grades are published.
            var attemptRepo = _unitOfWork.Repository<Attempt>();
            var studentIds = await attemptRepo.ToListAsync(
                attemptRepo
                    .Query()
                    .Where(a => a.AssignmentId == assignment.Id)
                    .Select(a => a.StudentId)
                    .Distinct(),
                ct
            );

            await _notifications.NotifyManyAsync(
                studentIds,
                new NotificationContent
                {
                    EventType = NotificationEventType.GradePublished,
                    Title = "Grades published",
                    Body = $"Grades for \"{assignment.Title}\" are now available.",
                    Link = $"/student/assignments/{assignment.PublicId}",
                    ReferenceId = assignment.PublicId.ToString(),
                    Payload = new Dictionary<string, object?>
                    {
                        ["assignmentTitle"] = assignment.Title,
                        ["assignmentPublicId"] = assignment.PublicId.ToString(),
                    },
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
