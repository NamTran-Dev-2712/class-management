import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";

import { useAuth } from "@/hooks/use-auth";
import { authService } from "@/services/auth/auth.service";

/** Logs out: clears the server cookie, the auth store, and cached queries. */
export function useLogout() {
    const { clear } = useAuth();
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: () => authService.logout(),
        // Clear locally even if the network call fails — the user intends to leave.
        onSettled: () => {
            clear();
            queryClient.clear();
            void navigate("/login");
        },
    });
}
