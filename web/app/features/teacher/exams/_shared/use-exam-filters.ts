import { useCallback } from "react";
import { useSearchParams } from "react-router";

import { useTableParams } from "@/hooks/use-table-params";
import type { ExamListQuery, ExamVisibility } from "@/services/exam/dtos/queries/exam-list";
import { FILTER_ALL, type ExamFilterState } from "./exam-filters";

/**
 * URL-synced controller for the exam list pages: table params (page/sort/search) plus the
 * exam-specific facets (subject, visibility). Returns the assembled query, the filter state for
 * <ExamFilters>, and the setters — so list/public/admin pages stay thin.
 */
export function useExamFilters(defaults?: { sortBy?: string; sortOrder?: "asc" | "desc" }) {
    const { params, setPage, setPageSize, setSort, setSearch } = useTableParams({
        pageSize: 10,
        sortBy: defaults?.sortBy ?? "updatedAt",
        sortOrder: defaults?.sortOrder ?? "desc",
    });
    const [searchParams, setSearchParams] = useSearchParams();

    const get = (key: string) => searchParams.get(key) ?? FILTER_ALL;

    const setFacet = useCallback(
        (key: string, value: string) =>
            setSearchParams((prev) => {
                const next = new URLSearchParams(prev);
                next.delete("page");
                if (!value || value === FILTER_ALL) next.delete(key);
                else next.set(key, value);
                return next;
            }),
        [setSearchParams],
    );

    const state: ExamFilterState = {
        searchTerm: params.searchTerm ?? "",
        subjectId: get("subjectId"),
        visibility: get("visibility"),
        tag: searchParams.get("tag") ?? "",
    };

    const toFacet = (value: string) => (value === FILTER_ALL ? undefined : value);

    const query: ExamListQuery = {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        subjectId: toFacet(state.subjectId),
        visibility: toFacet(state.visibility) as ExamVisibility | undefined,
        tag: state.tag || undefined,
    };

    return {
        params,
        query,
        state,
        setPage,
        setPageSize,
        setSort,
        setSearch,
        setSubject: (v: string) => setFacet("subjectId", v),
        setVisibility: (v: string) => setFacet("visibility", v),
        setTag: (v: string) => setFacet("tag", v),
    };
}
