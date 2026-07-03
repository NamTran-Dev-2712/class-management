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

        // Question-level media attachments (MVP-9).
        var mediaRepo = unitOfWork.Repository<QuestionMedia>();
        var attachments = await mediaRepo.ToListAsync(
            mediaRepo.Query().Where(m => m.QuestionId == view.Id).OrderBy(m => m.DisplayOrder),
            ct
        );

        // Resolve all referenced media (attachments + option images) to public id / URL / kind in one
        // pass. A soft-deleted asset drops out of the map → its reference simply renders empty.
        var mediaIds = attachments
            .Select(a => a.MediaId)
            .Concat(options.Where(o => o.MediaId.HasValue).Select(o => o.MediaId!.Value))
            .Distinct()
            .ToList();

        var mediaMap = new Dictionary<long, MediaAsset>();
        if (mediaIds.Count > 0)
        {
            var assetRepo = unitOfWork.Repository<MediaAsset>();
            var assets = await assetRepo.ToListAsync(
                assetRepo.Query().Where(a => mediaIds.Contains(a.Id)),
                ct
            );
            mediaMap = assets.ToDictionary(a => a.Id);
        }

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
                    MediaPublicId = LookupPublicId(o.MediaId, mediaMap),
                    MediaUrl = Lookup(o.MediaId, mediaMap)?.Url,
                    MediaKind = Lookup(o.MediaId, mediaMap)?.Kind.ToString(),
                }),
            ],
            Media =
            [
                .. attachments
                    .Where(a => mediaMap.ContainsKey(a.MediaId))
                    .Select(a => new QuestionMediaDto
                    {
                        MediaPublicId = mediaMap[a.MediaId].PublicId,
                        Url = mediaMap[a.MediaId].Url,
                        Kind = mediaMap[a.MediaId].Kind.ToString(),
                        Role = a.Role.ToString(),
                        DisplayOrder = a.DisplayOrder,
                    }),
            ],
            CreatedAt = view.CreatedAt,
            UpdatedAt = view.UpdatedAt,
        };
    }

    private static MediaAsset? Lookup(long? id, IReadOnlyDictionary<long, MediaAsset> map) =>
        id is { } value && map.TryGetValue(value, out var asset) ? asset : null;

    private static Guid? LookupPublicId(long? id, IReadOnlyDictionary<long, MediaAsset> map) =>
        Lookup(id, map)?.PublicId;
}
