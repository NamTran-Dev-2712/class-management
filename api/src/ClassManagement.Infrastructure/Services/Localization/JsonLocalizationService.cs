using System.Globalization;
using System.Text.Json;
using ClassManagement.Application.Interfaces.Localization;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Localization;

/// <summary>
/// File-backed localizer. Loads one flat key→value JSON document per supported culture
/// (embedded under Resources/&lt;culture&gt;.json) once at construction, then resolves keys
/// against <see cref="CultureInfo.CurrentUICulture"/> set by the RequestLocalization middleware.
/// </summary>
public sealed class JsonLocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _resources = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly string _defaultCulture;

    public JsonLocalizationService(IOptions<LocalizationOptions> options)
    {
        var opt = options.Value;
        _defaultCulture = opt.DefaultCulture;

        var assembly = typeof(JsonLocalizationService).Assembly;
        var prefix = $"{assembly.GetName().Name}.Resources.";

        foreach (var culture in opt.SupportedCultures)
        {
            using var stream = assembly.GetManifestResourceStream($"{prefix}{culture}.json");
            if (stream is null)
            {
                _resources[culture] = new();
                continue;
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            _resources[culture] =
                JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
    }

    public string this[string key] => Translate(key);

    public string Translate(string key, params object[] args)
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var value = Lookup(culture, key) ?? Lookup(_defaultCulture, key) ?? key;
        return args.Length > 0 ? string.Format(CultureInfo.CurrentCulture, value, args) : value;
    }

    private string? Lookup(string culture, string key) =>
        _resources.TryGetValue(culture, out var map) && map.TryGetValue(key, out var value)
            ? value
            : null;
}
