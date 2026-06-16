import type { SortingState } from "@tanstack/react-table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { getQuestionColumns } from "@/features/teacher/questions/_shared/question-columns";
import { QuestionFilters } from "@/features/teacher/questions/_shared/question-filters";
import { QuestionPreviewDialog } from "@/features/teacher/questions/_shared/question-preview";
import { useActiveSubjects } from "@/features/teacher/questions/_shared/questions.hook";
import { useQuestionFilters } from "@/features/teacher/questions/_shared/use-question-filters";
import { useAdminQuestion, useAdminQuestions } from "./admin-questions.hook";
import type { Route } from "./+types/questions.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Questions · Class Management" }];
}

export default function AdminQuestionsPage() {
    const { t, i18n } = useTranslation("question");
    const filters = useQuestionFilters();
    const { data, isLoading } = useAdminQuestions(filters.query);
    const { data: subjects } = useActiveSubjects();

    const [previewId, setPreviewId] = useState<string | null>(null);
    const { data: preview, isLoading: previewLoading } = useAdminQuestion(previewId ?? undefined);

    const sorting: SortingState = filters.params.sortBy
        ? [{ id: filters.params.sortBy, desc: filters.params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getQuestionColumns({
                t,
                locale: i18n.language,
                showOwner: true,
                onPreview: (q) => setPreviewId(q.publicId),
            }),
        [t, i18n.language],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) filters.setSort(first.id, first.desc ? "desc" : "asc");
        else filters.setSort(undefined, "asc");
    };

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("admin.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("admin.subtitle")}</p>
            </div>

            <QuestionFilters
                state={filters.state}
                subjects={subjects?.items ?? []}
                showVisibility
                onSearchChange={filters.setSearch}
                onTagChange={filters.setTag}
                onTypeChange={filters.setType}
                onDifficultyChange={filters.setDifficulty}
                onSubjectChange={filters.setSubject}
                onVisibilityChange={filters.setVisibility}
            />

            <DataTable
                columns={columns}
                data={data?.items ?? []}
                isLoading={isLoading}
                sorting={sorting}
                onSortingChange={handleSortingChange}
            />

            {data && data.items.length > 0 ? (
                <DataTablePagination
                    pageNumber={data.pageNumber}
                    pageSize={data.pageSize}
                    totalPages={data.totalPages}
                    totalCount={data.totalCount}
                    hasPreviousPage={data.hasPreviousPage}
                    hasNextPage={data.hasNextPage}
                    onPageChange={filters.setPage}
                    onPageSizeChange={filters.setPageSize}
                />
            ) : null}

            <QuestionPreviewDialog
                open={previewId !== null}
                onOpenChange={(open) => !open && setPreviewId(null)}
                question={preview}
                isLoading={previewLoading}
            />
        </div>
    );
}
