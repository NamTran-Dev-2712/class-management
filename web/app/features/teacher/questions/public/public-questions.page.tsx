import type { SortingState } from "@tanstack/react-table";
import { ArrowLeft } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router";
import { toast } from "sonner";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { getQuestionColumns } from "../_shared/question-columns";
import { QuestionFilters } from "../_shared/question-filters";
import { QuestionPreviewDialog } from "../_shared/question-preview";
import {
    useActiveSubjects,
    useDuplicateQuestion,
    usePublicQuestion,
    usePublicQuestions,
} from "../_shared/questions.hook";
import { useQuestionFilters } from "../_shared/use-question-filters";
import type { Route } from "./+types/public-questions.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Public Questions · Class Management" }];
}

export default function PublicQuestionsPage() {
    const { t, i18n } = useTranslation("question");
    const navigate = useNavigate();
    const filters = useQuestionFilters();
    const { data, isLoading } = usePublicQuestions(filters.query);
    const { data: subjects } = useActiveSubjects();
    const duplicate = useDuplicateQuestion();

    const [previewId, setPreviewId] = useState<string | null>(null);
    const { data: preview, isLoading: previewLoading } = usePublicQuestion(previewId ?? undefined);

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
                onDuplicate: (q) =>
                    duplicate.mutate(q.publicId, {
                        onSuccess: (res) => {
                            toast.success(t("toast.duplicated"));
                            navigate(`/teacher/question-bank/${res.publicId}`);
                        },
                        onError: () => toast.error(t("toast.error")),
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
                    to="/teacher/question-bank"
                    className="text-muted-foreground hover:text-foreground mb-2 inline-flex items-center gap-1.5 text-sm"
                >
                    <ArrowLeft className="size-4" />
                    {t("actions.back")}
                </Link>
                <h2 className="text-xl font-semibold">{t("public.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("public.subtitle")}</p>
            </div>

            <QuestionFilters
                state={filters.state}
                subjects={subjects?.items ?? []}
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
