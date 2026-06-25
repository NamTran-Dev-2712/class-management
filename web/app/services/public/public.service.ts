import { http } from "@/lib/axios.config";

export interface AppConfig {
    appName: string;
    maintenanceMode: boolean;
}

/** Anonymous-readable app configuration (brand + maintenance flag). */
export const publicService = {
    appConfig: () => http.get<AppConfig>("/public/app-config"),
};
