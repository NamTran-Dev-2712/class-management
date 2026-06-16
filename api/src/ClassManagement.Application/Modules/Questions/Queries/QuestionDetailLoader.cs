using ClassManagement.Application.Modules.Questions.DTOs;

// Shared loader for single-question detail reads. Materializes the display row from the read-model
// view (AsNoTracking) + its answer options, then maps to the detail DTO. Each detail handler runs
// its own access guard on the view before building (owner / public / admin).
internal static class QuestionDetailLoader
{
    public static async Task<QuestionView?> LoadViewAsync(
        IUnitOfWork unitOfWork,
        Guid publicId,
        CancellationToken ct
    )
    {
        var repo = unitOfWork.Repository<QuestionView>();
        var rows = await repo.ToListAsync(
            repo.Query().Where(q => q.PublicId == publicId).Take(1),
            ct
        );
        return rows.FirstOrDefault();
    }

    public static async Task<QuestionDetailDto> BuildAsync(
        IUnitOfWork unitOfWork,
        QuestionView view,
        CancellationToken ct
    )
    {
        var optionRepo = unitOfWork.Repository<QuestionOption>();
        var options = await optionRepo.ToListAsync(
            optionRepo.Query().Where(o => o.QuestionId == view.Id).OrderBy(o => o.DisplayOrder),
            ct
        );

        return new QuestionDetailDto
        {
            PublicId = view.PublicId,
            Type = view.Type,
            Content = view.Content,
            Difficulty = view.Difficulty,
            SuggestedPoint = view.SuggestedPoint,
            Visibility = view.Visibility,
            Explanation = view.Explanation,
            SubjectPublicId = view.SubjectPublicId,
            SubjectName = view.SubjectName,
            TeacherPublicId = view.TeacherPublicId,
            TeacherName = view.TeacherName,
            Tags = view.Tags,
            Options =
            [
                .. options.Select(o => new QuestionOptionDto
                {
                    Content = o.Content,
                    IsCorrect = o.IsCorrect,
                    DisplayOrder = o.DisplayOrder,
                }),
            ],
            CreatedAt = view.CreatedAt,
            UpdatedAt = view.UpdatedAt,
        };
    }
}
