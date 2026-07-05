using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetAttemptForTakingQueryHandler
    : IRequestHandler<GetAttemptForTakingQuery, AttemptTakingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetAttemptForTakingQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AttemptTakingDto> Handle(
        GetAttemptForTakingQuery request,
        CancellationToken ct
    )
    {
        var attempt = AssignmentGuard.EnsureAttemptOwner(
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct),
            _currentUser.UserId
        );

        // Assignment display info + option-shuffle flag.
        var avRepo = _unitOfWork.Repository<AssignmentView>();
        var assignmentInfo = (
            await avRepo.ToListAsync(
                avRepo
                    .Query()
                    .Where(a => a.Id == attempt.AssignmentId)
                    .Select(a => new AssignmentInfo(
                        a.PublicId,
                        a.Title,
                        a.TotalPoint,
                        a.ShuffleOptions,
                        a.RequireFullscreen,
                        a.DetectTabSwitch,
                        a.BlockCopyPaste,
                        a.MaxViolations,
                        a.ViolationAction
                    ))
                    .Take(1),
                ct
            )
        ).First();

        var questionIds = attempt.QuestionOrder;

        var sqRepo = _unitOfWork.Repository<SnapshotQuestion>();
        var questions = (
            await sqRepo.ToListAsync(
                sqRepo
                    .Query()
                    .Where(q => questionIds.Contains(q.Id))
                    .Select(q => new QuestionRow(
                        q.Id,
                        q.PublicId,
                        q.Type.ToString(),
                        q.Content,
                        q.Point
                    )),
                ct
            )
        ).ToDictionary(q => q.Id);

        var soRepo = _unitOfWork.Repository<SnapshotOption>();
        var options = await soRepo.ToListAsync(
            soRepo
                .Query()
                .Where(o => questionIds.Contains(o.SnapshotQuestionId))
                .Select(o => new OptionRow(
                    o.SnapshotQuestionId,
                    o.Id,
                    o.PublicId,
                    o.Content,
                    o.DisplayOrder
                )),
            ct
        );
        var optionsByQuestion = options
            .GroupBy(o => o.SnapshotQuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var optionPublicIdById = options.ToDictionary(o => o.Id, o => o.PublicId);

        // Frozen media (MVP-9): question-level attachments + per-option images.
        var smRepo = _unitOfWork.Repository<SnapshotMedia>();
        var snapshotMedia = await smRepo.ToListAsync(
            smRepo
                .Query()
                .Where(m => questionIds.Contains(m.SnapshotQuestionId))
                .Select(m => new MediaRow(
                    m.SnapshotQuestionId,
                    m.SnapshotOptionId,
                    m.MediaPublicId,
                    m.FrozenUrl,
                    m.Kind.ToString(),
                    m.Role.ToString(),
                    m.DisplayOrder
                )),
            ct
        );
        var attachmentsByQuestion = snapshotMedia
            .Where(m => m.SnapshotOptionId == null && m.Role == "Attachment")
            .GroupBy(m => m.SnapshotQuestionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(m => m.DisplayOrder).ToList());
        var mediaByOption = snapshotMedia
            .Where(m => m.SnapshotOptionId != null)
            .GroupBy(m => m.SnapshotOptionId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var answerByQuestion = attempt.Answers.ToDictionary(a => a.SnapshotQuestionId);

        var takingQuestions = new List<TakingQuestionDto>();
        var position = 1;
        foreach (var qId in questionIds)
        {
            if (!questions.TryGetValue(qId, out var q))
                continue; // defensive: snapshot is immutable, so this should not happen

            var rawOptions = optionsByQuestion.TryGetValue(qId, out var list) ? list : [];

            // Option order: canonical, or deterministic per-attempt shuffle (stable across reloads).
            IEnumerable<OptionRow> orderedOptions = assignmentInfo.ShuffleOptions
                ? rawOptions.OrderBy(o => StableKey(attempt.PublicId, o.PublicId))
                : rawOptions.OrderBy(o => o.DisplayOrder);

            answerByQuestion.TryGetValue(qId, out var answer);
            var selectedPublicIds = answer?.SelectedOptionIds is { Count: > 0 } selected
                ? selected
                    .Where(optionPublicIdById.ContainsKey)
                    .Select(id => optionPublicIdById[id])
                    .ToList()
                : [];

            takingQuestions.Add(
                new TakingQuestionDto
                {
                    PublicId = q.PublicId,
                    DisplayPosition = position++,
                    Type = q.Type,
                    Content = q.Content,
                    Point = q.Point,
                    Options =
                    [
                        .. orderedOptions.Select(o => new TakingOptionDto
                        {
                            PublicId = o.PublicId,
                            Content = o.Content,
                            DisplayOrder = o.DisplayOrder,
                            MediaUrl = mediaByOption.TryGetValue(o.Id, out var om) ? om.Url : null,
                            MediaKind = mediaByOption.TryGetValue(o.Id, out var ok)
                                ? ok.Kind
                                : null,
                        }),
                    ],
                    Media = attachmentsByQuestion.TryGetValue(qId, out var atts)
                        ?
                        [
                            .. atts.Select(m => new TakingMediaDto
                            {
                                MediaPublicId = m.MediaPublicId,
                                Url = m.Url,
                                Kind = m.Kind,
                                DisplayOrder = m.DisplayOrder,
                            }),
                        ]
                        : [],
                    SelectedOptionIds = selectedPublicIds,
                    TextAnswer = answer?.TextAnswer,
                }
            );
        }

        long? remainingSeconds = null;
        if (attempt.DeadlineAt is DateTime deadline)
        {
            var remaining = (long)Math.Floor((deadline - DateTime.UtcNow).TotalSeconds);
            remainingSeconds = remaining < 0 ? 0 : remaining;
        }

        return new AttemptTakingDto
        {
            PublicId = attempt.PublicId,
            AssignmentPublicId = assignmentInfo.PublicId,
            AssignmentTitle = assignmentInfo.Title,
            Status = attempt.Status.ToString(),
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            DeadlineAt = attempt.DeadlineAt,
            RemainingSeconds = remainingSeconds,
            TotalPoint = assignmentInfo.TotalPoint,
            Questions = takingQuestions,
            ViolationCount = attempt.ViolationCount,
            IsLocked = attempt.IsLocked,
            Proctoring = new ProctoringConfigDto
            {
                RequireFullscreen = assignmentInfo.RequireFullscreen,
                DetectTabSwitch = assignmentInfo.DetectTabSwitch,
                BlockCopyPaste = assignmentInfo.BlockCopyPaste,
                MaxViolations = assignmentInfo.MaxViolations,
                ViolationAction = assignmentInfo.ViolationAction,
            },
        };
    }

    // FNV-1a over the two guids → a stable 64-bit sort key (deterministic across processes).
    private static long StableKey(Guid attemptId, Guid optionId)
    {
        const ulong offset = 14695981039346656037;
        const ulong prime = 1099511628211;
        var hash = offset;
        foreach (var b in attemptId.ToByteArray())
            hash = (hash ^ b) * prime;
        foreach (var b in optionId.ToByteArray())
            hash = (hash ^ b) * prime;
        return unchecked((long)hash);
    }

    private readonly record struct AssignmentInfo(
        Guid PublicId,
        string Title,
        decimal? TotalPoint,
        bool ShuffleOptions,
        bool RequireFullscreen,
        bool DetectTabSwitch,
        bool BlockCopyPaste,
        int MaxViolations,
        string ViolationAction
    );

    private readonly record struct QuestionRow(
        long Id,
        Guid PublicId,
        string Type,
        string Content,
        decimal Point
    );

    private readonly record struct OptionRow(
        long SnapshotQuestionId,
        long Id,
        Guid PublicId,
        string Content,
        int DisplayOrder
    );

    private readonly record struct MediaRow(
        long SnapshotQuestionId,
        long? SnapshotOptionId,
        Guid MediaPublicId,
        string Url,
        string Kind,
        string Role,
        int DisplayOrder
    );
}
