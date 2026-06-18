import type { SortingState } from "@tanstack/react-table";
import { ArrowLeft } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router";
import { toast } from "sonner";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { ApiError } from "@/lib/api-error";
import { useActiveSubjects } from "@/features/teacher/questions/_shared/questions.hook";
import { getExamColumns } from "../_shared/exam-columns";
import { ExamDetailDialog } from "../_shared/exam-detail-dialog";
import { ExamFilters } from "../_shared/exam-filters";
import { useDuplicateExam, usePublicExam, usePublicExams } from "../_shared/exams.hook";
import { useExamFilters } from "../_shared/use-exam-filters";
import type { Route } from "./+types/public-exams.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Public Exams · Class Management" }];
}

export default function PublicExamsPage() {
    const { t, i18n } = useTranslation("exam");
    const navigate = useNavigate();
    const filters = useExamFilters();
    const { data, isLoading } = usePublicExams(filters.query);
    const { data: subjects } = useActiveSubjects();
    const duplicate = useDuplicateExam();

    const [previewId, setPreviewId] = useState<string | null>(null);
    const { data: preview, isLoading: previewLoading } = usePublicExam(previewId ?? undefined);

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
                onDuplicate: (e) =>
                    duplicate.mutate(e.publicId, {
                        onSuccess: (res) => {
                            toast.success(t("toast.duplicated"));
                            navigate(`/teacher/exams/${res.publicId}`);
                        },
                        onError: (err) =>
                            toast.error(err instanceof ApiError ? err.message : t("toast.error")),
                    }),
            }),
        [t, i18n.language, navigate, duplicate],
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
                <Link
                    to="/teacher/exams"
                    className="text-muted-foreground hover:text-foreground mb-2 inline-flex items-center gap-1.5 text-sm"
                >
                    <ArrowLeft className="size-4" />
                    {t("actions.back")}
                </Link>
                <h2 className="text-xl font-semibold">{t("public.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("public.subtitle")}</p>
            </div>

            <ExamFilters
                state={filters.state}
                subjects={subjects?.items ?? []}
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
