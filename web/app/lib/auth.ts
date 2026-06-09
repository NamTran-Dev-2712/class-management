import { Roles } from "@/config/roles";

/** The home/landing route for a user, by role precedence (Admin > Teacher > Student). */
export function roleHome(roles: string[]): string {
    if (roles.includes(Roles.Admin)) return "/admin";
    if (roles.includes(Roles.Teacher)) return "/teacher";
    if (roles.includes(Roles.Student)) return "/student";
    return "/";
}

/** Validate a post-login `redirectTo` query value — only allow same-origin app paths. */
export function safeRedirect(target: string | null, fallback: string): string {
    if (!target || !target.startsWith("/") || target.startsWith("//")) return fallback;
    return target;
}
