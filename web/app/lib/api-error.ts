import type { AxiosError } from "axios";

import type { ApiResponse } from "@/types/global/api.response";

/**
 * Normalized error thrown by the axios layer. Carries the backend's localized
 * `message`, field-level `errors`, and HTTP `statusCode` in a single shape so
 * UI code never has to dig through AxiosError internals.
 */
export class ApiError extends Error {
    readonly statusCode: number;
    readonly errors: string[];
    readonly traceId?: string;

    constructor(
        message: string,
        statusCode: number,
        errors: string[] = [],
        traceId?: string,
    ) {
        super(message);
        this.name = "ApiError";
        this.statusCode = statusCode;
        this.errors = errors;
        this.traceId = traceId;
    }

    static fromAxios(error: AxiosError<ApiResponse>): ApiError {
        const body = error.response?.data;
        const status = error.response?.status ?? 0;

        if (body && typeof body === "object" && "message" in body) {
            return new ApiError(
                body.message,
                status,
                body.errors ?? [],
                body.traceId,
            );
        }

        return new ApiError(error.message || "Network error", status);
    }
}
