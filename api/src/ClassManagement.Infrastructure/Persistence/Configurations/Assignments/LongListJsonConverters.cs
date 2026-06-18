using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// Shared EF value converters/comparers for the JSONB long-array columns (attempt question order +
// selected option ids). Stored as jsonb text; compared by sequence so EF tracks element changes.
internal static class LongListJsonConverters
{
    public static readonly ValueConverter<List<long>, string> NonNull = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<long>>(v, (JsonSerializerOptions?)null) ?? new()
    );

    public static readonly ValueComparer<List<long>> NonNullComparer = new(
        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
        v => v == null ? 0 : v.Aggregate(17, (h, x) => HashCode.Combine(h, x)),
        v => v == null ? new List<long>() : v.ToList()
    );

    public static readonly ValueConverter<List<long>?, string?> Nullable = new(
        v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v =>
            v == null
                ? null
                : JsonSerializer.Deserialize<List<long>>(v, (JsonSerializerOptions?)null)
    );

    public static readonly ValueComparer<List<long>?> NullableComparer = new(
        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
        v => v == null ? 0 : v.Aggregate(17, (h, x) => HashCode.Combine(h, x)),
        v => v == null ? null : v.ToList()
    );
}
