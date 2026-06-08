import { create } from "zustand";

import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

interface AuthState {
    user: ProfileResponse | null;
    isAuthenticated: boolean;
    setUser: (user: ProfileResponse | null) => void;
    clear: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
    user: null,
    isAuthenticated: false,
    setUser: (user) => set({ user, isAuthenticated: !!user }),
    clear: () => set({ user: null, isAuthenticated: false }),
}));
