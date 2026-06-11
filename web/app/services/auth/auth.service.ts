import { http } from "@/lib/axios.config";
import type { ForgotPasswordRequest } from "@/services/auth/dtos/commands/forgot-password/request";
import type { LoginRequest } from "@/services/auth/dtos/commands/login/login.request";
import type { LoginResponse } from "@/services/auth/dtos/commands/login/login.response";
import type { RegisterRequest } from "@/services/auth/dtos/commands/register/register.request";
import type { ResetPasswordRequest } from "@/services/auth/dtos/commands/reset-password/request";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

export const authService = {
    register: (payload: RegisterRequest) =>
        http.post<{ userId: number }>("/auth/register", payload),
    login: (payload: LoginRequest) => http.post<LoginResponse>("/auth/login", payload),
    getProfile: () => http.get<ProfileResponse>("/auth/me"),
    logout: () => http.post<void>("/auth/logout"),
    forgotPassword: (payload: ForgotPasswordRequest) =>
        http.post<null>("/auth/forgot-password", payload),
    resetPassword: (payload: ResetPasswordRequest) =>
        http.post<null>("/auth/reset-password", payload),
};
