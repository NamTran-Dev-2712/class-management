using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class SubmitAttemptCommandHandler : IRequestHandler<SubmitAttemptCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAutoGradingService _grader;

    public SubmitAttemptCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAutoGradingService grader
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _grader = grader;
    }

    public async Task Handle(SubmitAttemptCommand request, CancellationToken ct)
    {
        var attempt = AssignmentGuard.EnsureAttemptOwner(
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct),
            _currentUser.UserId
        );

        // Idempotency (BR-5 race guard): only an InProgress attempt can be submitted.
        if (attempt.Status != AttemptStatus.InProgress)
            throw new BadException("Attempt.AlreadySubmitted");

        var now = DateTime.UtcNow;
        var autoSubmitted = attempt.DeadlineAt is DateTime deadline && now > deadline;

        await AttemptGrading.FinalizeAsync(_unitOfWork, _grader, attempt, now, autoSubmitted, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
