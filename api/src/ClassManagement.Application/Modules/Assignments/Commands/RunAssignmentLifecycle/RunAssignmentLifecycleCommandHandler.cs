using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class RunAssignmentLifecycleCommandHandler : IRequestHandler<RunAssignmentLifecycleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAutoGradingService _grader;
    private readonly IAssignmentPolicy _policy;

    public RunAssignmentLifecycleCommandHandler(
        IUnitOfWork unitOfWork,
        IAutoGradingService grader,
        IAssignmentPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _grader = grader;
        _policy = policy;
    }

    public async Task Handle(RunAssignmentLifecycleCommand request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // 1. Scheduled → Open (opens_at reached).
        var due = await _unitOfWork.Assignments.GetScheduledDueAsync(now, ct);
        foreach (var assignment in due)
        {
            assignment.Status = AssignmentStatus.Open;
            _unitOfWork.Assignments.Update(assignment);
        }

        // 2. Open → Closed (closes_at passed) + auto-submit their InProgress attempts (BR-5-08).
        var closing = await _unitOfWork.Assignments.GetOpenClosingDueAsync(now, ct);
        foreach (var assignment in closing)
        {
            assignment.Status = AssignmentStatus.Closed;
            assignment.ClosedAt = now;
            _unitOfWork.Assignments.Update(assignment);

            var inProgress = await _unitOfWork.Attempts.GetInProgressByAssignmentAsync(
                assignment.Id,
                ct
            );
            foreach (var attempt in inProgress)
                await AttemptGrading.FinalizeAsync(_unitOfWork, _grader, attempt, now, true, ct);
        }

        // 3. Auto-submit attempts whose per-attempt deadline passed (BR-5-07), bounded per sweep.
        var expired = await _unitOfWork.Attempts.GetExpiredInProgressAsync(
            now,
            _policy.LifecycleBatchSize,
            ct
        );
        foreach (var attempt in expired)
            await AttemptGrading.FinalizeAsync(_unitOfWork, _grader, attempt, now, true, ct);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
