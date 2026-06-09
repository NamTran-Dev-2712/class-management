import { http } from "@/lib/axios.config";
import type { LoginRequest } from "@/services/auth/dtos/commands/login/login.request";
import type { LoginResponse } from "@/services/auth/dtos/commands/login/login.response";
import type { RegisterRequest } from "@/services/auth/dtos/commands/register/register.request";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

export const authService = {
    register: (payload: RegisterRequest) =>
        http.post<{ userId: number }>("/auth/register", payload),
    login: (payload: LoginRequest) => http.post<LoginResponse>("/auth/login", payload),
    getProfile: () => http.get<ProfileResponse>("/auth/me"),
    logout: () => http.post<void>("/auth/logout"),
};
