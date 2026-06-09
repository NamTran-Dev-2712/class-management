import { redirect } from "react-router";

import { roleHome } from "@/lib/auth";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

/**
 * Guest-only guard: if the request is already authenticated, redirect to the
 * user's role home instead of showing the page (login/register).
 */
export function requireGuest(user: ProfileResponse | null): null {
    if (user) throw redirect(roleHome(user.roles));
    return null;
}
