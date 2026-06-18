using ClassManagement.Application.Modules.Exams.DTOs;

// Shared loader for single-exam reads. Materializes the display row from the read-model view
// (AsNoTracking), then loads its ordered exam_questions and joins each to the referenced question's
// display info from vw_questions. A question that was soft-deleted (e.g. a Public question removed by
// its owner) is absent from vw_questions → marked IsAvailable = false so the teacher can replace it
// before publishing. Each detail handler runs its own access guard on the view before building.
internal static class ExamDetailLoader
{
    public static async Task<ExamView?> LoadViewAsync(
        IUnitOfWork unitOfWork,
        Guid publicId,
        CancellationToken ct
    )
    {
        var repo = unitOfWork.Repository<ExamView>();
        var rows = await repo.ToListAsync(
            repo.Query().Where(e => e.PublicId == publicId).Take(1),
            ct
        );
        return rows.FirstOrDefault();
    }

    public static async Task<ExamDetailDto> BuildDetailAsync(
        IUnitOfWork unitOfWork,
        ExamView view,
        CancellationToken ct
    )
    {
        var items = await LoadOrderedItemsAsync(unitOfWork, view.Id, ct);
        var questionInfo = await LoadQuestionInfoAsync(unitOfWork, items, ct);

        var questions = items.Select(eq =>
        {
            if (questionInfo.TryGetValue(eq.QuestionId, out var info))
                return new ExamQuestionDto
                {
                    QuestionPublicId = info.PublicId,
                    DisplayOrder = eq.DisplayOrder,
                    Point = eq.Point,
                    IsAvailable = true,
                    Type = info.Type,
                    Content = info.Content,
                    Difficulty = info.Difficulty,
                    OptionCount = info.OptionCount,
                    SubjectPublicId = info.SubjectPublicId,
                    SubjectName = info.SubjectName,
                };

            return new ExamQuestionDto
            {
                QuestionPublicId = Guid.Empty,
                DisplayOrder = eq.DisplayOrder,
                Point = eq.Point,
                IsAvailable = false,
            };
        });

        return new ExamDetailDto
        {
            PublicId = view.PublicId,
            Title = view.Title,
            Description = view.Description,
            Visibility = view.Visibility,
            Version = view.Version,
            TotalPoint = view.TotalPoint,
            TotalQuestions = view.TotalQuestions,
            SubjectPublicId = view.SubjectPublicId,
            SubjectName = view.SubjectName,
            TeacherPublicId = view.TeacherPublicId,
            TeacherName = view.TeacherName,
            Tags = view.Tags,
            Questions = [.. questions],
            CreatedAt = view.CreatedAt,
            UpdatedAt = view.UpdatedAt,
        };
    }

    public static async Task<ExamPreviewDto> BuildPreviewAsync(
        IUnitOfWork unitOfWork,
        ExamView view,
        CancellationToken ct
    )
    {
        var items = await LoadOrderedItemsAsync(unitOfWork, view.Id, ct);
        var questionInfo = await LoadQuestionInfoAsync(unitOfWork, items, ct);

        // Load the answer options for the available questions (no IsCorrect leak in the preview DTO).
        var availableIds = items
            .Where(eq => questionInfo.ContainsKey(eq.QuestionId))
            .Select(eq => eq.QuestionId)
            .ToList();

        var optionRepo = unitOfWork.Repository<QuestionOption>();
        var options = await optionRepo.ToListAsync(
            optionRepo
                .Query()
                .Where(o => availableIds.Contains(o.QuestionId))
                .OrderBy(o => o.QuestionId)
                .ThenBy(o => o.DisplayOrder)
                .Select(o => new OptionRef(o.QuestionId, o.Content, o.DisplayOrder)),
            ct
        );
        var optionsByQuestion = options
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var questions = items.Select(eq =>
        {
            if (!questionInfo.TryGetValue(eq.QuestionId, out var info))
                return new ExamPreviewQuestionDto
                {
                    DisplayOrder = eq.DisplayOrder,
                    Point = eq.Point,
                    IsAvailable = false,
                };

            var opts = optionsByQuestion.TryGetValue(eq.QuestionId, out var list) ? list : [];

            return new ExamPreviewQuestionDto
            {
                DisplayOrder = eq.DisplayOrder,
                Point = eq.Point,
                IsAvailable = true,
                Type = info.Type,
                Content = info.Content,
                Difficulty = info.Difficulty,
                Options =
                [
                    .. opts.Select(o => new ExamPreviewOptionDto
                    {
                        Content = o.Content,
                        DisplayOrder = o.DisplayOrder,
                    }),
                ],
            };
        });

        return new ExamPreviewDto
        {
            PublicId = view.PublicId,
            Title = view.Title,
            Description = view.Description,
            TotalPoint = view.TotalPoint,
            TotalQuestions = view.TotalQuestions,
            Questions = [.. questions],
        };
    }

    private static async Task<List<ExamQuestion>> LoadOrderedItemsAsync(
        IUnitOfWork unitOfWork,
        long examId,
        CancellationToken ct
    )
    {
        var repo = unitOfWork.Repository<ExamQuestion>();
        return await repo.ToListAsync(
            repo.Query().Where(eq => eq.ExamId == examId).OrderBy(eq => eq.DisplayOrder),
            ct
        );
    }

    private static async Task<Dictionary<long, QuestionInfo>> LoadQuestionInfoAsync(
        IUnitOfWork unitOfWork,
        List<ExamQuestion> items,
        CancellationToken ct
    )
    {
        var questionIds = items.Select(eq => eq.QuestionId).Distinct().ToList();
        if (questionIds.Count == 0)
            return [];

        var repo = unitOfWork.Repository<QuestionView>();
        var rows = await repo.ToListAsync(
            repo.Query()
                .Where(q => questionIds.Contains(q.Id))
                .Select(q => new QuestionInfo(
                    q.Id,
                    q.PublicId,
                    q.Type,
                    q.Content,
                    q.Difficulty,
                    q.OptionCount,
                    q.SubjectPublicId,
                    q.SubjectName
                )),
            ct
        );
        return rows.ToDictionary(q => q.Id);
    }

    private readonly record struct QuestionInfo(
        long Id,
        Guid PublicId,
        string Type,
        string Content,
        string Difficulty,
        int OptionCount,
        Guid? SubjectPublicId,
        string? SubjectName
    );

    private readonly record struct OptionRef(long QuestionId, string Content, int DisplayOrder);
}
