import { redirect } from "react-router";

import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

/** Normalized request pathname (strips the v8_passThroughRequests `.data` suffix). */
export function pathnameOf(request: Request): string {
    return new URL(request.url).pathname.replace(/_?\.data$/, "");
}

/**
 * Require an authenticated user (resolved by the root middleware into context).
 * Redirects to `/login?redirectTo=` when absent. Use inside a route `loader`:
 * `const user = requireAuth(context.get(userContext), request);`
 */
export function requireAuth(user: ProfileResponse | null, request: Request): ProfileResponse {
    if (!user) {
        throw redirect(`/login?redirectTo=${encodeURIComponent(pathnameOf(request))}`);
    }
    return user;
}
