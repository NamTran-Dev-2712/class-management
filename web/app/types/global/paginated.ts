/** Mirrors the backend `PaginatedResult<T>` payload. */
export interface Paginated<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
}
