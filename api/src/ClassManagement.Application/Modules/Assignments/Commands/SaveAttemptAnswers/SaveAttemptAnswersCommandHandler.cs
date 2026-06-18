using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class SaveAttemptAnswersCommandHandler : IRequestHandler<SaveAttemptAnswersCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SaveAttemptAnswersCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(SaveAttemptAnswersCommand request, CancellationToken ct)
    {
        var attempt = AssignmentGuard.EnsureAttemptOwner(
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct),
            _currentUser.UserId
        );

        // Saving is only allowed while InProgress and before the deadline (server time is authoritative).
        if (attempt.Status != AttemptStatus.InProgress)
            throw new BadException("Attempt.NotInProgress");

        var now = DateTime.UtcNow;
        if (attempt.DeadlineAt is DateTime deadline && now > deadline)
            throw new BadException("Attempt.DeadlinePassed");

        if (request.Answers.Count == 0)
            return;

        // Resolve the snapshot questions + options of this attempt (public id → internal id) so we can
        // validate the answer refs belong to this attempt and store internal ids.
        var snapshotRepo = _unitOfWork.Repository<AssignmentSnapshot>();
        var snapshotId = (
            await snapshotRepo.ToListAsync(
                snapshotRepo
                    .Query()
                    .Where(s => s.AssignmentId == attempt.AssignmentId)
                    .Select(s => s.Id)
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        var sqRepo = _unitOfWork.Repository<SnapshotQuestion>();
        var questionMap = (
            await sqRepo.ToListAsync(
                sqRepo
                    .Query()
                    .Where(q => q.SnapshotId == snapshotId)
                    .Select(q => new { q.Id, q.PublicId }),
                ct
            )
        ).ToDictionary(q => q.PublicId, q => q.Id);

        var soRepo = _unitOfWork.Repository<SnapshotOption>();
        var optionRows = await soRepo.ToListAsync(
            soRepo
                .Query()
                .Where(o => questionMap.Values.Contains(o.SnapshotQuestionId))
                .Select(o => new
                {
                    o.Id,
                    o.PublicId,
                    o.SnapshotQuestionId,
                }),
            ct
        );
        var optionIdByPublicId = optionRows.ToDictionary(o => o.PublicId, o => o.Id);
        var optionQuestionById = optionRows.ToDictionary(o => o.Id, o => o.SnapshotQuestionId);

        var answerByQuestion = attempt.Answers.ToDictionary(a => a.SnapshotQuestionId);

        foreach (var input in request.Answers)
        {
            if (!questionMap.TryGetValue(input.QuestionId, out var snapshotQuestionId))
                throw new BadException("Attempt.QuestionNotInAttempt");

            // Map + validate selected option refs (each must belong to this question).
            List<long>? selected = null;
            if (input.SelectedOptionIds.Count > 0)
            {
                selected = [];
                foreach (var optionPublicId in input.SelectedOptionIds)
                {
                    if (
                        !optionIdByPublicId.TryGetValue(optionPublicId, out var optionId)
                        || optionQuestionById[optionId] != snapshotQuestionId
                    )
                        throw new BadException("Attempt.OptionNotInQuestion");
                    selected.Add(optionId);
                }
            }

            var text = string.IsNullOrWhiteSpace(input.TextAnswer) ? null : input.TextAnswer;

            if (answerByQuestion.TryGetValue(snapshotQuestionId, out var existing))
            {
                existing.SelectedOptionIds = selected;
                existing.TextAnswer = text;
                existing.LastSavedAt = now;
            }
            else
            {
                attempt.Answers.Add(
                    new AttemptAnswer
                    {
                        SnapshotQuestionId = snapshotQuestionId,
                        SelectedOptionIds = selected,
                        TextAnswer = text,
                        LastSavedAt = now,
                    }
                );
            }
        }

        _unitOfWork.Attempts.Update(attempt);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
