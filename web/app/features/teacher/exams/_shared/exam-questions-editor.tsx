import {
    DndContext,
    KeyboardSensor,
    PointerSensor,
    closestCenter,
    useSensor,
    useSensors,
    type DragEndEvent,
} from "@dnd-kit/core";
import {
    SortableContext,
    arrayMove,
    sortableKeyboardCoordinates,
    useSortable,
    verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import {
    AlertTriangle,
    ChevronDown,
    ChevronUp,
    GripVertical,
    Loader2,
    Plus,
    Trash2,
} from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { ApiError } from "@/lib/api-error";
import type { QuestionListItem } from "@/services/question/dtos/queries/question-list";
import type { ExamDetail, ExamQuestionItem } from "@/services/exam/dtos/queries/exam-detail";
import { useUpdateExamQuestions } from "./exams.hook";
import { ExamQuestionPickerDialog } from "./exam-question-picker-dialog";

interface EditorRow {
    uid: string;
    questionPublicId: string;
    point: number;
    isAvailable: boolean;
    type: string;
    content: string;
    difficulty: string;
}

let uidCounter = 0;
const nextUid = () => `row-${uidCounter++}`;

function toRow(q: ExamQuestionItem): EditorRow {
    return {
        uid: nextUid(),
        questionPublicId: q.questionPublicId,
        point: q.point,
        isAvailable: q.isAvailable,
        type: q.type,
        content: q.content,
        difficulty: q.difficulty,
    };
}

function fromPicked(q: QuestionListItem): EditorRow {
    return {
        uid: nextUid(),
        questionPublicId: q.publicId,
        point: q.suggestedPoint > 0 ? q.suggestedPoint : 1,
        isAvailable: true,
        type: q.type,
        content: q.content,
        difficulty: q.difficulty,
    };
}

interface ExamQuestionsEditorProps {
    exam: ExamDetail;
}

export function ExamQuestionsEditor({ exam }: ExamQuestionsEditorProps) {
    const { t } = useTranslation("exam");
    const save = useUpdateExamQuestions();
    const [rows, setRows] = useState<EditorRow[]>(() => exam.questions.map(toRow));
    const [pickerOpen, setPickerOpen] = useState(false);

    const sensors = useSensors(
        useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
        useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
    );

    const totalPoint = useMemo(
        () => rows.reduce((sum, r) => sum + (Number.isFinite(r.point) ? r.point : 0), 0),
        [rows],
    );
    const existingIds = useMemo(
        () => new Set(rows.filter((r) => r.isAvailable).map((r) => r.questionPublicId)),
        [rows],
    );
    const hasUnavailable = rows.some((r) => !r.isAvailable);

    const handleDragEnd = (event: DragEndEvent) => {
        const { active, over } = event;
        if (!over || active.id === over.id) return;
        setRows((prev) => {
            const from = prev.findIndex((r) => r.uid === active.id);
            const to = prev.findIndex((r) => r.uid === over.id);
            if (from < 0 || to < 0) return prev;
            return arrayMove(prev, from, to);
        });
    };

    const move = (index: number, dir: -1 | 1) => {
        setRows((prev) => {
            const to = index + dir;
            if (to < 0 || to >= prev.length) return prev;
            return arrayMove(prev, index, to);
        });
    };

    const setPoint = (uid: string, value: number) =>
        setRows((prev) => prev.map((r) => (r.uid === uid ? { ...r, point: value } : r)));

    const remove = (uid: string) => setRows((prev) => prev.filter((r) => r.uid !== uid));

    const addPicked = (picked: QuestionListItem[]) => {
        setRows((prev) => {
            const present = new Set(prev.map((r) => r.questionPublicId));
            const additions = picked.filter((q) => !present.has(q.publicId)).map(fromPicked);
            return [...prev, ...additions];
        });
    };

    const handleSave = () => {
        // Only available questions are persisted; dangling (soft-deleted) references are dropped.
        const payload = {
            questions: rows
                .filter((r) => r.isAvailable && r.questionPublicId)
                .map((r) => ({ questionId: r.questionPublicId, point: r.point })),
        };
        save.mutate(
            { publicId: exam.publicId, payload },
            {
                onSuccess: () => toast.success(t("toast.questionsSaved")),
                onError: (error) =>
                    toast.error(error instanceof ApiError ? error.message : t("toast.error")),
            },
        );
    };

    return (
        <Card>
            <CardHeader className="flex-row items-center justify-between gap-4 space-y-0">
                <div>
                    <CardTitle className="text-base">{t("builder.title")}</CardTitle>
                    <p className="text-muted-foreground text-sm">{t("builder.subtitle")}</p>
                </div>
                <Button type="button" variant="outline" onClick={() => setPickerOpen(true)}>
                    <Plus className="size-4" />
                    <span className="hidden sm:inline">{t("builder.addQuestions")}</span>
                </Button>
            </CardHeader>

            <CardContent className="space-y-4">
                {hasUnavailable ? (
                    <div className="flex items-center gap-2 rounded-md border border-amber-500/50 bg-amber-500/10 px-3 py-2 text-sm">
                        <AlertTriangle className="size-4 shrink-0 text-amber-600" />
                        <span>{t("builder.unavailable")}</span>
                    </div>
                ) : null}

                {rows.length === 0 ? (
                    <p className="text-muted-foreground rounded-md border border-dashed py-10 text-center text-sm">
                        {t("builder.empty")}
                    </p>
                ) : (
                    <DndContext
                        sensors={sensors}
                        collisionDetection={closestCenter}
                        onDragEnd={handleDragEnd}
                    >
                        <SortableContext
                            items={rows.map((r) => r.uid)}
                            strategy={verticalListSortingStrategy}
                        >
                            <ol className="space-y-2">
                                {rows.map((row, index) => (
                                    <SortableRow
                                        key={row.uid}
                                        row={row}
                                        index={index}
                                        total={rows.length}
                                        onMove={move}
                                        onPoint={setPoint}
                                        onRemove={remove}
                                    />
                                ))}
                            </ol>
                        </SortableContext>
                    </DndContext>
                )}

                <div className="flex flex-wrap items-center justify-between gap-3 border-t pt-4">
                    <div className="flex gap-4 text-sm">
                        <span>
                            {t("builder.totalQuestions")}:{" "}
                            <span className="font-semibold tabular-nums">{rows.length}</span>
                        </span>
                        <span>
                            {t("builder.totalPoint")}:{" "}
                            <span className="font-semibold tabular-nums">{totalPoint}</span>
                        </span>
                    </div>
                    <Button type="button" onClick={handleSave} disabled={save.isPending}>
                        {save.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                        {t("builder.save")}
                    </Button>
                </div>
            </CardContent>

            <ExamQuestionPickerDialog
                open={pickerOpen}
                onOpenChange={setPickerOpen}
                existingIds={existingIds}
                onAdd={addPicked}
            />
        </Card>
    );
}

interface SortableRowProps {
    row: EditorRow;
    index: number;
    total: number;
    onMove: (index: number, dir: -1 | 1) => void;
    onPoint: (uid: string, value: number) => void;
    onRemove: (uid: string) => void;
}

function SortableRow({ row, index, total, onMove, onPoint, onRemove }: SortableRowProps) {
    const { t } = useTranslation("exam");
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
        id: row.uid,
    });

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.6 : 1,
    };

    return (
        <li
            ref={setNodeRef}
            style={style}
            className="bg-card flex items-center gap-2 rounded-md border p-2 sm:gap-3 sm:p-3"
        >
            <button
                type="button"
                className="text-muted-foreground hover:text-foreground hidden cursor-grab touch-none sm:block"
                aria-label={t("builder.dragHandle")}
                {...attributes}
                {...listeners}
            >
                <GripVertical className="size-4" />
            </button>

            <div className="flex flex-col sm:hidden">
                <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="size-6"
                    disabled={index === 0}
                    onClick={() => onMove(index, -1)}
                    aria-label={t("builder.moveUp")}
                >
                    <ChevronUp className="size-4" />
                </Button>
                <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="size-6"
                    disabled={index === total - 1}
                    onClick={() => onMove(index, 1)}
                    aria-label={t("builder.moveDown")}
                >
                    <ChevronDown className="size-4" />
                </Button>
            </div>

            <span className="text-muted-foreground w-5 text-center text-sm tabular-nums">
                {index + 1}
            </span>

            <div className="min-w-0 flex-1">
                <p className="line-clamp-2 text-sm font-medium">
                    {row.isAvailable ? (
                        row.content
                    ) : (
                        <span className="text-amber-600">{t("preview.unavailable")}</span>
                    )}
                </p>
                {row.isAvailable ? (
                    <div className="mt-1 flex flex-wrap gap-1">
                        <Badge variant="outline" className="text-xs">
                            {t(`question:types.${row.type}`)}
                        </Badge>
                        <Badge variant="secondary" className="text-xs">
                            {t(`question:difficulty.${row.difficulty}`)}
                        </Badge>
                    </div>
                ) : null}
            </div>

            <div className="flex items-center gap-1">
                <Input
                    type="number"
                    min={0.5}
                    max={100}
                    step={0.5}
                    value={row.point}
                    onChange={(e) => onPoint(row.uid, e.target.valueAsNumber)}
                    className="h-8 w-20"
                    aria-label={t("builder.point")}
                    disabled={!row.isAvailable}
                />
                <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="size-8 text-destructive"
                    onClick={() => onRemove(row.uid)}
                    aria-label={t("builder.remove")}
                >
                    <Trash2 className="size-4" />
                </Button>
            </div>
        </li>
    );
}
