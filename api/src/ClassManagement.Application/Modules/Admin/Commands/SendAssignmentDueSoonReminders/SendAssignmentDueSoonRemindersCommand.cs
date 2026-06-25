using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Classroom.Enums;

// System job (MVP-7): for each Open assignment whose deadline is within the configured window, remind
// every approved student who has not submitted (no attempt, or only an InProgress one — BR-7-07).
// Idempotent: NotifyMany skips students already reminded for that assignment (reference id + index).
public record SendAssignmentDueSoonRemindersCommand : IRequest;

public class SendAssignmentDueSoonRemindersCommandHandler
    : IRequestHandler<SendAssignmentDueSoonRemindersCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notifications;
    private readonly ISystemSettingsService _settings;

    public SendAssignmentDueSoonRemindersCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notifications,
        ISystemSettingsService settings
    )
    {
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _settings = settings;
    }

    public async Task Handle(SendAssignmentDueSoonRemindersCommand request, CancellationToken ct)
    {
        var hours = await _settings.GetIntAsync(SystemSettingKeys.AssignmentDueSoonHours, 24, ct);
        var now = DateTime.UtcNow;
        var windowEnd = now.AddHours(hours);

        var assignmentRepo = _unitOfWork.Repository<Assignment>();
        var due = await assignmentRepo.ToListAsync(
            assignmentRepo
                .Query()
                .Where(a =>
                    a.Status == AssignmentStatus.Open
                    && a.ClosesAt != null
                    && a.ClosesAt > now
                    && a.ClosesAt <= windowEnd
                )
                .Select(a => new DueAssignment(
                    a.Id,
                    a.PublicId,
                    a.Title,
                    a.ClassId,
                    a.ClosesAt!.Value
                )),
            ct
        );

        if (due.Count == 0)
            return;

        var memberRepo = _unitOfWork.Repository<ClassMembership>();
        var attemptRepo = _unitOfWork.Repository<Attempt>();

        foreach (var assignment in due)
        {
            var approvedIds = await memberRepo.ToListAsync(
                memberRepo
                    .Query()
                    .Where(m =>
                        m.ClassId == assignment.ClassId && m.Status == MembershipStatus.Approved
                    )
                    .Select(m => m.StudentId),
                ct
            );
            if (approvedIds.Count == 0)
                continue;

            // A student "submitted" if they have any non-InProgress attempt for this assignment.
            var submittedIds = await attemptRepo.ToListAsync(
                attemptRepo
                    .Query()
                    .Where(a =>
                        a.AssignmentId == assignment.Id && a.Status != AttemptStatus.InProgress
                    )
                    .Select(a => a.StudentId)
                    .Distinct(),
                ct
            );

            var targets = approvedIds.Except(submittedIds).ToList();
            if (targets.Count == 0)
                continue;

            await _notifications.NotifyManyAsync(
                targets,
                new NotificationContent
                {
                    EventType = NotificationEventType.AssignmentDueSoon,
                    Title = "Assignment due soon",
                    Body = $"\"{assignment.Title}\" is due soon. Don't forget to submit.",
                    Link = $"/student/assignments/{assignment.PublicId}",
                    ReferenceId = assignment.PublicId.ToString(),
                    Payload = new Dictionary<string, object?>
                    {
                        ["assignmentTitle"] = assignment.Title,
                        ["assignmentPublicId"] = assignment.PublicId.ToString(),
                        ["closesAt"] = assignment.ClosesAt,
                    },
                },
                ct
            );
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private readonly record struct DueAssignment(
        long Id,
        Guid PublicId,
        string Title,
        long ClassId,
        DateTime ClosesAt
    );
}
