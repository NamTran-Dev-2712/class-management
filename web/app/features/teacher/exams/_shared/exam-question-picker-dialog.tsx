import { Check, Plus } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import {
    FILTER_ALL,
    QuestionFilters,
    type QuestionFilterState,
} from "@/features/teacher/questions/_shared/question-filters";
import {
    useActiveSubjects,
    usePublicQuestions,
    useTeacherQuestions,
} from "@/features/teacher/questions/_shared/questions.hook";
import type {
    QuestionDifficulty,
    QuestionListItem,
    QuestionListQuery,
    QuestionType,
    QuestionVisibility,
} from "@/services/question/dtos/queries/question-list";

type Tab = "mine" | "public";

interface ExamQuestionPickerDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    /** Question public ids already in the exam (shown as disabled "already added"). */
    existingIds: Set<string>;
    onAdd: (questions: QuestionListItem[]) => void;
}

const emptyState: QuestionFilterState = {
    searchTerm: "",
    type: FILTER_ALL,
    difficulty: FILTER_ALL,
    subjectId: FILTER_ALL,
    visibility: FILTER_ALL,
    tag: "",
};

export function ExamQuestionPickerDialog({
    open,
    onOpenChange,
    existingIds,
    onAdd,
}: ExamQuestionPickerDialogProps) {
    const { t } = useTranslation("exam");
    const [tab, setTab] = useState<Tab>("mine");
    const [filter, setFilter] = useState<QuestionFilterState>(emptyState);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(8);
    const [staged, setStaged] = useState<Map<string, QuestionListItem>>(new Map());
    const { data: subjects } = useActiveSubjects();

    const toFacet = (v: string) => (v === FILTER_ALL ? undefined : v);
    const query: QuestionListQuery = {
        pageNumber: page,
        pageSize,
        sortBy: "updatedAt",
        sortOrder: "desc",
        searchTerm: filter.searchTerm || undefined,
        type: toFacet(filter.type) as QuestionType | undefined,
        difficulty: toFacet(filter.difficulty) as QuestionDifficulty | undefined,
        subjectId: toFacet(filter.subjectId),
        visibility: toFacet(filter.visibility) as QuestionVisibility | undefined,
        tag: filter.tag || undefined,
    };

    const mine = useTeacherQuestions(query);
    const pub = usePublicQuestions(query);
    const active = tab === "mine" ? mine : pub;
    const data = active.data;

    const resetFacet = (patch: Partial<QuestionFilterState>) => {
        setFilter((prev) => ({ ...prev, ...patch }));
        setPage(1);
    };

    const toggle = (q: QuestionListItem) =>
        setStaged((prev) => {
            const next = new Map(prev);
            if (next.has(q.publicId)) next.delete(q.publicId);
            else next.set(q.publicId, q);
            return next;
        });

    const close = (nextOpen: boolean) => {
        if (!nextOpen) {
            setStaged(new Map());
            setFilter(emptyState);
            setPage(1);
            setTab("mine");
        }
        onOpenChange(nextOpen);
    };

    const confirm = () => {
        onAdd(Array.from(staged.values()));
        close(false);
    };

    const switchTab = (next: Tab) => {
        setTab(next);
        setPage(1);
    };

    const stagedCount = staged.size;
    const items = data?.items ?? [];

    const tabButtons = useMemo(
        () =>
            (["mine", "public"] as const).map((key) => (
                <Button
                    key={key}
                    type="button"
                    size="sm"
                    variant={tab === key ? "default" : "outline"}
                    onClick={() => switchTab(key)}
                >
                    {key === "mine" ? t("builder.picker.tabMine") : t("builder.picker.tabPublic")}
                </Button>
            )),
        [tab, t],
    );

    return (
        <Dialog open={open} onOpenChange={close}>
            <DialogContent className="flex max-h-[88vh] flex-col sm:max-w-2xl">
                <DialogHeader>
                    <DialogTitle>{t("builder.picker.title")}</DialogTitle>
                    <DialogDescription>{t("builder.picker.subtitle")}</DialogDescription>
                </DialogHeader>

                <div className="flex gap-2">{tabButtons}</div>

                <QuestionFilters
                    state={filter}
                    subjects={subjects?.items ?? []}
                    showVisibility={tab === "mine"}
                    onSearchChange={(v) => resetFacet({ searchTerm: v })}
                    onTagChange={(v) => resetFacet({ tag: v })}
                    onTypeChange={(v) => resetFacet({ type: v })}
                    onDifficultyChange={(v) => resetFacet({ difficulty: v })}
                    onSubjectChange={(v) => resetFacet({ subjectId: v })}
                    onVisibilityChange={(v) => resetFacet({ visibility: v })}
                />

                <div className="min-h-0 flex-1 space-y-2 overflow-y-auto pr-1">
                    {active.isLoading ? (
                        <div className="space-y-2">
                            <Skeleton className="h-14 w-full" />
                            <Skeleton className="h-14 w-full" />
                            <Skeleton className="h-14 w-full" />
                        </div>
                    ) : items.length === 0 ? (
                        <p className="text-muted-foreground py-10 text-center text-sm">
                            {t("builder.picker.empty")}
                        </p>
                    ) : (
                        items.map((q) => {
                            const alreadyIn = existingIds.has(q.publicId);
                            const isStaged = staged.has(q.publicId);
                            return (
                                <div
                                    key={q.publicId}
                                    className={cn(
                                        "flex items-center gap-3 rounded-md border p-2.5",
                                        isStaged ? "border-primary bg-primary/5" : "",
                                    )}
                                >
                                    <div className="min-w-0 flex-1">
                                        <p className="line-clamp-2 text-sm font-medium">
                                            {q.content}
                                        </p>
                                        <div className="mt-1 flex flex-wrap gap-1">
                                            <Badge variant="outline" className="text-xs">
                                                {t(`question:types.${q.type}`)}
                                            </Badge>
                                            <Badge variant="secondary" className="text-xs">
                                                {t(`question:difficulty.${q.difficulty}`)}
                                            </Badge>
                                            {q.subjectName ? (
                                                <span className="text-muted-foreground text-xs">
                                                    {q.subjectName}
                                                </span>
                                            ) : null}
                                        </div>
                                    </div>
                                    {alreadyIn ? (
                                        <Badge variant="secondary">
                                            {t("builder.picker.alreadyIn")}
                                        </Badge>
                                    ) : (
                                        <Button
                                            type="button"
                                            size="sm"
                                            variant={isStaged ? "default" : "outline"}
                                            onClick={() => toggle(q)}
                                        >
                                            {isStaged ? (
                                                <>
                                                    <Check className="size-4" />
                                                    {t("builder.picker.added")}
                                                </>
                                            ) : (
                                                <>
                                                    <Plus className="size-4" />
                                                    {t("builder.picker.add")}
                                                </>
                                            )}
                                        </Button>
                                    )}
                                </div>
                            );
                        })
                    )}
                </div>

                {data && data.items.length > 0 ? (
                    <DataTablePagination
                        pageNumber={data.pageNumber}
                        pageSize={data.pageSize}
                        totalPages={data.totalPages}
                        totalCount={data.totalCount}
                        hasPreviousPage={data.hasPreviousPage}
                        hasNextPage={data.hasNextPage}
                        onPageChange={setPage}
                        onPageSizeChange={(s) => {
                            setPageSize(s);
                            setPage(1);
                        }}
                    />
                ) : null}

                <DialogFooter className="flex-row items-center justify-between gap-2 sm:justify-between">
                    <span className="text-muted-foreground text-sm">
                        {t("builder.picker.selectedCount", { count: stagedCount })}
                    </span>
                    <Button type="button" onClick={confirm} disabled={stagedCount === 0}>
                        {t("builder.picker.done")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
