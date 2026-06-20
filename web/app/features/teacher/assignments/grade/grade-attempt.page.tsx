import { ArrowLeft, Loader2, Save } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";

import { MarkdownContent } from "@/components/shared/markdown/markdown-content";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { ApiError } from "@/lib/api-error";
import type {
    AttemptGrading,
    GradingQuestion,
} from "@/services/assignment/dtos/queries/assignment-detail";
import { useAttemptGrading, useGradeAttempt } from "../_shared/grading.hook";
import type { Route } from "./+types/grade-attempt.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Grade attempt · Class Management" }];
}

type GradeState = Record<string, { score: string; feedback: string }>;

export default function GradeAttemptPage() {
    const { t } = useTranslation("assignment");
    const navigate = useNavigate();
    const { publicId, attemptId } = useParams();
    const { data, isLoading } = useAttemptGrading(attemptId);

    if (isLoading || !data) {
        return <Skeleton className="h-96 w-full" />;
    }

    return (
        <GradeAttemptView
            grading={data}
            attemptId={attemptId!}
            onBack={() => navigate(`/teacher/assignments/${publicId}`)}
            t={t}
        />
    );
}

function GradeAttemptView({
    grading,
    attemptId,
    onBack,
    t,
}: {
    grading: AttemptGrading;
    attemptId: string;
    onBack: () => void;
    t: (k: string, o?: Record<string, unknown>) => string;
}) {
    const writingQuestions = useMemo(
        () => grading.questions.filter((q) => q.isWriting),
        [grading.questions],
    );

    const [grades, setGrades] = useState<GradeState>(() => {
        const initial: GradeState = {};
        for (const q of writingQuestions) {
            initial[q.questionPublicId] = {
                score: q.manualScore != null ? String(q.manualScore) : "",
                feedback: q.feedback ?? "",
            };
        }
        return initial;
    });

    const gradeMutation = useGradeAttempt(attemptId);

    const setScore = (id: string, score: string) =>
        setGrades((g) => ({ ...g, [id]: { ...g[id], score } }));
    const setFeedback = (id: string, feedback: string) =>
        setGrades((g) => ({ ...g, [id]: { ...g[id], feedback } }));

    // Sum of entered manual scores + the objective auto scores → running total.
    const autoTotal = grading.questions
        .filter((q) => !q.isWriting)
        .reduce((sum, q) => sum + (q.autoScore ?? 0), 0);
    const manualTotal = writingQuestions.reduce((sum, q) => {
        const v = Number(grades[q.questionPublicId]?.score);
        return sum + (Number.isFinite(v) ? v : 0);
    }, 0);
    const pendingCount = writingQuestions.filter(
        (q) => grades[q.questionPublicId]?.score.trim() === "",
    ).length;

    const onSave = () => {
        // Validate each entered score is within [0, point].
        const payload: { questionPublicId: string; score: number; feedback: string | null }[] = [];
        for (const q of writingQuestions) {
            const entry = grades[q.questionPublicId];
            if (entry.score.trim() === "") continue;
            const score = Number(entry.score);
            if (!Number.isFinite(score) || score < 0 || score > q.point) {
                toast.error(t("grading.scoreRange", { max: q.point }));
                return;
            }
            payload.push({
                questionPublicId: q.questionPublicId,
                score,
                feedback: entry.feedback.trim() === "" ? null : entry.feedback,
            });
        }

        if (payload.length === 0) return;

        gradeMutation.mutate(
            { grades: payload },
            {
                onSuccess: () => toast.success(t("grading.saved")),
                onError: (err) =>
                    toast.error(err instanceof ApiError ? err.message : t("toast.error")),
            },
        );
    };

    return (
        <div className="mx-auto max-w-3xl space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex items-center gap-3">
                    <Button variant="ghost" size="icon" onClick={onBack}>
                        <ArrowLeft className="size-4" />
                    </Button>
                    <div>
                        <h2 className="text-xl font-semibold">{t("grading.title")}</h2>
                        <p className="text-muted-foreground text-sm">
                            {grading.studentName} ·{" "}
                            {t("grading.attempt", { number: grading.attemptNumber })}
                        </p>
                    </div>
                </div>
                <Badge variant="outline">{t(`attemptStatus.${grading.status}`)}</Badge>
            </div>

            {grading.questions.length === 0 ? (
                <p className="text-muted-foreground py-8 text-center text-sm">
                    {t("grading.empty")}
                </p>
            ) : (
                grading.questions.map((q) => (
                    <QuestionCard
                        key={q.questionPublicId}
                        q={q}
                        state={grades[q.questionPublicId]}
                        onScore={(v) => setScore(q.questionPublicId, v)}
                        onFeedback={(v) => setFeedback(q.questionPublicId, v)}
                        t={t}
                    />
                ))
            )}

            <Card>
                <CardContent className="flex flex-wrap items-center justify-between gap-3 py-4">
                    <div className="text-sm">
                        <span className="text-muted-foreground">{t("grading.total")}: </span>
                        <span className="font-semibold tabular-nums">
                            {autoTotal + manualTotal}
                            {grading.totalPoint != null ? ` / ${grading.totalPoint}` : ""}
                        </span>
                        <p className="text-muted-foreground mt-1 text-xs">
                            {pendingCount === 0
                                ? t("grading.allGraded")
                                : t("grading.pending", { count: pendingCount })}
                        </p>
                    </div>
                    <Button onClick={onSave} disabled={gradeMutation.isPending}>
                        {gradeMutation.isPending ? (
                            <Loader2 className="size-4 animate-spin" />
                        ) : (
                            <Save className="size-4" />
                        )}
                        {t("grading.save")}
                    </Button>
                </CardContent>
            </Card>
        </div>
    );
}

