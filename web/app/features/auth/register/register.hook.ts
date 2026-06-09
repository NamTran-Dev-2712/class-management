import { useMutation } from "@tanstack/react-query";

import { useAuth } from "@/hooks/use-auth";
import { authService } from "@/services/auth/auth.service";
import type { RegisterRequest } from "@/services/auth/dtos/commands/register/register.request";

/**
 * Register then auto-login, so the new user lands authenticated. Returns the
 * profile (with roles) for role-based redirect; hydrates the auth store.
 */
export function useRegister() {
    const { setUser } = useAuth();

    return useMutation({
        mutationFn: async (payload: RegisterRequest) => {
            await authService.register(payload);
            return authService.login({ email: payload.email, password: payload.password });
        },
        onSuccess: (user) => setUser(user),
    });
}
