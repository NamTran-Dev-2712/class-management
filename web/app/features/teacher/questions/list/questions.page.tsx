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
import type { QuestionListItem } from "@/services/question/dtos/queries/question-list";
import { getQuestionColumns } from "../_shared/question-columns";
import { QuestionFilters } from "../_shared/question-filters";
import { QuestionPreviewDialog } from "../_shared/question-preview";
import {
    useActiveSubjects,
    useDeleteQuestion,
    useSetQuestionVisibility,
    useTeacherQuestion,
    useTeacherQuestions,
} from "../_shared/questions.hook";
import { useQuestionFilters } from "../_shared/use-question-filters";
import type { Route } from "./+types/questions.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Question Bank · Class Management" }];
}

export default function QuestionsPage() {
    const { t, i18n } = useTranslation("question");
    const navigate = useNavigate();
    const filters = useQuestionFilters();
    const { data, isLoading } = useTeacherQuestions(filters.query);
    const { data: subjects } = useActiveSubjects();
    const remove = useDeleteQuestion();
    const setVisibility = useSetQuestionVisibility();

    const [previewId, setPreviewId] = useState<string | null>(null);
    const [deleting, setDeleting] = useState<QuestionListItem | null>(null);
    const { data: preview, isLoading: previewLoading } = useTeacherQuestion(previewId ?? undefined);

    const sorting: SortingState = filters.params.sortBy
        ? [{ id: filters.params.sortBy, desc: filters.params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getQuestionColumns({
                t,
                locale: i18n.language,
                onPreview: (q) => setPreviewId(q.publicId),
                onEdit: (q) => navigate(`/teacher/question-bank/${q.publicId}`),
                onToggleVisibility: (q) =>
                    setVisibility.mutate(
                        {
                            publicId: q.publicId,
                            payload: {
                                visibility: q.visibility === "Public" ? "Private" : "Public",
                            },
                        },
                        {
                            onSuccess: () => toast.success(t("toast.visibilityUpdated")),
                            onError: () => toast.error(t("toast.error")),
                        },
                    ),
                onDelete: (q) => setDeleting(q),
            }),
        [t, i18n.language, navigate, setVisibility],
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
            onError: () => toast.error(t("toast.error")),
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
                    <Button
                        variant="outline"
                        onClick={() => navigate("/teacher/question-bank/public")}
                    >
                        <Globe className="size-4" />
                        <span className="hidden sm:inline">{t("actions.browsePublic")}</span>
                    </Button>
                    <Button onClick={() => navigate("/teacher/question-bank/new")}>
                        <Plus className="size-4" />
                        <span className="hidden sm:inline">{t("actions.create")}</span>
                    </Button>
                </div>
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
