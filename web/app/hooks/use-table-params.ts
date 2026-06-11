import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router";

export interface TableParams {
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortOrder: "asc" | "desc";
    searchTerm?: string;
}

const FALLBACK: TableParams = { pageNumber: 1, pageSize: 10, sortOrder: "asc" };

/**
 * URL-synced table state (page/pageSize/sort/search) shared by any paginated list.
 * Keeping it in the query string makes list views shareable and back/forward-friendly.
 */
export function useTableParams(defaults?: Partial<TableParams>) {
    const [searchParams, setSearchParams] = useSearchParams();
    const base = useMemo(() => ({ ...FALLBACK, ...defaults }), [defaults]);

    const params = useMemo<TableParams>(
        () => ({
            pageNumber: Number(searchParams.get("page")) || base.pageNumber,
            pageSize: Number(searchParams.get("pageSize")) || base.pageSize,
            sortBy: searchParams.get("sortBy") ?? base.sortBy,
            sortOrder: (searchParams.get("sortOrder") as "asc" | "desc") || base.sortOrder,
            searchTerm: searchParams.get("q") || undefined,
        }),
        [searchParams, base],
    );

    const patch = useCallback(
        (updates: Record<string, string | number | undefined>, resetPage = false) => {
            setSearchParams(
                (prev) => {
                    const next = new URLSearchParams(prev);
                    if (resetPage) next.delete("page");
                    for (const [key, value] of Object.entries(updates)) {
                        if (value === undefined || value === "") next.delete(key);
                        else next.set(key, String(value));
                    }
                    return next;
                },
                { replace: false },
            );
        },
        [setSearchParams],
    );

    const setPage = useCallback((page: number) => patch({ page }), [patch]);
    const setPageSize = useCallback((size: number) => patch({ pageSize: size }, true), [patch]);
    const setSort = useCallback(
        (sortBy: string | undefined, sortOrder: "asc" | "desc") =>
            patch({ sortBy, sortOrder }, true),
        [patch],
    );
    const setSearch = useCallback((term: string | undefined) => patch({ q: term }, true), [patch]);

    return { params, setPage, setPageSize, setSort, setSearch };
}
