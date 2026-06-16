import { useCallback } from "react";
import { useSearchParams } from "react-router";

import { useTableParams } from "@/hooks/use-table-params";
import type {
    QuestionDifficulty,
    QuestionListQuery,
    QuestionType,
    QuestionVisibility,
} from "@/services/question/dtos/queries/question-list";
import { FILTER_ALL, type QuestionFilterState } from "./question-filters";

/**
 * URL-synced controller for the question list pages: table params (page/sort/search) plus the
 * question-specific facets (type, difficulty, subject, visibility, tag). Returns the assembled query,
 * the filter state for <QuestionFilters>, and the setters — so list/public/admin pages stay thin.
 */
export function useQuestionFilters(defaults?: { sortBy?: string; sortOrder?: "asc" | "desc" }) {
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

    const state: QuestionFilterState = {
        searchTerm: params.searchTerm ?? "",
        type: get("type"),
        difficulty: get("difficulty"),
        subjectId: get("subjectId"),
        visibility: get("visibility"),
        tag: searchParams.get("tag") ?? "",
    };

    const toFacet = (value: string) => (value === FILTER_ALL ? undefined : value);

    const query: QuestionListQuery = {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        type: toFacet(state.type) as QuestionType | undefined,
        difficulty: toFacet(state.difficulty) as QuestionDifficulty | undefined,
        subjectId: toFacet(state.subjectId),
        visibility: toFacet(state.visibility) as QuestionVisibility | undefined,
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
        setType: (v: string) => setFacet("type", v),
        setDifficulty: (v: string) => setFacet("difficulty", v),
        setSubject: (v: string) => setFacet("subjectId", v),
        setVisibility: (v: string) => setFacet("visibility", v),
        setTag: (v: string) => setFacet("tag", v),
    };
}
