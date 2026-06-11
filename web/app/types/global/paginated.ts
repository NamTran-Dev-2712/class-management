/** Mirrors the backend `PaginatedResult<T>` JSON envelope. */
export interface Paginated<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

/** Common query params accepted by paginated list endpoints (mirrors BaseFilterQuery). */
export interface PageQuery {
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortOrder?: "asc" | "desc";
    searchTerm?: string;
}
