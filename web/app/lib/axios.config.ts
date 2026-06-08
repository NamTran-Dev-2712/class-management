import axios, { type AxiosInstance, type AxiosRequestConfig } from "axios";
import i18next from "i18next";

import { ApiError } from "@/lib/api-error";
import type { ApiResponse } from "@/types/global/api.response";

const baseURL = import.meta.env.VITE_API_URL ?? "/api";

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
        (error) => Promise.reject(ApiError.fromAxios(error)),
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
    async post<T>(
        url: string,
        body?: unknown,
        config?: AxiosRequestConfig,
    ): Promise<T> {
        const res = await apiClient.post<ApiResponse<T>>(url, body, config);
        return res.data.data as T;
    },
    async put<T>(
        url: string,
        body?: unknown,
        config?: AxiosRequestConfig,
    ): Promise<T> {
        const res = await apiClient.put<ApiResponse<T>>(url, body, config);
        return res.data.data as T;
    },
    async patch<T>(
        url: string,
        body?: unknown,
        config?: AxiosRequestConfig,
    ): Promise<T> {
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
    if (acceptLanguage)
        instance.defaults.headers.common["Accept-Language"] = acceptLanguage;
    return instance;
}
