/** Languages supported by the app. English is the default / fallback. */
export const SUPPORTED_LANGUAGES = [
    { code: "en", label: "English", flag: "🇬🇧" },
    { code: "vi", label: "Tiếng Việt", flag: "🇻🇳" },
] as const;

export type LanguageCode = (typeof SUPPORTED_LANGUAGES)[number]["code"];

export const SUPPORTED_LANGUAGE_CODES = SUPPORTED_LANGUAGES.map((l) => l.code);

export const DEFAULT_LANGUAGE: LanguageCode = "en";

/** Cookie key used to persist the user's language preference (client + server). */
export const LANGUAGE_COOKIE = "i18next";
