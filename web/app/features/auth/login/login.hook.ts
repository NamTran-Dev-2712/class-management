import { useMutation } from "@tanstack/react-query";

import { useAuth } from "@/hooks/use-auth";
import { authService } from "@/services/auth/auth.service";

/** Login mutation: authenticates and hydrates the auth store on success. */
export function useLogin() {
    const { setUser } = useAuth();

    return useMutation({
        mutationFn: authService.login,
        onSuccess: (user) => setUser(user),
    });
}
