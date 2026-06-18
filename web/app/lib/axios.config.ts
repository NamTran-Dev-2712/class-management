import axios, {
    type AxiosInstance,
    type AxiosRequestConfig,
    type InternalAxiosRequestConfig,
} from "axios";
import i18next from "i18next";

import { ApiError } from "@/lib/api-error";
import { useAuthStore } from "@/stores/auth.store";
import type { ApiResponse } from "@/types/global/api.response";

const baseURL = import.meta.env.VITE_API_URL ?? "/api";

// Auth endpoints must never trigger the refresh-retry (avoids recursion / pointless retries).
const AUTH_PATHS = [
    "/auth/refresh",
    "/auth/login",
    "/auth/register",
    "/auth/me",
    "/auth/logout",
    "/auth/forgot-password",
    "/auth/reset-password",
];

type RetriableConfig = InternalAxiosRequestConfig & { _retry?: boolean };

// Shared single-flight refresh: concurrent 401s wait on one POST /auth/refresh. Declared before the
// instance so the response interceptor (added in createInstance) can close over it.
let refreshPromise: Promise<void> | null = null;

function isAuthPath(url: string | undefined): boolean {
    return !!url && AUTH_PATHS.some((p) => url.includes(p));
}

function createInstance(): AxiosInstance {
    const instance = axios.create({
        baseURL,
        withCredentials: true, // send/receive the JWT auth cookie
        headers: { "Content-Type": "application/json" },
    });

    // Tell the API which language to localize responses in (browser only — on the
    // server the per-request factory below sets the header explicitly).
    instance.interceptors.request.use((config) => {
        if (typeof window !== "undefined" && i18next.language) {
            config.headers.set("Accept-Language", i18next.language);
        }
        return config;
    });

    instance.interceptors.response.use(
        (response) => response,
        async (error) => {
            const config = error?.config as RetriableConfig | undefined;
            const status = error?.response?.status;

            // Browser-only silent refresh: on a 401 from a non-auth request that hasn't been retried
            // yet, refresh the access token once (single-flight) and replay the original request. The
            // rotated cookies ride along automatically via `withCredentials`. SSR requests skip this —
            // the root middleware (auth.server.ts) already refreshes there.
            if (
                typeof window !== "undefined" &&
                status === 401 &&
                config &&
                !config._retry &&
                !isAuthPath(config.url)
            ) {
                config._retry = true;
                try {
                    refreshPromise ??= apiClient
                        .post("/auth/refresh")
                        .then(() => undefined)
                        .finally(() => {
                            refreshPromise = null;
                        });
                    await refreshPromise;
                    return apiClient(config);
                } catch {
                    // Refresh failed → the session is truly gone. Clear client state and bounce to
                    // login (guards would do the same on the next navigation).
                    useAuthStore.getState().clear();
                    if (!window.location.pathname.startsWith("/login")) {
                        window.location.href = "/login";
                    }
                    return Promise.reject(ApiError.fromAxios(error));
                }
            }

            return Promise.reject(ApiError.fromAxios(error));
        },
    );

    return instance;
}

/** Shared browser/client instance. */
export const apiClient = createInstance();

/** Typed facade that unwraps the backend `ApiResponse<T>` envelope to `T`. */
export const http = {
    async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const res = await apiClient.get<ApiResponse<T>>(url, config);
        return res.data.data as T;
    },
    async post<T>(url: string, body?: unknown, config?: AxiosRequestConfig): Promise<T> {
        const res = await apiClient.post<ApiResponse<T>>(url, body, config);
        return res.data.data as T;
    },
    async put<T>(url: string, body?: unknown, config?: AxiosRequestConfig): Promise<T> {
        const res = await apiClient.put<ApiResponse<T>>(url, body, config);
        return res.data.data as T;
    },
    async patch<T>(url: string, body?: unknown, config?: AxiosRequestConfig): Promise<T> {
        const res = await apiClient.patch<ApiResponse<T>>(url, body, config);
        return res.data.data as T;
    },
    async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const res = await apiClient.delete<ApiResponse<T>>(url, config);
        return res.data.data as T;
    },
};

/**
 * Per-request instance for use inside loaders/actions during SSR. Forwards the
 * incoming Cookie (auth) and Accept-Language (locale) to the backend, since the
 * browser's automatic cookie/header behavior is unavailable server-side.
 */
export function createServerApiClient(request: Request): AxiosInstance {
    const instance = createInstance();
    const cookie = request.headers.get("Cookie");
    const acceptLanguage = request.headers.get("Accept-Language");
    if (cookie) instance.defaults.headers.common["Cookie"] = cookie;
    if (acceptLanguage) instance.defaults.headers.common["Accept-Language"] = acceptLanguage;
    return instance;
}
