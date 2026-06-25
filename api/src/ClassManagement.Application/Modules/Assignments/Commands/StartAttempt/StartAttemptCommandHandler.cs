using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class StartAttemptCommandHandler : IRequestHandler<StartAttemptCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAssignmentPolicy _policy;

    public StartAttemptCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAssignmentPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _policy = policy;
    }

    public async Task<Guid> Handle(StartAttemptCommand request, CancellationToken ct)
    {
        var studentId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var assignment =
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.AssignmentId, false, ct)
            ?? throw new NotFoundException("Assignment.NotFound");

        var now = DateTime.UtcNow;

        // The assignment must be effectively open: published, opens_at reached, closes_at not passed
        // (lazy state check — the lifecycle job also flips the persisted status).
        if (assignment.Status is AssignmentStatus.Draft or AssignmentStatus.Archived)
            throw new NotFoundException("Assignment.NotFound");
        if (assignment.Status == AssignmentStatus.Closed)
            throw new BadException("Assignment.Closed");
        if (assignment.OpensAt is DateTime opensAt && now < opensAt)
            throw new BadException("Assignment.NotOpen");
        if (assignment.ClosesAt is DateTime closesAt && now > closesAt)
            throw new BadException("Assignment.Closed");

        // Only an approved member of the class may start (BR-5-09 — kicked/left students cannot start new).
        if (
            !await _unitOfWork.ClassMemberships.IsApprovedMemberAsync(
                assignment.ClassId,
                studentId,
                ct
            )
        )
            throw new ForbiddenException("Class.NotMember");

        // One InProgress attempt at a time (BR-5-06): return the existing one instead of creating a new.
        var existing = await _unitOfWork.Attempts.GetInProgressAsync(assignment.Id, studentId, ct);
        if (existing is not null)
            return existing.PublicId;

        // Max attempts (BR-5-05). The effective ceiling is the lower of the assignment's own MaxAttempts
        // and the live global cap (system_settings.max_attempts_per_assignment; 0 = unlimited).
        var usedAttempts = await _unitOfWork.Attempts.CountByStudentAsync(
            assignment.Id,
            studentId,
            ct
        );
        var globalCap = await _policy.GetMaxAttemptsPerAssignmentAsync(ct);
        var effectiveMax =
            globalCap > 0 ? Math.Min(assignment.MaxAttempts, globalCap) : assignment.MaxAttempts;
        if (usedAttempts >= effectiveMax)
            throw new BadException("Assignment.MaxAttemptsReached");

        // Server-computed deadline (BR-5-07): started_at + time limit, capped at the assignment close.
        DateTime? deadline = null;
        if (assignment.TimeLimitMinutes is int minutes)
        {
            var candidate = now.AddMinutes(minutes);
            deadline = assignment.ClosesAt is DateTime cap && cap < candidate ? cap : candidate;
        }
        else if (assignment.ClosesAt is DateTime close)
        {
            deadline = close;
        }

        // Per-attempt question order (BR-5-10), fixed at Start.
        var snapshotRepo = _unitOfWork.Repository<AssignmentSnapshot>();
        var snapshotId = (
            await snapshotRepo.ToListAsync(
                snapshotRepo
                    .Query()
                    .Where(s => s.AssignmentId == assignment.Id)
                    .Select(s => s.Id)
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        if (snapshotId == 0)
            throw new BadException("Assignment.SnapshotMissing");

        var sqRepo = _unitOfWork.Repository<SnapshotQuestion>();
        var orderedQuestionIds = await sqRepo.ToListAsync(
            sqRepo
                .Query()
                .Where(q => q.SnapshotId == snapshotId)
                .OrderBy(q => q.DisplayOrder)
                .Select(q => q.Id),
            ct
        );

        var questionOrder = orderedQuestionIds.ToList();
        if (assignment.ShuffleQuestions)
            Shuffle(questionOrder);

        var attempt = new Attempt
        {
            AssignmentId = assignment.Id,
            StudentId = studentId,
            AttemptNumber = usedAttempts + 1,
            Status = AttemptStatus.InProgress,
            StartedAt = now,
            DeadlineAt = deadline,
            QuestionOrder = questionOrder,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
        };

        await _unitOfWork.Attempts.AddAsync(attempt, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return attempt.PublicId;
    }

    private static void Shuffle(List<long> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
