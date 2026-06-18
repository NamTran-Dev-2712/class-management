using System.Text.RegularExpressions;

// Normalizes free-form exam tags (same rule as questions): trim, lowercase, collapse whitespace to
// dashes, strip any character outside [a-z0-9-], drop empties/over-long, dedupe, and cap the count.
// Matches the DB CHECK constraint on exam_tags.tag ('^[a-z0-9-]{1,50}$').
public static partial class ExamTagNormalizer
{
    public const int MaxTagLength = 50;
    public const int MaxTagsPerExam = 10;

    public static List<string> Normalize(IEnumerable<string>? rawTags)
    {
        if (rawTags is null)
            return [];

        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var raw in rawTags)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var slug = WhitespaceRegex().Replace(raw.Trim().ToLowerInvariant(), "-");
            slug = DisallowedRegex().Replace(slug, "");
            slug = DashCollapseRegex().Replace(slug, "-").Trim('-');

            if (slug.Length == 0)
                continue;

            if (slug.Length > MaxTagLength)
                slug = slug[..MaxTagLength].Trim('-');

            if (slug.Length == 0 || !seen.Add(slug))
                continue;

            result.Add(slug);
            if (result.Count >= MaxTagsPerExam)
                break;
        }

        return result;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("[^a-z0-9-]")]
    private static partial Regex DisallowedRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex DashCollapseRegex();
}
