import { AlertTriangle, Clock, Loader2, Send } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { MarkdownContent } from "@/components/shared/markdown/markdown-content";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { useCountdown } from "@/hooks/use-countdown";
import { ApiError } from "@/lib/api-error";
import type {
    AttemptAnswerResult,
    TakingQuestion,
} from "@/services/assignment/dtos/queries/assignment-detail";
import {
    useAttemptResult,
    useAttemptTaking,
    useMyAttempts,
    useSaveAttemptAnswers,
    useSubmitAttempt,
} from "../_shared/assignments.hook";
import type { Route } from "./+types/attempt.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Attempt · Class Management" }];
}

interface AnswerValue {
    selectedOptionIds: string[];
    textAnswer: string;
}
type AnswerState = Record<string, AnswerValue>;

const WRITING = new Set(["ShortWriting", "LongWriting"]);
const MULTI = "MultipleChoice";

export default function AttemptPage() {
    const { attemptId } = useParams();
    const { data: taking, isLoading, refetch } = useAttemptTaking(attemptId);

    if (isLoading || !taking) return <Skeleton className="h-96 w-full" />;

    if (taking.status !== "InProgress") {
        return <AttemptResultView attemptId={attemptId!} />;
    }

    return <TakingView key={taking.publicId} taking={taking} onSubmitted={() => refetch()} />;
}

function TakingView({
    taking,
    onSubmitted,
}: {
    taking: NonNullable<ReturnType<typeof useAttemptTaking>["data"]>;
    onSubmitted: () => void;
}) {
    const { t } = useTranslation("assignment");
    const save = useSaveAttemptAnswers();
    const submit = useSubmitAttempt();
    const [confirmOpen, setConfirmOpen] = useState(false);
    const submittedRef = useRef(false);

    const [answers, setAnswers] = useState<AnswerState>(() => {
        const init: AnswerState = {};
        for (const q of taking.questions) {
            init[q.publicId] = {
                selectedOptionIds: q.selectedOptionIds ?? [],
                textAnswer: q.textAnswer ?? "",
            };
        }
        return init;
    });

    const buildPayload = () => ({
        answers: taking.questions.map((q) => ({
            questionId: q.publicId,
            selectedOptionIds: answers[q.publicId]?.selectedOptionIds ?? [],
            textAnswer: answers[q.publicId]?.textAnswer || null,
        })),
    });

    const payloadRef = useRef(buildPayload);
    payloadRef.current = buildPayload;

    // Debounced auto-save: persist ~2.5s after the last change.
    const dirtyRef = useRef(false);
    useEffect(() => {
        if (!dirtyRef.current) return;
        const id = setTimeout(() => {
            save.mutate({ attemptId: taking.publicId, payload: payloadRef.current() });
            dirtyRef.current = false;
        }, 2500);
        return () => clearTimeout(id);
    }, [answers, save, taking.publicId]);

    const doSubmit = (auto: boolean) => {
        if (submittedRef.current) return;
        submittedRef.current = true;
        // Final save then submit.
        save.mutate(
            { attemptId: taking.publicId, payload: payloadRef.current() },
            {
                onSettled: () =>
                    submit.mutate(taking.publicId, {
                        onSuccess: () => {
                            toast.success(auto ? t("attempt.autoSubmitted") : t("toast.submitted"));
                            onSubmitted();
                        },
                        onError: (err) => {
                            submittedRef.current = false;
                            toast.error(err instanceof ApiError ? err.message : t("toast.error"));
                        },
                    }),
            },
        );
    };

    const { minutes, seconds, remaining } = useCountdown(taking.remainingSeconds, () =>
        doSubmit(true),
    );
    const lowTime = remaining !== null && remaining <= 300;

    const setAnswer = (qid: string, value: AnswerValue) => {
        dirtyRef.current = true;
        setAnswers((prev) => ({ ...prev, [qid]: value }));
    };

    return (
        <div className="mx-auto max-w-3xl space-y-6 pb-24">
            <div className="bg-background sticky top-0 z-10 flex items-center justify-between gap-4 border-b py-3">
                <div>
                    <h2 className="text-lg font-semibold">{taking.assignmentTitle}</h2>
                    <p className="text-muted-foreground text-xs">
                        {t("attempt.attemptNumber", { number: taking.attemptNumber })}
                    </p>
                </div>
                {minutes !== null ? (
                    <div
                        className={`flex items-center gap-2 rounded-md border px-3 py-1.5 font-mono text-sm ${
                            lowTime ? "border-destructive text-destructive" : ""
                        }`}
                    >
                        <Clock className="size-4" />
                        {String(minutes).padStart(2, "0")}:{String(seconds).padStart(2, "0")}
                    </div>
                ) : null}
            </div>

            {taking.questions.map((q) => (
                <QuestionCard
                    key={q.publicId}
                    question={q}
                    value={answers[q.publicId]}
                    onChange={(v) => setAnswer(q.publicId, v)}
                />
            ))}

            <div className="bg-background fixed inset-x-0 bottom-0 border-t p-4">
                <div className="mx-auto flex max-w-3xl items-center justify-between">
                    <span className="text-muted-foreground text-xs">
                        {save.isPending ? t("attempt.saving") : t("attempt.autoSaveHint")}
                    </span>
                    <Button onClick={() => setConfirmOpen(true)} disabled={submit.isPending}>
                        {submit.isPending ? (
                            <Loader2 className="size-4 animate-spin" />
                        ) : (
                            <Send className="size-4" />
                        )}
                        {t("attempt.submit")}
                    </Button>
                </div>
            </div>

            <ConfirmDialog
                open={confirmOpen}
                onOpenChange={setConfirmOpen}
                title={t("attempt.submitConfirm.title")}
                description={t("attempt.submitConfirm.description")}
                confirmLabel={t("attempt.submit")}
                isPending={submit.isPending}
                onConfirm={() => {
                    setConfirmOpen(false);
                    doSubmit(false);
                }}
            />
        </div>
    );
}

