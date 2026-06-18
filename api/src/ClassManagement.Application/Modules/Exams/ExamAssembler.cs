// Builds the child ExamTag entities for an exam from client input, applying the normalization +
// cap rules (ExamTagNormalizer). Count validation lives in the validator.
internal static class ExamAssembler
{
    public static List<ExamTag> BuildTags(IEnumerable<string>? rawTags) =>
        ExamTagNormalizer.Normalize(rawTags).Select(t => new ExamTag { Tag = t }).ToList();
}
