namespace ClassManagement.Application.Interfaces.Localization;

/// <summary>
/// Resolves message keys (e.g. "Auth.InvalidCredentials") to localized text for the
/// current request culture, falling back to the default culture and finally to the key
/// itself when no translation exists.
/// </summary>
public interface ILocalizationService
{
    /// <summary>Translate a key for the current request culture.</summary>
    string this[string key] { get; }

    /// <summary>Translate a key and apply <see cref="string.Format(string, object[])"/> arguments.</summary>
    string Translate(string key, params object[] args);
}
