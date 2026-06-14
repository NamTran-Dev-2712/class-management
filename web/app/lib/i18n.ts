import { DEFAULT_LANGUAGE, SUPPORTED_LANGUAGE_CODES } from "@/config/languages";

/** i18next namespaces — one JSON file per namespace under public/locales/<lng>/. */
export const I18N_NAMESPACES = [
    "common",
    "auth",
    "booking",
    "dashboard",
    "mentor",
    "payment",
    "public",
    "subject",
    "user",
] as const;

export const I18N_DEFAULT_NAMESPACE = "common";

/** Shared i18next options used by both the server and client init paths. */
export const i18nConfig = {
    supportedLngs: SUPPORTED_LANGUAGE_CODES,
    fallbackLng: DEFAULT_LANGUAGE,
    defaultNS: I18N_DEFAULT_NAMESPACE,
    ns: I18N_NAMESPACES,
    interpolation: { escapeValue: false },
} as const;
