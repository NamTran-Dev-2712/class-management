import { useMutation } from "@tanstack/react-query";

import { authService } from "@/services/auth/auth.service";

/** Requests a password-reset OTP. The backend always responds 200 (no account enumeration). */
export function useForgotPassword() {
    return useMutation({ mutationFn: authService.forgotPassword });
}
