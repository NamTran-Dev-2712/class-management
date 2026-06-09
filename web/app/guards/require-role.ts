import { redirect } from "react-router";

import { requireAuth } from "@/guards/require-auth";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

/**
 * Require an authenticated user holding at least one of `allowedRoles`. Redirects
 * to `/login` when unauthenticated, or `/unauthorized` when the role check fails.
 */
export function requireRole(
    user: ProfileResponse | null,
    allowedRoles: string[],
    request: Request,
): ProfileResponse {
    const current = requireAuth(user, request);
    if (!allowedRoles.some((role) => current.roles.includes(role))) {
        throw redirect("/unauthorized");
    }
    return current;
}
