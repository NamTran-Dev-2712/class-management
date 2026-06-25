import { create } from "zustand";

interface AppConfigState {
    /** Brand name from system_settings; null until the public config is fetched (SSR-safe fallback). */
    appName: string | null;
    maintenanceMode: boolean;
    setConfig: (config: { appName: string; maintenanceMode: boolean }) => void;
}

// Not persisted: starts null so SSR and the first client render both fall back to the i18n brand,
// avoiding a hydration mismatch; updated once the public config loads on the client.
export const useAppConfigStore = create<AppConfigState>()((set) => ({
    appName: null,
    maintenanceMode: false,
    setConfig: ({ appName, maintenanceMode }) => set({ appName, maintenanceMode }),
}));
