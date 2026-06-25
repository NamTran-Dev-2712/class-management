using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class CloseAssignmentCommandHandler : IRequestHandler<CloseAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAutoGradingService _grader;
    private readonly IAuditLogger _auditLogger;

    public CloseAssignmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAutoGradingService grader,
        IAuditLogger auditLogger
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _grader = grader;
        _auditLogger = auditLogger;
    }

    public async Task Handle(CloseAssignmentCommand request, CancellationToken ct)
    {
        var assignment = AssignmentGuard.EnsureOwned(
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // Only an Open (or Scheduled) assignment can be closed early.
        if (assignment.Status is not (AssignmentStatus.Open or AssignmentStatus.Scheduled))
            throw new BadException("Assignment.NotOpen");

        var now = DateTime.UtcNow;
        assignment.Status = AssignmentStatus.Closed;
        assignment.ClosedAt = now;
        _unitOfWork.Assignments.Update(assignment);

        // Auto-submit every InProgress attempt (BR-5-08).
        var inProgress = await _unitOfWork.Attempts.GetInProgressByAssignmentAsync(
            assignment.Id,
            ct
        );
        foreach (var attempt in inProgress)
            await AttemptGrading.FinalizeAsync(_unitOfWork, _grader, attempt, now, true, ct);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.AssignmentClosed,
                TargetType = AuditTargetTypes.Assignment,
                TargetId = assignment.Id,
                TargetPublicId = assignment.PublicId,
                Metadata = new Dictionary<string, object?>
                {
                    ["auto_submitted_attempts"] = inProgress.Count,
                },
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
