import { useEffect } from "react";

import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";
import { useAuthStore } from "@/stores/auth.store";

/**
 * Keeps the client auth store in sync with the server-resolved user on every
 * navigation/refresh. The root loader re-runs server-side each request (after the
 * middleware refreshes tokens), so `user` is always fresh; we mirror it into the
 * store (or clear it when the session ended) as the single client source of truth.
 */
export function AuthProvider({
    user,
    children,
}: {
    user: ProfileResponse | null;
    children: React.ReactNode;
}) {
    const setUser = useAuthStore((s) => s.setUser);
    const clear = useAuthStore((s) => s.clear);

    useEffect(() => {
        if (user) setUser(user);
        else clear();
    }, [user, setUser, clear]);

    return <>{children}</>;
}
