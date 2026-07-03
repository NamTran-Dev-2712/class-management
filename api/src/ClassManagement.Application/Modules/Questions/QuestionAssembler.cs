using ClassManagement.Application.Modules.Questions.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

// Builds the child Option/Tag entities for a question from client input, applying the per-type
// rules (BR-3-02..05): choice types keep the supplied options in order; TrueFalse forces the two
// canonical "True"/"False" labels (server-seeded) while honouring the teacher's correct choice;
// writing types have no options. Validation of counts/correctness lives in the validators.
internal static class QuestionAssembler
{
    public const string TrueLabel = "True";
    public const string FalseLabel = "False";

    public static List<QuestionOption> BuildOptions(
        QuestionType type,
        IReadOnlyList<QuestionOptionInput>? inputs,
        IReadOnlyDictionary<Guid, long>? mediaMap = null
    )
    {
        if (type is QuestionType.ShortWriting or QuestionType.LongWriting || inputs is null)
            return [];

        if (type == QuestionType.TrueFalse)
        {
            // Seed the fixed labels by position; the teacher only chooses which side is correct.
            return
            [
                new QuestionOption
                {
                    Content = TrueLabel,
                    IsCorrect = inputs.Count > 0 && inputs[0].IsCorrect,
                    DisplayOrder = 0,
                },
                new QuestionOption
                {
                    Content = FalseLabel,
                    IsCorrect = inputs.Count > 1 && inputs[1].IsCorrect,
                    DisplayOrder = 1,
                },
            ];
        }

        return inputs
            .Select(
                (o, i) =>
                    new QuestionOption
                    {
                        Content = o.Content.Trim(),
                        IsCorrect = o.IsCorrect,
                        DisplayOrder = i,
                        MediaId = ResolveMediaId(o.MediaPublicId, mediaMap),
                    }
            )
            .ToList();
    }

    public static List<QuestionTag> BuildTags(IEnumerable<string>? rawTags) =>
        QuestionTagNormalizer.Normalize(rawTags).Select(t => new QuestionTag { Tag = t }).ToList();

    // Builds the question-level attachment rows (MVP-9), re-indexing display order 0-based from position.
    public static List<QuestionMedia> BuildMedia(
        IReadOnlyList<QuestionMediaInput>? attachments,
        IReadOnlyDictionary<Guid, long> mediaMap
    )
    {
        if (attachments is null || attachments.Count == 0)
            return [];

        return attachments
            .OrderBy(a => a.DisplayOrder)
            .Select(
                (a, i) =>
                    new QuestionMedia
                    {
                        MediaId = mediaMap[a.MediaPublicId],
                        Role = a.Role,
                        DisplayOrder = i,
                    }
            )
            .ToList();
    }

    // Every media public id referenced by an option image or a question attachment (distinct), so the
    // handler can validate ownership + Confirmed status in one query.
    public static IReadOnlyList<Guid> CollectMediaPublicIds(
        QuestionType type,
        IReadOnlyList<QuestionOptionInput>? options,
        IReadOnlyList<QuestionMediaInput>? attachments
    )
    {
        var ids = new HashSet<Guid>();

        // TrueFalse options are server-seeded (no client media); other choice types may carry images.
        if (
            type
                is not (
                    QuestionType.TrueFalse
                    or QuestionType.ShortWriting
                    or QuestionType.LongWriting
                )
            && options is not null
        )
        {
            foreach (var o in options)
                if (o.MediaPublicId is { } id)
                    ids.Add(id);
        }

        if (attachments is not null)
            foreach (var a in attachments)
                ids.Add(a.MediaPublicId);

        return [.. ids];
    }

    private static long? ResolveMediaId(
        Guid? mediaPublicId,
        IReadOnlyDictionary<Guid, long>? mediaMap
    ) =>
        mediaPublicId is { } id
        && mediaMap is not null
        && mediaMap.TryGetValue(id, out var internalId)
            ? internalId
            : null;
}
