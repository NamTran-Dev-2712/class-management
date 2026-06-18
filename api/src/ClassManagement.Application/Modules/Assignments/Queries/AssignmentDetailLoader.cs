using ClassManagement.Application.Modules.Assignments.DTOs;

// Shared loader for single-assignment reads. Materializes the display row from vw_assignments, then
// (once published) loads the frozen snapshot questions + options. Detail includes correct flags
// (teacher/admin); preview strips them (student-eye). Each caller runs its own access guard first.
internal static class AssignmentDetailLoader
{
    public static async Task<AssignmentView?> LoadViewAsync(
        IUnitOfWork unitOfWork,
        Guid publicId,
        CancellationToken ct
    )
    {
        var repo = unitOfWork.Repository<AssignmentView>();
        var rows = await repo.ToListAsync(
            repo.Query().Where(a => a.PublicId == publicId).Take(1),
            ct
        );
        return rows.FirstOrDefault();
    }

    public static async Task<AssignmentDetailDto> BuildDetailAsync(
        IUnitOfWork unitOfWork,
        AssignmentView view,
        CancellationToken ct
    )
    {
        var questions = await LoadQuestionsAsync(unitOfWork, view.Id, ct);

        return new AssignmentDetailDto
        {
            PublicId = view.PublicId,
            Title = view.Title,
            Description = view.Description,
            Status = view.Status,
            OpensAt = view.OpensAt,
            ClosesAt = view.ClosesAt,
            TimeLimitMinutes = view.TimeLimitMinutes,
            MaxAttempts = view.MaxAttempts,
            ScorePolicy = view.ScorePolicy,
            AllowLate = view.AllowLate,
            GradePublishPolicy = view.GradePublishPolicy,
            ShuffleQuestions = view.ShuffleQuestions,
            ShuffleOptions = view.ShuffleOptions,
            ShowAnswersAfterGrade = view.ShowAnswersAfterGrade,
            PublishedAt = view.PublishedAt,
            ClosedAt = view.ClosedAt,
            ExamVersionAtPublish = view.ExamVersionAtPublish,
            ExamPublicId = view.ExamPublicId,
            ExamTitle = view.ExamTitle,
            ClassPublicId = view.ClassPublicId,
            ClassName = view.ClassName,
            TeacherPublicId = view.TeacherPublicId,
            TeacherName = view.TeacherName,
            TotalPoint = view.TotalPoint,
            TotalQuestions = view.TotalQuestions,
            AttemptCount = view.AttemptCount,
            SubmittedCount = view.SubmittedCount,
            CreatedAt = view.CreatedAt,
            UpdatedAt = view.UpdatedAt,
            Questions =
            [
                .. questions.Select(q => new AssignmentSnapshotQuestionDto
                {
                    PublicId = q.Question.PublicId,
                    Type = q.Question.Type,
                    Content = q.Question.Content,
                    Point = q.Question.Point,
                    DisplayOrder = q.Question.DisplayOrder,
                    Explanation = q.Question.Explanation,
                    Options =
                    [
                        .. q.Options.Select(o => new AssignmentSnapshotOptionDto
                        {
                            PublicId = o.PublicId,
                            Content = o.Content,
                            IsCorrect = o.IsCorrect,
                            DisplayOrder = o.DisplayOrder,
                        }),
                    ],
                }),
            ],
        };
    }

    public static async Task<AssignmentPreviewDto> BuildPreviewAsync(
        IUnitOfWork unitOfWork,
        AssignmentView view,
        CancellationToken ct
    )
    {
        var questions = await LoadQuestionsAsync(unitOfWork, view.Id, ct);

        return new AssignmentPreviewDto
        {
            PublicId = view.PublicId,
            Title = view.Title,
            Description = view.Description,
            TotalPoint = view.TotalPoint,
            TotalQuestions = view.TotalQuestions,
            Questions =
            [
                .. questions.Select(q => new AssignmentPreviewQuestionDto
                {
                    PublicId = q.Question.PublicId,
                    Type = q.Question.Type,
                    Content = q.Question.Content,
                    Point = q.Question.Point,
                    DisplayOrder = q.Question.DisplayOrder,
                    Options =
                    [
                        .. q.Options.Select(o => new AssignmentPreviewOptionDto
                        {
                            PublicId = o.PublicId,
                            Content = o.Content,
                            DisplayOrder = o.DisplayOrder,
                        }),
                    ],
                }),
            ],
        };
    }

    // Loads the snapshot questions (ordered) + their options for a published assignment. Empty for a
    // Draft (no snapshot yet).
    private static async Task<List<QuestionWithOptions>> LoadQuestionsAsync(
        IUnitOfWork unitOfWork,
        long assignmentId,
        CancellationToken ct
    )
    {
        var snapshotRepo = unitOfWork.Repository<AssignmentSnapshot>();
        var snapshotId = (
            await snapshotRepo.ToListAsync(
                snapshotRepo
                    .Query()
                    .Where(s => s.AssignmentId == assignmentId)
                    .Select(s => s.Id)
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        if (snapshotId == 0)
            return [];

        var sqRepo = unitOfWork.Repository<SnapshotQuestion>();
        var questions = await sqRepo.ToListAsync(
            sqRepo
                .Query()
                .Where(q => q.SnapshotId == snapshotId)
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new SnapshotQuestionRow(
                    q.Id,
                    q.PublicId,
                    q.Type.ToString(),
                    q.Content,
                    q.Point,
                    q.DisplayOrder,
                    q.Explanation
                )),
            ct
        );

        var questionIds = questions.Select(q => q.Id).ToList();
        var soRepo = unitOfWork.Repository<SnapshotOption>();
        var options = await soRepo.ToListAsync(
            soRepo
                .Query()
                .Where(o => questionIds.Contains(o.SnapshotQuestionId))
                .OrderBy(o => o.SnapshotQuestionId)
                .ThenBy(o => o.DisplayOrder)
                .Select(o => new SnapshotOptionRow(
                    o.SnapshotQuestionId,
                    o.PublicId,
                    o.Content,
                    o.IsCorrect,
                    o.DisplayOrder
                )),
            ct
        );
        var optionsByQuestion = options
            .GroupBy(o => o.SnapshotQuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return questions
            .Select(q => new QuestionWithOptions(
                q,
                optionsByQuestion.TryGetValue(q.Id, out var list) ? list : []
            ))
            .ToList();
    }

    private sealed record QuestionWithOptions(
        SnapshotQuestionRow Question,
        List<SnapshotOptionRow> Options
    );

    internal sealed record SnapshotQuestionRow(
        long Id,
        Guid PublicId,
        string Type,
        string Content,
        decimal Point,
        int DisplayOrder,
        string? Explanation
    );

    internal sealed record SnapshotOptionRow(
        long SnapshotQuestionId,
        Guid PublicId,
        string Content,
        bool IsCorrect,
        int DisplayOrder
    );
}
