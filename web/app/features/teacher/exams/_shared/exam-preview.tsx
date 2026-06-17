import { AlertTriangle } from "lucide-react";
import { useTranslation } from "react-i18next";

import { MarkdownContent } from "@/components/shared/markdown/markdown-content";
import { Badge } from "@/components/ui/badge";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import type { ExamPreview } from "@/services/exam/dtos/queries/exam-detail";

interface ExamPreviewDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    exam: ExamPreview | undefined;
    isLoading: boolean;
}

/** Read-only preview of an exam as a student sees it — ordered questions, options, no answers. */
export function ExamPreviewDialog({ open, onOpenChange, exam, isLoading }: ExamPreviewDialogProps) {
    const { t } = useTranslation("exam");

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-h-[88vh] overflow-y-auto sm:max-w-2xl">
                <DialogHeader>
                    <DialogTitle>{exam?.title ?? t("preview.title")}</DialogTitle>
                    <DialogDescription>{t("preview.subtitle")}</DialogDescription>
                </DialogHeader>

                {isLoading || !exam ? (
                    <div className="space-y-3">
                        <Skeleton className="h-6 w-3/4" />
                        <Skeleton className="h-20 w-full" />
                    </div>
                ) : (
                    <div className="space-y-5">
                        <div className="flex flex-wrap items-center gap-2">
                            <Badge variant="outline">
                                {t("preview.totalPoint", { points: exam.totalPoint })}
                            </Badge>
                            {exam.description ? (
                                <span className="text-muted-foreground text-sm">
                                    {exam.description}
                                </span>
                            ) : null}
                        </div>

                        {exam.questions.length === 0 ? (
                            <p className="text-muted-foreground py-6 text-center text-sm">
                                {t("preview.empty")}
                            </p>
                        ) : (
                            <ol className="space-y-5">
                                {exam.questions.map((q) => (
                                    <li key={q.displayOrder} className="space-y-2">
                                        <div className="flex items-center justify-between gap-2">
                                            <span className="text-sm font-semibold">
                                                {t("preview.questionLabel", {
                                                    order: q.displayOrder,
                                                })}
                                            </span>
                                            <Badge variant="outline">
                                                {t("preview.points", { points: q.point })}
                                            </Badge>
                                        </div>

                                        {q.isAvailable ? (
                                            <>
                                                <MarkdownContent>{q.content}</MarkdownContent>
                                                {q.options.length > 0 ? (
                                                    <ul className="space-y-2">
                                                        {q.options.map((o) => (
                                                            <li
                                                                key={o.displayOrder}
                                                                className="rounded-md border px-3 py-2 text-sm"
                                                            >
                                                                {o.content}
                                                            </li>
                                                        ))}
                                                    </ul>
                                                ) : (
                                                    <p className="text-muted-foreground text-sm italic">
                                                        {t("preview.writingHint")}
                                                    </p>
                                                )}
                                            </>
                                        ) : (
                                            <div className="flex items-center gap-2 rounded-md border border-amber-500/50 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
                                                <AlertTriangle className="size-4" />
                                                {t("preview.unavailable")}
                                            </div>
                                        )}
                                    </li>
                                ))}
                            </ol>
                        )}
                    </div>
                )}
            </DialogContent>
        </Dialog>
    );
}
