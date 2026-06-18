import type { SortingState } from "@tanstack/react-table";
import { Globe, Plus } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Button } from "@/components/ui/button";
import { ApiError } from "@/lib/api-error";
import { useActiveSubjects } from "@/features/teacher/questions/_shared/questions.hook";
import type { ExamListItem } from "@/services/exam/dtos/queries/exam-list";
import { getExamColumns } from "../_shared/exam-columns";
import { ExamFilters } from "../_shared/exam-filters";
import { ExamPreviewDialog } from "../_shared/exam-preview";
import {
    useDeleteExam,
    useDuplicateExam,
    useExamPreview,
    useSetExamVisibility,
    useTeacherExams,
} from "../_shared/exams.hook";
import { useExamFilters } from "../_shared/use-exam-filters";
import type { Route } from "./+types/exams.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Exams · Class Management" }];
}

export default function ExamsPage() {
    const { t, i18n } = useTranslation("exam");
    const navigate = useNavigate();
    const filters = useExamFilters();
    const { data, isLoading } = useTeacherExams(filters.query);
    const { data: subjects } = useActiveSubjects();
    const remove = useDeleteExam();
    const setVisibility = useSetExamVisibility();
    const duplicate = useDuplicateExam();

    const [previewId, setPreviewId] = useState<string | null>(null);
    const [deleting, setDeleting] = useState<ExamListItem | null>(null);
    const { data: preview, isLoading: previewLoading } = useExamPreview(previewId ?? undefined);

    const sorting: SortingState = filters.params.sortBy
        ? [{ id: filters.params.sortBy, desc: filters.params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getExamColumns({
                t,
                locale: i18n.language,
                onPreview: (e) => setPreviewId(e.publicId),
                onEdit: (e) => navigate(`/teacher/exams/${e.publicId}`),
                onDuplicate: (e) =>
                    duplicate.mutate(e.publicId, {
                        onSuccess: (res) => {
                            toast.success(t("toast.duplicated"));
                            navigate(`/teacher/exams/${res.publicId}`);
                        },
                        onError: (err) =>
                            toast.error(err instanceof ApiError ? err.message : t("toast.error")),
                    }),
                onToggleVisibility: (e) =>
                    setVisibility.mutate(
                        {
                            publicId: e.publicId,
                            payload: {
                                visibility: e.visibility === "Public" ? "Private" : "Public",
                            },
                        },
                        {
                            onSuccess: () => toast.success(t("toast.visibilityUpdated")),
                            onError: () => toast.error(t("toast.error")),
                        },
                    ),
                onDelete: (e) => setDeleting(e),
            }),
        [t, i18n.language, navigate, setVisibility, duplicate],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) filters.setSort(first.id, first.desc ? "desc" : "asc");
        else filters.setSort(undefined, "asc");
    };

    const confirmDelete = () => {
        if (!deleting) return;
        remove.mutate(deleting.publicId, {
            onSuccess: () => {
                toast.success(t("toast.deleted"));
                setDeleting(null);
            },
            onError: (err) => toast.error(err instanceof ApiError ? err.message : t("toast.error")),
        });
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between gap-4">
                <div>
                    <h2 className="text-xl font-semibold">{t("title")}</h2>
                    <p className="text-muted-foreground text-sm">{t("subtitle")}</p>
                </div>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={() => navigate("/teacher/exams/public")}>
                        <Globe className="size-4" />
                        <span className="hidden sm:inline">{t("actions.browsePublic")}</span>
                    </Button>
                    <Button onClick={() => navigate("/teacher/exams/new")}>
                        <Plus className="size-4" />
                        <span className="hidden sm:inline">{t("actions.create")}</span>
                    </Button>
                </div>
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

            <ExamPreviewDialog
                open={previewId !== null}
                onOpenChange={(open) => !open && setPreviewId(null)}
                exam={preview}
                isLoading={previewLoading}
            />

            <ConfirmDialog
                open={deleting !== null}
                onOpenChange={(open) => !open && setDeleting(null)}
                title={t("deleteConfirm.title")}
                description={t("deleteConfirm.description")}
                confirmLabel={t("actions.delete")}
                onConfirm={confirmDelete}
                isPending={remove.isPending}
                destructive
            />
        </div>
    );
}
