import type { SortingState } from "@tanstack/react-table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { useActiveSubjects } from "@/features/teacher/questions/_shared/questions.hook";
import { getExamColumns } from "@/features/teacher/exams/_shared/exam-columns";
import { ExamDetailDialog } from "@/features/teacher/exams/_shared/exam-detail-dialog";
import { ExamFilters } from "@/features/teacher/exams/_shared/exam-filters";
import { useExamFilters } from "@/features/teacher/exams/_shared/use-exam-filters";
import { useAdminExam, useAdminExams } from "./admin-exams.hook";
import type { Route } from "./+types/exams.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Exams · Class Management" }];
}

export default function AdminExamsPage() {
    const { t, i18n } = useTranslation("exam");
    const filters = useExamFilters();
    const { data, isLoading } = useAdminExams(filters.query);
    const { data: subjects } = useActiveSubjects();

    const [previewId, setPreviewId] = useState<string | null>(null);
    const { data: preview, isLoading: previewLoading } = useAdminExam(previewId ?? undefined);

    const sorting: SortingState = filters.params.sortBy
        ? [{ id: filters.params.sortBy, desc: filters.params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getExamColumns({
                t,
                locale: i18n.language,
                showOwner: true,
                onPreview: (e) => setPreviewId(e.publicId),
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

            <ExamFilters
                state={filters.state}
                subjects={subjects?.items ?? []}
                showVisibility
                onSearchChange={filters.setSearch}
                onSubjectChange={filters.setSubject}
                onVisibilityChange={filters.setVisibility}
                onTagChange={filters.setTag}
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

            <ExamDetailDialog
                open={previewId !== null}
                onOpenChange={(open) => !open && setPreviewId(null)}
                exam={preview}
                isLoading={previewLoading}
            />
        </div>
    );
}
