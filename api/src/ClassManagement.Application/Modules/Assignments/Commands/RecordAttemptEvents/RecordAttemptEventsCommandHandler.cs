using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Entities;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class RecordAttemptEventsCommandHandler
    : IRequestHandler<RecordAttemptEventsCommand, RecordEventsResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAutoGradingService _grader;

    public RecordAttemptEventsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAutoGradingService grader
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _grader = grader;
    }

    public async Task<RecordEventsResultDto> Handle(
        RecordAttemptEventsCommand request,
        CancellationToken ct
    )
    {
        // Load with answers so a threshold auto-submit can finalize in the same transaction.
        var attempt = AssignmentGuard.EnsureAttemptOwner(
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct),
            _currentUser.UserId
        );

        // Same guard chain as SaveAttemptAnswers (server time authoritative), plus the lock reject.
        if (attempt.Status != AttemptStatus.InProgress)
            throw new BadException("Attempt.NotInProgress");
        if (attempt.IsLocked)
            throw new BadException("Attempt.Locked");

        var now = DateTime.UtcNow;
        if (attempt.DeadlineAt is DateTime deadline && now > deadline)
            throw new BadException("Attempt.DeadlinePassed");

        // Resolve the assignment's violation threshold + action (config lives on the entity).
        var config = await ResolveConfigAsync(attempt.AssignmentId, ct);

        // Append the reported events (append-only evidence). Each counts toward the threshold.
        var newEvents = request
            .Events.Select(e => new AttemptEvent
            {
                AttemptId = attempt.Id,
                EventType = e.EventType,
                OccurredAt = NormalizeOccurredAt(e.OccurredAt, now),
                Metadata = e.Metadata,
            })
            .ToList();

        if (newEvents.Count > 0)
        {
            await _unitOfWork.Repository<AttemptEvent>().AddRangeAsync(newEvents, ct);
            attempt.ViolationCount += newEvents.Count;
            attempt.LastEventAt = now;
        }

        var submitted = false;
        var locked = false;
        var thresholdExceeded =
            config.MaxViolations > 0 && attempt.ViolationCount > config.MaxViolations;

        if (thresholdExceeded)
        {
            attempt.IsFlagged = true;
            switch (config.ViolationAction)
            {
                case ViolationAction.AutoSubmit:
                    await AttemptGrading.FinalizeAsync(
                        _unitOfWork,
                        _grader,
                        attempt,
                        now,
                        autoSubmitted: true,
                        ct
                    );
                    submitted = true;
                    break;
                case ViolationAction.LockAttempt:
                    attempt.IsLocked = true;
                    locked = true;
                    break;
                case ViolationAction.WarnOnly:
                default:
                    break;
            }
        }

        _unitOfWork.Attempts.Update(attempt);
        await _unitOfWork.SaveChangesAsync(ct);

        return new RecordEventsResultDto
        {
            ViolationCount = attempt.ViolationCount,
            MaxViolations = config.MaxViolations,
            Action = config.ViolationAction.ToString(),
            ThresholdExceeded = thresholdExceeded,
            Submitted = submitted,
            Locked = locked,
        };
    }

    private async Task<ProctoringConfig> ResolveConfigAsync(long assignmentId, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Assignment>();
        var rows = await repo.ToListAsync(
            repo.Query()
                .Where(a => a.Id == assignmentId)
                .Select(a => new ProctoringConfig(a.MaxViolations, a.ViolationAction))
                .Take(1),
            ct
        );
        return rows.First();
    }

    // Use the client clock only when it looks sane (not in the future, not absurdly old); else the
    // server's received-at. CreatedAt is always the authoritative server timestamp.
    private static DateTime NormalizeOccurredAt(DateTime? reported, DateTime now)
    {
        if (reported is not DateTime value)
            return now;
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        if (utc > now.AddMinutes(1) || utc < now.AddHours(-24))
            return now;
        return utc;
    }

    private readonly record struct ProctoringConfig(
        int MaxViolations,
        ViolationAction ViolationAction
    );
}
