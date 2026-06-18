using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Questions.Enums;

public class PublishAssignmentCommandHandler : IRequestHandler<PublishAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PublishAssignmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
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
                .Select(o => new OptionRef(o.QuestionId, o.Content, o.IsCorrect, o.DisplayOrder)),
            ct
        );
        var optionsByQuestion = options
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var now = DateTime.UtcNow;

        var snapshot = new AssignmentSnapshot
        {
            TotalQuestions = examQuestions.Count,
            TotalPoint = examQuestions.Sum(eq => eq.Point),
            SnapshotCreatedAt = now,
        };

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

            if (optionsByQuestion.TryGetValue(eq.QuestionId, out var qOptions))
            {
                foreach (var opt in qOptions)
                {
                    snapshotQuestion.Options.Add(
                        new SnapshotOption
                        {
                            Content = opt.Content,
                            IsCorrect = opt.IsCorrect,
                            DisplayOrder = opt.DisplayOrder,
                            SnapshotCreatedAt = now,
                        }
                    );
                }
            }

            snapshot.Questions.Add(snapshotQuestion);
        }

        assignment.Snapshot = snapshot;
        assignment.ExamVersionAtPublish = await GetExamVersionAsync(assignment.ExamId, ct);
        assignment.PublishedAt = now;
        assignment.Status =
            assignment.OpensAt is DateTime opensAt && opensAt > now
                ? AssignmentStatus.Scheduled
                : AssignmentStatus.Open;

        _unitOfWork.Assignments.Update(assignment);

        // One atomic SaveChanges — snapshot + questions + options + the status flip commit together.
        await _unitOfWork.SaveChangesAsync(ct);
    }

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
        int DisplayOrder
    );
}
