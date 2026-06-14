import { useAuthStore } from "@/stores/auth.store";

/** Convenience selector hook over the auth store. */
export function useAuth() {
    const user = useAuthStore((s) => s.user);
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const setUser = useAuthStore((s) => s.setUser);
    const clear = useAuthStore((s) => s.clear);

    return { user, isAuthenticated, roles: user?.roles ?? [], setUser, clear };
}