function QuestionCard({
    q,
    state,
    onScore,
    onFeedback,
    t,
}: {
    q: GradingQuestion;
    state: { score: string; feedback: string } | undefined;
    onScore: (v: string) => void;
    onFeedback: (v: string) => void;
    t: (k: string, o?: Record<string, unknown>) => string;
}) {
    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center justify-between text-base">
                    <span>
                        {q.displayPosition}. {q.type}
                    </span>
                    <span className="text-muted-foreground text-sm font-normal">
                        {t("grading.outOf", { point: q.point })}
                    </span>
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
                <MarkdownContent>{q.content}</MarkdownContent>

                {q.isWriting ? (
                    <>
                        <div className="rounded-md border bg-muted/30 p-3">
                            <p className="text-muted-foreground mb-1 text-xs">
                                {t("grading.writing")}
                            </p>
                            {q.textAnswer && q.textAnswer.trim() !== "" ? (
                                <MarkdownContent>{q.textAnswer}</MarkdownContent>
                            ) : (
                                <p className="text-muted-foreground text-sm italic">
                                    {t("grading.noAnswer")}
                                </p>
                            )}
                        </div>
                        <div className="grid gap-3 sm:grid-cols-[140px_1fr]">
                            <div className="space-y-1">
                                <Label htmlFor={`score-${q.questionPublicId}`}>
                                    {t("grading.score")}
                                </Label>
                                <Input
                                    id={`score-${q.questionPublicId}`}
                                    type="number"
                                    min={0}
                                    max={q.point}
                                    step="0.5"
                                    value={state?.score ?? ""}
                                    onChange={(e) => onScore(e.target.value)}
                                />
                            </div>
                            <div className="space-y-1">
                                <Label htmlFor={`fb-${q.questionPublicId}`}>
                                    {t("grading.feedback")}
                                </Label>
                                <Textarea
                                    id={`fb-${q.questionPublicId}`}
                                    rows={2}
                                    placeholder={t("grading.feedbackPlaceholder")}
                                    value={state?.feedback ?? ""}
                                    onChange={(e) => onFeedback(e.target.value)}
                                />
                            </div>
                        </div>
                    </>
                ) : (
                    <div className="space-y-2">
                        <ul className="space-y-1 text-sm">
                            {q.options.map((o) => (
                                <li
                                    key={o.publicId}
                                    className={`flex items-center gap-2 ${
                                        o.isCorrect ? "text-green-600" : ""
                                    }`}
                                >
                                    <span>• {o.content}</span>
                                    {o.isCorrect ? (
                                        <Badge variant="outline" className="text-green-600">
                                            {t("grading.correct")}
                                        </Badge>
                                    ) : null}
                                    {o.isSelected ? (
                                        <Badge variant="secondary">{t("grading.selected")}</Badge>
                                    ) : null}
                                </li>
                            ))}
                        </ul>
                        <p className="text-muted-foreground text-xs">
                            {t("grading.autoGraded")}: {q.autoScore ?? 0} / {q.point}
                        </p>
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
