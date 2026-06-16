import { Check } from "lucide-react";
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
import { cn } from "@/lib/utils";
import type { QuestionDetail } from "@/services/question/dtos/queries/question-detail";

interface QuestionPreviewDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    question: QuestionDetail | undefined;
    isLoading: boolean;
    /** When true, marks the correct options (teacher/owner view). Student preview hides them. */
    showAnswers?: boolean;
}

/** Read-only preview of a question — content (Markdown), options, and optional answer highlighting. */
export function QuestionPreviewDialog({
    open,
    onOpenChange,
    question,
    isLoading,
    showAnswers = true,
}: QuestionPreviewDialogProps) {
    const { t } = useTranslation("question");

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-2xl">
                <DialogHeader>
                    <DialogTitle>{t("preview.title")}</DialogTitle>
                    <DialogDescription>{t("preview.subtitle")}</DialogDescription>
                </DialogHeader>

                {isLoading || !question ? (
                    <div className="space-y-3">
                        <Skeleton className="h-6 w-3/4" />
                        <Skeleton className="h-20 w-full" />
                    </div>
                ) : (
                    <div className="space-y-4">
                        <div className="flex flex-wrap gap-1.5">
                            <Badge variant="outline">{t(`types.${question.type}`)}</Badge>
                            <Badge variant="secondary">
                                {t(`difficulty.${question.difficulty}`)}
                            </Badge>
                            <Badge variant="outline">
                                {t("preview.points", { points: question.suggestedPoint })}
                            </Badge>
                        </div>

                        <MarkdownContent>{question.content}</MarkdownContent>

                        {question.options.length > 0 ? (
                            <ul className="space-y-2">
                                {question.options.map((o) => (
                                    <li
                                        key={o.displayOrder}
                                        className={cn(
                                            "flex items-center justify-between gap-2 rounded-md border px-3 py-2 text-sm",
                                            showAnswers && o.isCorrect
                                                ? "border-emerald-500/50 bg-emerald-500/10"
                                                : "",
                                        )}
                                    >
                                        <span>{o.content}</span>
                                        {showAnswers && o.isCorrect ? (
                                            <Check className="size-4 text-emerald-600" />
                                        ) : null}
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <p className="text-muted-foreground text-sm italic">
                                {t("preview.writingHint")}
                            </p>
                        )}

                        {showAnswers && question.explanation ? (
                            <div className="bg-muted/40 rounded-md p-3">
                                <p className="mb-1 text-xs font-medium">
                                    {t("preview.explanation")}
                                </p>
                                <MarkdownContent>{question.explanation}</MarkdownContent>
                            </div>
                        ) : null}

                        {question.tags.length > 0 ? (
                            <div className="flex flex-wrap gap-1.5">
                                {question.tags.map((tag) => (
                                    <Badge key={tag} variant="outline">
                                        #{tag}
                                    </Badge>
                                ))}
                            </div>
                        ) : null}
                    </div>
                )}
            </DialogContent>
        </Dialog>
    );
}
