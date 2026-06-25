using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Assignments.Enums;

// Admin emergency force-close of any assignment (A7-07). Same close + auto-submit path as the teacher
// CloseAssignment, but with no owner guard (admin authority) and a dedicated audit action.
public record ForceCloseAssignmentCommand(Guid PublicId) : IRequest;

public class ForceCloseAssignmentCommandHandler : IRequestHandler<ForceCloseAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAutoGradingService _grader;
    private readonly IAuditLogger _auditLogger;

    public ForceCloseAssignmentCommandHandler(
        IUnitOfWork unitOfWork,
        IAutoGradingService grader,
        IAuditLogger auditLogger
    )
    {
        _unitOfWork = unitOfWork;
        _grader = grader;
        _auditLogger = auditLogger;
    }

    public async Task Handle(ForceCloseAssignmentCommand request, CancellationToken ct)
    {
        var assignment =
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.PublicId, false, ct)
            ?? throw new NotFoundException("Assignment.NotFound");

        if (assignment.Status is not (AssignmentStatus.Open or AssignmentStatus.Scheduled))
            throw new BadException("Assignment.NotOpen");

        var now = DateTime.UtcNow;
        assignment.Status = AssignmentStatus.Closed;
        assignment.ClosedAt = now;
        _unitOfWork.Assignments.Update(assignment);

        var inProgress = await _unitOfWork.Attempts.GetInProgressByAssignmentAsync(
            assignment.Id,
            ct
        );
        foreach (var attempt in inProgress)
            await AttemptGrading.FinalizeAsync(_unitOfWork, _grader, attempt, now, true, ct);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.AdminAssignmentForceClosed,
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
