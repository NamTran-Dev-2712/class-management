using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Questions.Enums;

public class GradeAttemptCommandHandler : IRequestHandler<GradeAttemptCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GradeAttemptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(GradeAttemptCommand request, CancellationToken ct)
    {
        var attempt =
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct)
            ?? throw new NotFoundException("Attempt.NotFound");

        // The teacher must own the assignment this attempt belongs to.
        var avRepo = _unitOfWork.Repository<AssignmentView>();
        var ownerId = (
            await avRepo.ToListAsync(
                avRepo
                    .Query()
                    .Where(a => a.Id == attempt.AssignmentId)
                    .Select(a => a.TeacherId)
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        if (ownerId == 0 || ownerId != _currentUser.UserId)
            throw new ForbiddenException("Assignment.NotOwner");

        // Can only grade a submitted attempt (writing questions are pending or already graded).
        if (attempt.Status == AttemptStatus.InProgress)
            throw new BadException("Grade.AttemptNotSubmitted");

        // Resolve the attempt's snapshot writing questions (by public id), with their max points.
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

        if (snapshotId == 0)
            throw new BadException("Assignment.SnapshotMissing");

        var sqRepo = _unitOfWork.Repository<SnapshotQuestion>();
        var writingQuestions = await sqRepo.ToListAsync(
            sqRepo
                .Query()
                .Where(q =>
                    q.SnapshotId == snapshotId
                    && (q.Type == QuestionType.ShortWriting || q.Type == QuestionType.LongWriting)
                )
                .Select(q => new WritingQuestionRef(q.Id, q.PublicId, q.Point)),
            ct
        );

        if (writingQuestions.Count == 0)
            throw new BadException("Grade.NoWritingQuestions");

        var byPublicId = writingQuestions.ToDictionary(q => q.PublicId);

        // Validate every grade item references a writing question of this snapshot and is in range.
        foreach (var item in request.Grades)
        {
            if (!byPublicId.TryGetValue(item.QuestionPublicId, out var q))
                throw new BadException("Grade.QuestionNotGradable");

            if (item.Score < 0m || item.Score > q.Point)
                throw new BadException("Grade.ScoreOutOfRange");
        }

        // Upsert one manual_grade per (attempt, snapshot question).
        var now = DateTime.UtcNow;
        var existing = (await _unitOfWork.ManualGrades.GetByAttemptAsync(attempt.Id, ct)).ToList();
        var existingByQuestion = existing.ToDictionary(g => g.SnapshotQuestionId);

        foreach (var item in request.Grades)
        {
            var snapshotQuestionId = byPublicId[item.QuestionPublicId].Id;
            if (existingByQuestion.TryGetValue(snapshotQuestionId, out var grade))
            {
                grade.Score = item.Score;
                grade.Feedback = item.Feedback;
                _unitOfWork.ManualGrades.Update(grade);
            }
            else
            {
                grade = new ManualGrade
                {
                    AttemptId = attempt.Id,
                    SnapshotQuestionId = snapshotQuestionId,
                    Score = item.Score,
                    Feedback = item.Feedback,
                };
                await _unitOfWork.ManualGrades.AddAsync(grade, ct);
                existing.Add(grade);
                existingByQuestion[snapshotQuestionId] = grade;
            }
        }

        var writingQuestionIds = writingQuestions.Select(q => q.Id).ToList();
        ManualGradeFinalizer.Recompute(_unitOfWork, attempt, writingQuestionIds, existing);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private readonly record struct WritingQuestionRef(long Id, Guid PublicId, decimal Point);
}
