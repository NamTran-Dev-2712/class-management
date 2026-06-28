using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Payment;

// Shared EF value converter/comparer for the nullable JSONB string-list column (plans.features).
// Serialized to jsonb text; compared structurally so EF tracks element changes. Application stays
// EF-free — this lives in Infrastructure only.
internal static class PaymentJsonConverters
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static readonly ValueConverter<List<string>?, string?> NullableStringList = new(
        v => v == null ? null : JsonSerializer.Serialize(v, Options),
        v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<List<string>>(v, Options)
    );

    public static readonly ValueComparer<List<string>?> NullableStringListComparer = new(
        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
        v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
        v => v == null ? null : v.ToList()
    );
}
