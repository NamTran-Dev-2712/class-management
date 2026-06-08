import { redirect } from "react-router";

import { createServerApiClient } from "@/lib/axios.config";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";
import type { ApiResponse } from "@/types/global/api.response";

/**
 * Loader guard: resolves the current user by calling the API with the request's
 * auth cookie. Throws a redirect to `/login` when unauthenticated. Use inside a
 * route `loader`: `const user = await requireAuth(request);`
 */
export async function requireAuth(request: Request): Promise<ProfileResponse> {
    const api = createServerApiClient(request);
    try {
        const res = await api.get<ApiResponse<ProfileResponse>>("/auth/me");
        if (!res.data.data) throw new Error("No profile");
        return res.data.data;
    } catch {
        const redirectTo = new URL(request.url).pathname;
        throw redirect(`/login?redirectTo=${encodeURIComponent(redirectTo)}`);
    }
}
