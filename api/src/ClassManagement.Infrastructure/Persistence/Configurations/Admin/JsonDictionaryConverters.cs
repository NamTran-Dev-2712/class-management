using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Admin;

// Shared EF value converter/comparer for the nullable JSONB string-keyed dictionary columns
// (audit_logs.metadata, notifications.payload). Serialized to jsonb text; compared structurally so EF
// tracks element changes. Application stays EF-free — these live in Infrastructure only.
internal static class JsonDictionaryConverters
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static readonly ValueConverter<Dictionary<string, object?>?, string?> Nullable = new(
        v => v == null ? null : JsonSerializer.Serialize(v, Options),
        v =>
            string.IsNullOrEmpty(v)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(v, Options)
    );

    public static readonly ValueComparer<Dictionary<string, object?>?> NullableComparer = new(
        (a, b) =>
            (a == null && b == null)
            || (
                a != null
                && b != null
                && JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options)
            ),
        v => v == null ? 0 : JsonSerializer.Serialize(v, Options).GetHashCode(),
        v =>
            v == null
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    JsonSerializer.Serialize(v, Options),
                    Options
                )
    );
}
