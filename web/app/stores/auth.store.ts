import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

interface AuthState {
    user: ProfileResponse | null;
    isAuthenticated: boolean;
    setUser: (user: ProfileResponse | null) => void;
    clear: () => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            user: null,
            isAuthenticated: false,
            setUser: (user) => set({ user, isAuthenticated: !!user }),
            clear: () => set({ user: null, isAuthenticated: false }),
        }),
        {
            name: "auth-storage",
            // localStorage is browser-only; `skipHydration` defers reading it until
            // the client calls `rehydrate()` (in root.tsx), avoiding SSR mismatch.
            storage: createJSONStorage(() => localStorage),
            partialize: (state) => ({ user: state.user, isAuthenticated: state.isAuthenticated }),
            skipHydration: true,
        },
    ),
);
