import {
    DEFAULT_LANGUAGE,
    LANGUAGE_COOKIE,
    SUPPORTED_LANGUAGE_CODES,
} from "@/config/languages";

const isSupported = (lng: string | undefined): lng is string =>
    !!lng && (SUPPORTED_LANGUAGE_CODES as readonly string[]).includes(lng);

function readCookie(request: Request, key: string): string | undefined {
    const header = request.headers.get("Cookie");
    if (!header) return undefined;
    for (const part of header.split(";")) {
        const [name, ...rest] = part.trim().split("=");
        if (name === key) return decodeURIComponent(rest.join("="));
    }
    return undefined;
}

/**
 * Resolve the request language on the server: cookie first, then the
 * Accept-Language header, falling back to the default. Mirrors the client-side
 * LanguageDetector order so SSR output and hydration agree.
 */
export function detectLocale(request: Request): string {
    const cookieLng = readCookie(request, LANGUAGE_COOKIE);
    if (isSupported(cookieLng)) return cookieLng;

    const header = request.headers.get("Accept-Language");
    if (header) {
        for (const entry of header.split(",")) {
            const code = entry
                .split(";")[0]
                ?.trim()
                .split("-")[0]
                ?.toLowerCase();
            if (isSupported(code)) return code;
        }
    }

    return DEFAULT_LANGUAGE;
}
