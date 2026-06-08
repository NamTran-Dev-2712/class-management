import { redirect } from "react-router";

import { requireAuth } from "@/guards/require-auth";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

/**
 * Loader guard: ensures the current user holds at least one of `allowedRoles`.
 * Redirects to `/login` when unauthenticated, or `/unauthorized` when the role
 * check fails. Use inside a route `loader`.
 */
export async function requireRole(
    request: Request,
    allowedRoles: string[],
): Promise<ProfileResponse> {
    const user = await requireAuth(request);
    const hasRole = allowedRoles.some((role) => user.roles.includes(role));
    if (!hasRole) throw redirect("/unauthorized");
    return user;
}
