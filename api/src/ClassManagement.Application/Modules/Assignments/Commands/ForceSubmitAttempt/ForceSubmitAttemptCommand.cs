using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Assignments.Enums;

// Teacher/Admin force-submits an in-progress attempt (MVP-10, permission matrix §9). Uses the shared
// finalize path (auto_submitted = true), unlocks it, and audits the action. OwnerScoped = true for the
// teacher surface (must own the assignment), false for admin. AttemptId comes from the route.
public record ForceSubmitAttemptCommand(Guid AttemptId, bool OwnerScoped) : IRequest;

public class ForceSubmitAttemptCommandHandler : IRequestHandler<ForceSubmitAttemptCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAutoGradingService _grader;
    private readonly IAuditLogger _auditLogger;

    public ForceSubmitAttemptCommandHandler(
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

    public async Task Handle(ForceSubmitAttemptCommand request, CancellationToken ct)
    {
        var attempt = await AttemptModeration.ResolveAsync(
            _unitOfWork,
            _currentUser.UserId,
            request.AttemptId,
            request.OwnerScoped,
            ct
        );

        if (attempt.Status != AttemptStatus.InProgress)
            throw new BadException("Attempt.NotInProgress");

        var now = DateTime.UtcNow;
        attempt.IsLocked = false;
        await AttemptGrading.FinalizeAsync(_unitOfWork, _grader, attempt, now, true, ct);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.AttemptForceSubmitted,
                TargetType = AuditTargetTypes.Attempt,
                TargetId = attempt.Id,
                TargetPublicId = attempt.PublicId,
                Metadata = new Dictionary<string, object?>
                {
                    ["violation_count"] = attempt.ViolationCount,
                },
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
