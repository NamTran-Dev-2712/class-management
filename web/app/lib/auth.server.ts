import { createServerApiClient } from "@/lib/axios.config";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";
import type { ApiResponse } from "@/types/global/api.response";

export interface AuthResolution {
    user: ProfileResponse | null;
    /** New `Set-Cookie` values from a token refresh, to forward to the browser. */
    setCookies: string[];
}

const EMPTY: AuthResolution = { user: null, setCookies: [] };

function hasCookie(request: Request, name: string): boolean {
    const header = request.headers.get("Cookie");
    if (!header) return false;
    return header.split(";").some((part) => part.trim().startsWith(`${name}=`));
}

function extractSetCookie(headers: unknown): string[] {
    const value = (headers as { [k: string]: unknown })?.["set-cookie"];
    if (Array.isArray(value)) return value as string[];
    if (typeof value === "string") return [value];
    return [];
}

/**
 * Resolve the current user for an SSR request, refreshing tokens when needed:
 *  1. If neither auth cookie is present → anonymous (no API calls).
 *  2. Try `GET /auth/me` with the request cookies.
 *  3. On failure, if a refresh cookie exists, `POST /auth/refresh` — the backend
 *     rotates the tokens (old refresh token invalidated) and returns the profile;
 *     its `Set-Cookie` headers are captured to forward to the browser.
 * Never throws — returns `{ user: null, setCookies: [] }` when unauthenticated.
 */
export async function authenticateRequest(request: Request): Promise<AuthResolution> {
    const hasAccess = hasCookie(request, "access_token");
    const hasRefresh = hasCookie(request, "refresh_token");
    if (!hasAccess && !hasRefresh) return EMPTY;

    const api = createServerApiClient(request);

    if (hasAccess) {
        try {
            const res = await api.get<ApiResponse<ProfileResponse>>("/auth/me");
            if (res.data.data) return { user: res.data.data, setCookies: [] };
        } catch {
            // Access token likely expired — fall through to refresh.
        }
    }

    if (hasRefresh) {
        try {
            const res = await api.post<ApiResponse<ProfileResponse>>("/auth/refresh");
            return { user: res.data.data ?? null, setCookies: extractSetCookie(res.headers) };
        } catch {
            // Refresh token invalid/expired — treat as anonymous.
        }
    }

    return EMPTY;
}