function QuestionCard({
    question,
    value,
    onChange,
}: {
    question: TakingQuestion;
    value: AnswerValue | undefined;
    onChange: (v: AnswerValue) => void;
}) {
    const { t } = useTranslation("assignment");
    const selected = value?.selectedOptionIds ?? [];
    const text = value?.textAnswer ?? "";

    const toggleSingle = (optionId: string) =>
        onChange({ selectedOptionIds: [optionId], textAnswer: "" });
    const toggleMulti = (optionId: string) => {
        const next = selected.includes(optionId)
            ? selected.filter((id) => id !== optionId)
            : [...selected, optionId];
        onChange({ selectedOptionIds: next, textAnswer: "" });
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-baseline justify-between gap-2 text-base">
                    <span>{t("attempt.questionLabel", { order: question.displayPosition })}</span>
                    <span className="text-muted-foreground text-xs font-normal">
                        {t("attempt.points", { count: question.point })}
                    </span>
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
                <MarkdownContent>{question.content}</MarkdownContent>

                {WRITING.has(question.type) ? (
                    <Textarea
                        rows={question.type === "LongWriting" ? 8 : 3}
                        value={text}
                        placeholder={t("attempt.answerPlaceholder")}
                        onChange={(e) =>
                            onChange({ selectedOptionIds: [], textAnswer: e.target.value })
                        }
                    />
                ) : (
                    <div className="space-y-2">
                        {question.options.map((o) => {
                            const checked = selected.includes(o.publicId);
                            const multi = question.type === MULTI;
                            return (
                                <label
                                    key={o.publicId}
                                    className={`flex cursor-pointer items-center gap-3 rounded-md border p-3 text-sm ${
                                        checked ? "border-primary bg-primary/5" : ""
                                    }`}
                                >
                                    <input
                                        type={multi ? "checkbox" : "radio"}
                                        name={question.publicId}
                                        checked={checked}
                                        onChange={() =>
                                            multi
                                                ? toggleMulti(o.publicId)
                                                : toggleSingle(o.publicId)
                                        }
                                        className="size-4"
                                    />
                                    <span>{o.content}</span>
                                </label>
                            );
                        })}
                    </div>
                )}
            </CardContent>
        </Card>
    );
}

