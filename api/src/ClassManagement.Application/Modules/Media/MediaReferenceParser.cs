using System.Text.RegularExpressions;

// Extracts the media public ids referenced inline in Markdown/HTML content (MVP-9). Our storage keys are
// `media/{publicId:N}.{ext}` so the public CDN URL always contains `media/{32-hex}.` — this matches that
// precise pattern (avoiding over-matching arbitrary GUIDs in prose) and returns the parsed public ids.
internal static partial class MediaReferenceParser
{
    [GeneratedRegex(@"media/([0-9a-fA-F]{32})\.", RegexOptions.CultureInvariant)]
    private static partial Regex StorageKeyRegex();

    public static IReadOnlyList<Guid> ExtractPublicIds(params string?[] contents)
    {
        var ids = new HashSet<Guid>();
        foreach (var content in contents)
        {
            if (string.IsNullOrEmpty(content))
                continue;

            foreach (Match match in StorageKeyRegex().Matches(content))
            {
                if (Guid.TryParseExact(match.Groups[1].Value, "N", out var id))
                    ids.Add(id);
            }
        }
        return [.. ids];
    }
}
