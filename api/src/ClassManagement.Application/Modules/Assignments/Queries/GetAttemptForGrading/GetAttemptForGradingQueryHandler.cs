using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

public class GetAttemptForGradingQueryHandler
    : IRequestHandler<GetAttemptForGradingQuery, AttemptGradingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetAttemptForGradingQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AttemptGradingDto> Handle(
        GetAttemptForGradingQuery request,
        CancellationToken ct
    )
    {
        var attempt =
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct)
            ?? throw new NotFoundException("Attempt.NotFound");

        // Owner guard: the teacher must own the assignment this attempt belongs to.
        var avRepo = _unitOfWork.Repository<AssignmentView>();
        var assignmentInfo = (
            await avRepo.ToListAsync(
                avRepo
                    .Query()
                    .Where(a => a.Id == attempt.AssignmentId)
                    .Select(a => new
                    {
                        a.TeacherId,
                        a.PublicId,
                        a.Title,
                        a.TotalPoint,
                    })
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        if (assignmentInfo is null || assignmentInfo.TeacherId != _currentUser.UserId)
            throw new ForbiddenException("Assignment.NotOwner");

        // Student display name from the attempt view.
        var atvRepo = _unitOfWork.Repository<AttemptView>();
        var student = (
            await atvRepo.ToListAsync(
                atvRepo
                    .Query()
                    .Where(a => a.Id == attempt.Id)
                    .Select(a => new { a.StudentPublicId, a.StudentName })
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        // Snapshot questions (canonical display order) + options.
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
        var questions = await sqRepo.ToListAsync(
            sqRepo
                .Query()
                .Where(q => q.SnapshotId == snapshotId)
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new QuestionRow(
                    q.Id,
                    q.PublicId,
                    q.Type,
                    q.Content,
                    q.Point,
                    q.DisplayOrder
                )),
            ct
        );

        var questionIds = questions.Select(q => q.Id).ToList();
        var soRepo = _unitOfWork.Repository<SnapshotOption>();
        var options = await soRepo.ToListAsync(
            soRepo
                .Query()
                .Where(o => questionIds.Contains(o.SnapshotQuestionId))
                .OrderBy(o => o.SnapshotQuestionId)
                .ThenBy(o => o.DisplayOrder)
                .Select(o => new OptionRow(
                    o.SnapshotQuestionId,
                    o.Id,
                    o.PublicId,
                    o.Content,
                    o.IsCorrect
                )),
            ct
        );
        var optionsByQuestion = options
            .GroupBy(o => o.SnapshotQuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var answerByQuestion = attempt.Answers.ToDictionary(a => a.SnapshotQuestionId);
        var grades = await _unitOfWork.ManualGrades.GetByAttemptAsync(attempt.Id, ct);
        var gradeByQuestion = grades.ToDictionary(g => g.SnapshotQuestionId);

        var position = 1;
        var questionDtos = new List<GradingQuestionDto>(questions.Count);
        foreach (var q in questions)
        {
            var isWriting =
                q.Type == QuestionType.ShortWriting || q.Type == QuestionType.LongWriting;
            answerByQuestion.TryGetValue(q.Id, out var answer);
            var selected = answer?.SelectedOptionIds ?? [];

            IReadOnlyList<GradingOptionDto> opts = isWriting
                ? []
                :
                [
                    .. (optionsByQuestion.TryGetValue(q.Id, out var list) ? list : []).Select(
                        o => new GradingOptionDto
                        {
                            PublicId = o.PublicId,
                            Content = o.Content,
                            IsCorrect = o.IsCorrect,
                            IsSelected = selected.Contains(o.Id),
                        }
                    ),
                ];

            gradeByQuestion.TryGetValue(q.Id, out var grade);

            questionDtos.Add(
                new GradingQuestionDto
                {
                    QuestionPublicId = q.PublicId,
                    DisplayPosition = position++,
                    Type = q.Type.ToString(),
                    Content = q.Content,
                    Point = q.Point,
                    IsWriting = isWriting,
                    AutoScore = isWriting ? null : answer?.AutoScore,
                    Options = opts,
                    TextAnswer = isWriting ? answer?.TextAnswer : null,
                    ManualScore = isWriting ? grade?.Score : null,
                    Feedback = isWriting ? grade?.Feedback : null,
                }
            );
        }

        return new AttemptGradingDto
        {
            PublicId = attempt.PublicId,
            AssignmentPublicId = assignmentInfo.PublicId,
            AssignmentTitle = assignmentInfo.Title,
            StudentPublicId = student?.StudentPublicId ?? Guid.Empty,
            StudentName = student?.StudentName ?? string.Empty,
            AttemptNumber = attempt.AttemptNumber,
            Status = attempt.Status.ToString(),
            SubmittedAt = attempt.SubmittedAt,
            AutoSubmitted = attempt.AutoSubmitted,
            TotalPoint = assignmentInfo.TotalPoint,
            TotalAutoScore = attempt.TotalAutoScore,
            TotalManualScore = attempt.TotalManualScore,
            TotalScore = attempt.TotalScore,
            Questions = questionDtos,
        };
    }

    private readonly record struct QuestionRow(
        long Id,
        Guid PublicId,
        QuestionType Type,
        string Content,
        decimal Point,
        int DisplayOrder
    );

    private readonly record struct OptionRow(
        long SnapshotQuestionId,
        long Id,
        Guid PublicId,
        string Content,
        bool IsCorrect
    );
}
