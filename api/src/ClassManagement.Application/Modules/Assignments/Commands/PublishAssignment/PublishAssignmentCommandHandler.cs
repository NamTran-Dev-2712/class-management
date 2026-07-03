using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Admin.Enums;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Classroom.Enums;
using ClassManagement.Domain.Modules.Questions.Enums;

public class PublishAssignmentCommandHandler : IRequestHandler<PublishAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notifications;

    public PublishAssignmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger,
        INotificationService notifications
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
        _notifications = notifications;
    }

    public async Task Handle(PublishAssignmentCommand request, CancellationToken ct)
    {
        var assignment = AssignmentGuard.EnsureOwned(
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // Idempotency: only a Draft can be published (BR-5-02 — the snapshot is built exactly once).
        if (assignment.Status != AssignmentStatus.Draft)
            throw new BadException("Assignment.AlreadyPublished");

        // Load the exam's ordered question list.
        var examQuestionRepo = _unitOfWork.Repository<ExamQuestion>();
        var examQuestions = await examQuestionRepo.ToListAsync(
            examQuestionRepo
                .Query()
                .Where(eq => eq.ExamId == assignment.ExamId)
                .OrderBy(eq => eq.DisplayOrder)
                .Select(eq => new ExamQuestionRef(eq.QuestionId, eq.DisplayOrder, eq.Point)),
            ct
        );

        if (examQuestions.Count == 0)
            throw new BadException("Assignment.ExamHasNoQuestions");

        var questionIds = examQuestions.Select(eq => eq.QuestionId).Distinct().ToList();

        // Load question bodies (soft-deleted questions are excluded by the global filter — a missing
        // reference means the teacher must fix the exam before publishing).
        var questionRepo = _unitOfWork.Repository<Question>();
        var questions = await questionRepo.ToListAsync(
            questionRepo
                .Query()
                .Where(q => questionIds.Contains(q.Id))
                .Select(q => new QuestionRef(q.Id, q.Type, q.Content, q.Explanation)),
            ct
        );
        var questionById = questions.ToDictionary(q => q.Id);

        if (questionById.Count != questionIds.Count)
            throw new BadException("Assignment.ExamQuestionUnavailable");

        // Load options for choice questions.
        var optionRepo = _unitOfWork.Repository<QuestionOption>();
        var options = await optionRepo.ToListAsync(
            optionRepo
                .Query()
                .Where(o => questionIds.Contains(o.QuestionId))
                .OrderBy(o => o.QuestionId)
                .ThenBy(o => o.DisplayOrder)
                .Select(o => new OptionRef(
                    o.QuestionId,
                    o.Content,
                    o.IsCorrect,
                    o.DisplayOrder,
                    o.MediaId
                )),
            ct
        );
        var optionsByQuestion = options
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Question-level media attachments to freeze (MVP-9).
        var questionMediaRepo = _unitOfWork.Repository<QuestionMedia>();
        var questionMedia = await questionMediaRepo.ToListAsync(
            questionMediaRepo
                .Query()
                .Where(m => questionIds.Contains(m.QuestionId))
                .Select(m => new QuestionMediaRef(m.QuestionId, m.MediaId, m.Role, m.DisplayOrder)),
            ct
        );
        var questionMediaByQuestion = questionMedia
            .GroupBy(m => m.QuestionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(m => m.DisplayOrder).ToList());

        // Resolve the media assets referenced by attachments + option images (by internal id) and any
        // inline media in content/explanation (by public id), so we can freeze public_id + URL + kind.
        var explicitMediaIds = questionMedia
            .Select(m => m.MediaId)
            .Concat(options.Where(o => o.MediaId.HasValue).Select(o => o.MediaId!.Value))
            .Distinct()
            .ToList();

        var assetRepo = _unitOfWork.Repository<MediaAsset>();
        var assetsById =
            explicitMediaIds.Count == 0
                ? new Dictionary<long, MediaAsset>()
                : (
                    await assetRepo.ToListAsync(
                        assetRepo.Query().Where(a => explicitMediaIds.Contains(a.Id)),
                        ct
                    )
                ).ToDictionary(a => a.Id);

        var inlinePublicIds = MediaReferenceParser.ExtractPublicIds([
            .. questions.Select(q => q.Content),
            .. questions.Select(q => q.Explanation),
        ]);
        var assetsByPublicId =
            inlinePublicIds.Count == 0
                ? new Dictionary<Guid, MediaAsset>()
                : (
                    await assetRepo.ToListAsync(
                        assetRepo.Query().Where(a => inlinePublicIds.Contains(a.PublicId)),
                        ct
                    )
                ).ToDictionary(a => a.PublicId);

        var now = DateTime.UtcNow;

        var snapshot = new AssignmentSnapshot
        {
            TotalQuestions = examQuestions.Count,
            TotalPoint = examQuestions.Sum(eq => eq.Point),
            SnapshotCreatedAt = now,
        };

        // Track the built rows so we can freeze their media once the snapshot ids are generated (MVP-9).
        var builtQuestions =
            new List<(
                SnapshotQuestion Sq,
                string Content,
                string? Explanation,
                List<(SnapshotOption So, long? MediaId)> Options
            )>();

        var displayOrder = 1;
        foreach (var eq in examQuestions)
        {
            var q = questionById[eq.QuestionId];
            var snapshotQuestion = new SnapshotQuestion
            {
                OriginalQuestionId = q.Id,
                Type = q.Type,
                Content = q.Content,
                Point = eq.Point,
                DisplayOrder = displayOrder++,
                Explanation = q.Explanation,
                SnapshotCreatedAt = now,
            };

            var builtOptions = new List<(SnapshotOption, long?)>();
            if (optionsByQuestion.TryGetValue(eq.QuestionId, out var qOptions))
            {
                foreach (var opt in qOptions)
                {
                    var snapshotOption = new SnapshotOption
                    {
                        Content = opt.Content,
                        IsCorrect = opt.IsCorrect,
                        DisplayOrder = opt.DisplayOrder,
                        SnapshotCreatedAt = now,
                    };
                    snapshotQuestion.Options.Add(snapshotOption);
                    builtOptions.Add((snapshotOption, opt.MediaId));
                }
            }

            snapshot.Questions.Add(snapshotQuestion);
            builtQuestions.Add((snapshotQuestion, q.Content, q.Explanation, builtOptions));
        }

        assignment.Snapshot = snapshot;
        assignment.ExamVersionAtPublish = await GetExamVersionAsync(assignment.ExamId, ct);
        assignment.PublishedAt = now;
        assignment.Status =
            assignment.OpensAt is DateTime opensAt && opensAt > now
                ? AssignmentStatus.Scheduled
                : AssignmentStatus.Open;

        // Fan out "new assignment" to every approved student of the class.
        var memberRepo = _unitOfWork.Repository<ClassMembership>();
        var studentIds = await memberRepo.ToListAsync(
            memberRepo
                .Query()
                .Where(m =>
                    m.ClassId == assignment.ClassId && m.Status == MembershipStatus.Approved
                )
                .Select(m => m.StudentId),
            ct
        );

        // Two-phase, one transaction: (1) persist the snapshot so questions/options get ids, then
        // (2) freeze their media references + stage audit/notifications and commit together (BR-9-06).
        await _unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                _unitOfWork.Assignments.Update(assignment);
                await _unitOfWork.SaveChangesAsync(token);

                var snapshotMedia = BuildSnapshotMedia(
                    builtQuestions,
                    questionMediaByQuestion,
                    assetsById,
                    assetsByPublicId,
                    now
                );
                if (snapshotMedia.Count > 0)
                    await _unitOfWork
                        .Repository<SnapshotMedia>()
                        .AddRangeAsync(snapshotMedia, token);

                await _auditLogger.LogAsync(
                    new AuditEntry
                    {
                        Action = AuditActions.AssignmentPublished,
                        TargetType = AuditTargetTypes.Assignment,
                        TargetId = assignment.Id,
                        TargetPublicId = assignment.PublicId,
                        Metadata = new Dictionary<string, object?>
                        {
                            ["status"] = assignment.Status.ToString(),
                            ["total_questions"] = snapshot.TotalQuestions,
                        },
                    },
                    token
                );

                await _notifications.NotifyManyAsync(
                    studentIds,
                    new NotificationContent
                    {
                        EventType = NotificationEventType.AssignmentCreated,
                        Title = "New assignment",
                        Body = $"A new assignment \"{assignment.Title}\" is available.",
                        Link = $"/student/assignments/{assignment.PublicId}",
                        ReferenceId = assignment.PublicId.ToString(),
                        Payload = new Dictionary<string, object?>
                        {
                            ["assignmentTitle"] = assignment.Title,
                            ["assignmentPublicId"] = assignment.PublicId.ToString(),
                        },
                    },
                    token
                );

                await _unitOfWork.SaveChangesAsync(token);
            },
            ct
        );
    }

    // Builds the frozen media rows for a published snapshot (MVP-9): question-level attachments,
    // per-option images, and inline media parsed from content/explanation. Runs after the snapshot save
    // so SnapshotQuestion/SnapshotOption ids are available.
    private static List<SnapshotMedia> BuildSnapshotMedia(
        List<(
            SnapshotQuestion Sq,
            string Content,
            string? Explanation,
            List<(SnapshotOption So, long? MediaId)> Options
        )> builtQuestions,
        Dictionary<long, List<QuestionMediaRef>> questionMediaByQuestion,
        Dictionary<long, MediaAsset> assetsById,
        Dictionary<Guid, MediaAsset> assetsByPublicId,
        DateTime now
    )
    {
        var rows = new List<SnapshotMedia>();

        foreach (var (sq, content, explanation, builtOptions) in builtQuestions)
        {
            // Question-level attachments.
            if (
                sq.OriginalQuestionId is long qid
                && questionMediaByQuestion.TryGetValue(qid, out var attachments)
            )
            {
                foreach (var a in attachments)
                {
                    if (!assetsById.TryGetValue(a.MediaId, out var asset))
                        continue;
                    rows.Add(Freeze(sq.Id, null, asset, a.Role, a.DisplayOrder, now));
                }
            }

            // Per-option images.
            foreach (var (so, mediaId) in builtOptions)
            {
                if (mediaId is not long mid || !assetsById.TryGetValue(mid, out var asset))
                    continue;
                rows.Add(Freeze(sq.Id, so.Id, asset, MediaRole.Attachment, so.DisplayOrder, now));
            }

            // Inline media in content/explanation.
            var order = 0;
            foreach (var pid in MediaReferenceParser.ExtractPublicIds(content, explanation))
            {
                if (!assetsByPublicId.TryGetValue(pid, out var asset))
                    continue;
                rows.Add(Freeze(sq.Id, null, asset, MediaRole.Inline, order++, now));
            }
        }

        return rows;
    }

    private static SnapshotMedia Freeze(
        long snapshotQuestionId,
        long? snapshotOptionId,
        MediaAsset asset,
        MediaRole role,
        int displayOrder,
        DateTime now
    ) =>
        new()
        {
            SnapshotQuestionId = snapshotQuestionId,
            SnapshotOptionId = snapshotOptionId,
            MediaPublicId = asset.PublicId,
            FrozenUrl = asset.Url,
            Kind = asset.Kind,
            Role = role,
            DisplayOrder = displayOrder,
            SnapshotCreatedAt = now,
        };

    private async Task<int?> GetExamVersionAsync(long examId, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Exam>();
        var rows = await repo.ToListAsync(
            repo.Query().Where(e => e.Id == examId).Select(e => e.Version).Take(1),
            ct
        );
        return rows.FirstOrDefault();
    }

    private readonly record struct ExamQuestionRef(
        long QuestionId,
        int DisplayOrder,
        decimal Point
    );

    private readonly record struct QuestionRef(
        long Id,
        QuestionType Type,
        string Content,
        string? Explanation
    );

    private readonly record struct OptionRef(
        long QuestionId,
        string Content,
        bool IsCorrect,
        int DisplayOrder,
        long? MediaId
    );

    private readonly record struct QuestionMediaRef(
        long QuestionId,
        long MediaId,
        MediaRole Role,
        int DisplayOrder
    );
}