function AttemptResultView({ attemptId }: { attemptId: string }) {
    const { t, i18n } = useTranslation("assignment");
    const navigate = useNavigate();
    const { data: result, isLoading } = useAttemptResult(attemptId);
    const { data: history } = useMyAttempts({ pageNumber: 1, pageSize: 50 }, !!result);

    if (isLoading || !result) return <Skeleton className="h-64 w-full" />;

    const sameAssignment =
        history?.items
            .filter((a) => a.assignmentPublicId === result.assignmentPublicId)
            .sort((a, b) => a.attemptNumber - b.attemptNumber) ?? [];

    const fmt = (iso: string | null) =>
        iso
            ? new Intl.DateTimeFormat(i18n.language, {
                  dateStyle: "short",
                  timeStyle: "short",
              }).format(new Date(iso))
            : "—";

    return (
        <div className="mx-auto max-w-2xl space-y-6">
            <h2 className="text-xl font-semibold">{result.assignmentTitle}</h2>

            <Card>
                <CardHeader>
                    <CardTitle className="text-base">{t("result.title")}</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3 text-sm">
                    {result.autoSubmitted ? (
                        <p className="text-destructive flex items-center gap-2">
                            <AlertTriangle className="size-4" />
                            {t("result.autoSubmitted")}
                        </p>
                    ) : null}
                    {result.scoreReleased ? (
                        <div className="text-2xl font-semibold tabular-nums">
                            {result.totalScore ?? "—"}
                            {result.totalPoint != null ? (
                                <span className="text-muted-foreground text-base">
                                    {" "}
                                    / {result.totalPoint}
                                </span>
                            ) : null}
                        </div>
                    ) : (
                        <p className="text-muted-foreground">{t("result.pending")}</p>
                    )}
                    {result.status === "NeedManualGrading" ? (
                        <p className="text-muted-foreground">{t("result.manualPending")}</p>
                    ) : null}
                </CardContent>
            </Card>

            {result.scoreReleased && result.answers.length > 0 ? (
                <Card>
                    <CardHeader>
                        <CardTitle className="text-base">{t("result.review")}</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        {!result.showAnswers ? (
                            <p className="text-muted-foreground text-xs">
                                {t("result.answersHidden")}
                            </p>
                        ) : null}
                        {result.answers.map((a) => (
                            <AnswerReview key={a.questionPublicId} answer={a} t={t} />
                        ))}
                    </CardContent>
                </Card>
            ) : null}

            {sameAssignment.length > 1 ? (
                <Card>
                    <CardHeader>
                        <CardTitle className="text-base">{t("result.history")}</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-2 text-sm">
                        {sameAssignment.map((a) => (
                            <div
                                key={a.publicId}
                                className="flex items-center justify-between border-b py-1 last:border-0"
                            >
                                <span>
                                    {t("attempt.attemptNumber", { number: a.attemptNumber })}
                                </span>
                                <span className="text-muted-foreground">{fmt(a.submittedAt)}</span>
                                <span className="font-medium tabular-nums">
                                    {a.totalScore ?? "—"}
                                    {a.totalPoint != null ? ` / ${a.totalPoint}` : ""}
                                </span>
                            </div>
                        ))}
                    </CardContent>
                </Card>
            ) : null}

            <Button variant="outline" onClick={() => navigate("/student/assignments")}>
                {t("result.backToList")}
            </Button>
        </div>
    );
}

function AnswerReview({
    answer,
    t,
}: {
    answer: AttemptAnswerResult;
    t: (k: string, o?: Record<string, unknown>) => string;
}) {
    const score = answer.isWriting ? answer.manualScore : answer.autoScore;
    return (
        <div className="rounded-md border p-3">
            <div className="text-muted-foreground mb-1 flex justify-between text-xs">
                <span>{answer.displayPosition}.</span>
                <span className="tabular-nums">
                    {t("result.score")}: {score ?? "—"} / {answer.point}
                </span>
            </div>
            <MarkdownContent>{answer.content}</MarkdownContent>

            {answer.isWriting ? (
                <div className="mt-2 space-y-2">
                    <div>
                        <p className="text-muted-foreground text-xs">{t("result.yourAnswer")}</p>
                        {answer.textAnswer && answer.textAnswer.trim() !== "" ? (
                            <MarkdownContent>{answer.textAnswer}</MarkdownContent>
                        ) : (
                            <p className="text-muted-foreground text-sm italic">
                                {t("result.noAnswer")}
                            </p>
                        )}
                    </div>
                    {answer.feedback ? (
                        <div className="bg-muted/30 rounded-md border p-2">
                            <p className="text-muted-foreground text-xs">{t("result.feedback")}</p>
                            <MarkdownContent>{answer.feedback}</MarkdownContent>
                        </div>
                    ) : null}
                </div>
            ) : (
                <ul className="mt-2 space-y-1 text-sm">
                    {answer.options.map((o) => (
                        <li
                            key={o.publicId}
                            className={`flex items-center gap-2 ${
                                o.isCorrect ? "text-green-600" : ""
                            }`}
                        >
                            <span>
                                {o.isSelected ? "☑" : "☐"} {o.content}
                            </span>
                            {o.isCorrect ? (
                                <span className="text-green-600 text-xs">
                                    ✓ {t("result.correctMark")}
                                </span>
                            ) : null}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
