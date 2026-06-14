/** Uniform envelope returned by the backend (ClassManagement.Api `ApiResponse<T>`). */
export interface ApiResponse<T = unknown> {
    success: boolean;
    message: string;
    data: T | null;
    errors: string[] | null;
    statusCode: number;
    traceId: string;
    timestamp: string;
}
