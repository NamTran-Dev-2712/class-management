import { ArrowLeft, Loader2, Archive, BarChart3, Lock, Megaphone, Send } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { MarkdownContent } from "@/components/shared/markdown/markdown-content";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { useTableParams } from "@/hooks/use-table-params";
import { ApiError } from "@/lib/api-error";
import type { AttemptStatus } from "@/services/assignment/dtos/queries/assignment-list";
import { AssignmentForm } from "../_shared/assignment-form";
import { AssignmentStatusBadge } from "../_shared/assignment-status-badge";
import {
    useArchiveAssignment,
    useAssignmentAttempts,
    useCloseAssignment,
    usePublishAssignment,
    useTeacherAssignment,
} from "../_shared/assignments.hook";
import { useAttemptModeration } from "../_shared/grading.hook";
import { usePublishGrades } from "../_shared/grading.hook";
import type { Route } from "./+types/assignment-edit.page";

const GRADABLE_STATUSES: AttemptStatus[] = [
    "NeedManualGrading",
    "Graded",
    "AutoGraded",
    "Submitted",
];

export function meta(_: Route.MetaArgs) {
    return [{ title: "Assignment · Class Management" }];
}

export default function AssignmentEditPage() {
    const { t, i18n } = useTranslation("assignment");
    const navigate = useNavigate();
    const { publicId } = useParams();
    const { data: assignment, isLoading } = useTeacherAssignment(publicId);

    const publish = usePublishAssignment();
    const close = useCloseAssignment();
    const archive = useArchiveAssignment();
    const publishGrades = usePublishGrades();
    const [confirm, setConfirm] = useState<"close" | "archive" | "publishGrades" | null>(null);

    if (isLoading || !assignment) {
        return <Skeleton className="h-96 w-full" />;
    }

    const onError = (err: unknown) =>
        toast.error(err instanceof ApiError ? err.message : t("toast.error"));

    const isDraft = assignment.status === "Draft";
    const canClose = assignment.status === "Open" || assignment.status === "Scheduled";
    const canArchive = assignment.status !== "Archived";
    const canManageGrades = !isDraft;
    const canPublishGrades =
        canManageGrades &&
        assignment.gradePublishPolicy === "Manual" &&
        assignment.publishedAt != null;

    return (
        <div className="mx-auto max-w-4xl space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex items-center gap-3">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => navigate("/teacher/assignments")}
                    >
                        <ArrowLeft className="size-4" />
                    </Button>
                    <div>
                        <div className="flex items-center gap-2">
                            <h2 className="text-xl font-semibold">{assignment.title}</h2>
                            <AssignmentStatusBadge status={assignment.status} t={t} />
                        </div>
                        <p className="text-muted-foreground text-sm">
                            {assignment.className} · {assignment.examTitle}
                        </p>
                    </div>
                </div>
                <div className="flex gap-2">
                    {isDraft ? (
                        <Button
                            onClick={() =>
                                publish.mutate(assignment.publicId, {
                                    onSuccess: () => toast.success(t("toast.published")),
                                    onError,
                                })
                            }
                            disabled={publish.isPending}
                        >
                            {publish.isPending ? (
                                <Loader2 className="size-4 animate-spin" />
                            ) : (
                                <Send className="size-4" />
                            )}
                            {t("actions.publish")}
                        </Button>
                    ) : null}
                    {canClose ? (
                        <Button variant="outline" onClick={() => setConfirm("close")}>
                            <Lock className="size-4" />
                            {t("actions.close")}
                        </Button>
                    ) : null}
                    {canManageGrades ? (
                        <Button
                            variant="outline"
                            onClick={() =>
                                navigate(`/teacher/assignments/${assignment.publicId}/report`)
                            }
                        >
                            <BarChart3 className="size-4" />
                            {t("report.title")}
                        </Button>
                    ) : null}
                    {canPublishGrades ? (
                        <Button variant="outline" onClick={() => setConfirm("publishGrades")}>
                            <Megaphone className="size-4" />
                            {t("publishing.button")}
                        </Button>
                    ) : null}
                    {canArchive ? (
                        <Button variant="outline" onClick={() => setConfirm("archive")}>
                            <Archive className="size-4" />
                            {t("actions.archive")}
                        </Button>
                    ) : null}
                </div>
            </div>

            {isDraft ? (
                <AssignmentForm assignment={assignment} />
            ) : (
                <>
                    <PublishedSummary assignment={assignment} t={t} />
                    <SnapshotQuestions assignment={assignment} t={t} />
                    <AttemptsRoster
                        publicId={assignment.publicId}
                        locale={i18n.language}
                        onGrade={(attemptId) =>
                            navigate(
                                `/teacher/assignments/${assignment.publicId}/attempts/${attemptId}/grade`,
                            )
                        }
                        t={t}
                    />
                </>
            )}

            <ConfirmDialog
                open={confirm === "close"}
                onOpenChange={(open) => !open && setConfirm(null)}
                title={t("closeConfirm.title")}
                description={t("closeConfirm.description")}
                confirmLabel={t("actions.close")}
                isPending={close.isPending}
                onConfirm={() =>
                    close.mutate(assignment.publicId, {
                        onSuccess: () => {
                            toast.success(t("toast.closed"));
                            setConfirm(null);
                        },
                        onError,
                    })
                }
            />
            <ConfirmDialog
                open={confirm === "archive"}
                onOpenChange={(open) => !open && setConfirm(null)}
                title={t("archiveConfirm.title")}
                description={t("archiveConfirm.description")}
                confirmLabel={t("actions.archive")}
                isPending={archive.isPending}
                onConfirm={() =>
                    archive.mutate(assignment.publicId, {
                        onSuccess: () => {
                            toast.success(t("toast.archived"));
                            setConfirm(null);
                        },
                        onError,
                    })
                }
            />
            <ConfirmDialog
                open={confirm === "publishGrades"}
                onOpenChange={(open) => !open && setConfirm(null)}
                title={t("publishing.confirm.title")}
                description={t("publishing.confirm.description")}
                confirmLabel={t("publishing.confirm.action")}
                isPending={publishGrades.isPending}
                onConfirm={() =>
                    publishGrades.mutate(assignment.publicId, {
                        onSuccess: () => {
                            toast.success(t("publishing.published"));
                            setConfirm(null);
                        },
                        onError,
                    })
                }
            />
        </div>
    );
}

type Detail = NonNullable<ReturnType<typeof useTeacherAssignment>["data"]>;

function PublishedSummary({ assignment, t }: { assignment: Detail; t: (k: string) => string }) {
    return (
        <Card>
            <CardHeader>
                <CardTitle className="text-base">{t("summary.title")}</CardTitle>
            </CardHeader>
            <CardContent className="grid gap-3 text-sm sm:grid-cols-2">
                <Field
                    label={t("summary.totalQuestions")}
                    value={assignment.totalQuestions ?? "—"}
                />
                <Field label={t("summary.totalPoint")} value={assignment.totalPoint ?? "—"} />
                <Field label={t("summary.maxAttempts")} value={assignment.maxAttempts} />
                <Field label={t("summary.timeLimit")} value={assignment.timeLimitMinutes ?? "—"} />
                <Field
                    label={t("summary.submissions")}
                    value={`${assignment.submittedCount}/${assignment.attemptCount}`}
                />
            </CardContent>
        </Card>
    );
}

function Field({ label, value }: { label: string; value: string | number }) {
    return (
        <div className="flex justify-between gap-2 border-b py-1">
            <span className="text-muted-foreground">{label}</span>
            <span className="font-medium tabular-nums">{value}</span>
        </div>
    );
}

function SnapshotQuestions({ assignment, t }: { assignment: Detail; t: (k: string) => string }) {
    if (assignment.questions.length === 0) return null;
    return (
        <Card>
            <CardHeader>
                <CardTitle className="text-base">{t("summary.questions")}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
                {assignment.questions.map((q) => (
                    <div key={q.publicId} className="rounded-md border p-3">
                        <div className="text-muted-foreground mb-1 flex justify-between text-xs">
                            <span>
                                {q.displayOrder}. {q.type}
                            </span>
                            <span>{q.point} pt</span>
                        </div>
                        <MarkdownContent>{q.content}</MarkdownContent>
                        {q.options.length > 0 ? (
                            <ul className="mt-2 space-y-1 text-sm">
                                {q.options.map((o) => (
                                    <li
                                        key={o.publicId}
                                        className={o.isCorrect ? "font-medium text-green-600" : ""}
                                    >
                                        • {o.content}
                                        {o.isCorrect ? " ✓" : ""}
                                    </li>
                                ))}
                            </ul>
                        ) : null}
                    </div>
                ))}
            </CardContent>
        </Card>
    );
}

function AttemptsRoster({
    publicId,
    locale,
    onGrade,
    t,
}: {
    publicId: string;
    locale: string;
    onGrade: (attemptId: string) => void;
    t: (k: string, o?: Record<string, unknown>) => string;
}) {
    const { params, setPage } = useTableParams({ pageSize: 20 });
    const [status, setStatus] = useState<AttemptStatus | "all">("all");
    const { data, isLoading } = useAssignmentAttempts(publicId, {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        status: status === "all" ? undefined : status,
    });
    const moderate = useAttemptModeration();
    const runModeration = (
        attemptId: string,
        action: "force-submit" | "flag" | "unflag" | "unlock",
    ) =>
        moderate.mutate(
            { attemptId, action },
            {
                onSuccess: () => toast.success(t(`proctoring.moderation.${action}Done`)),
                onError: (err) =>
                    toast.error(err instanceof ApiError ? err.message : t("toast.error")),
            },
        );
    const fmt = (iso: string | null) =>
        iso
            ? new Intl.DateTimeFormat(locale, { dateStyle: "short", timeStyle: "short" }).format(
                  new Date(iso),
              )
            : "—";

    return (
        <Card>
            <CardHeader className="flex-row items-center justify-between gap-3 space-y-0">
                <CardTitle className="text-base">{t("roster.title")}</CardTitle>
                <Select value={status} onValueChange={(v) => setStatus(v as AttemptStatus | "all")}>
                    <SelectTrigger className="w-44" size="sm">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">{t("roster.filterStatus")}</SelectItem>
                        {(["NeedManualGrading", "Graded", "AutoGraded", "Submitted"] as const).map(
                            (s) => (
                                <SelectItem key={s} value={s}>
                                    {t(`attemptStatus.${s}`)}
                                </SelectItem>
                            ),
                        )}
                    </SelectContent>
                </Select>
            </CardHeader>
            <CardContent>
                {isLoading ? (
                    <Skeleton className="h-32 w-full" />
                ) : !data || data.items.length === 0 ? (
                    <p className="text-muted-foreground py-8 text-center text-sm">
                        {t("roster.empty")}
                    </p>
                ) : (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>{t("roster.student")}</TableHead>
                                <TableHead>{t("roster.attempt")}</TableHead>
                                <TableHead>{t("roster.status")}</TableHead>
                                <TableHead>{t("roster.violations")}</TableHead>
                                <TableHead>{t("roster.submittedAt")}</TableHead>
                                <TableHead className="text-right">{t("roster.score")}</TableHead>
                                <TableHead className="text-right">{t("roster.actions")}</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {data.items.map((a) => (
                                <TableRow key={a.publicId}>
                                    <TableCell className="font-medium">{a.studentName}</TableCell>
                                    <TableCell>#{a.attemptNumber}</TableCell>
                                    <TableCell>
                                        <Badge variant="outline">
                                            {t(`attemptStatus.${a.status}`)}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1.5">
                                            <Badge
                                                variant={
                                                    a.violationCount > 0 ? "destructive" : "outline"
                                                }
                                            >
                                                {a.violationCount}
                                            </Badge>
                                            {a.isFlagged ? (
                                                <Badge variant="destructive">
                                                    {t("roster.flagged")}
                                                </Badge>
                                            ) : null}
                                            {a.isLocked ? (
                                                <Lock className="text-destructive size-3.5" />
                                            ) : null}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-muted-foreground">
                                        {fmt(a.submittedAt)}
                                    </TableCell>
                                    <TableCell className="text-right tabular-nums">
                                        {a.totalScore ?? "—"}
                                        {a.totalPoint != null ? ` / ${a.totalPoint}` : ""}
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex items-center justify-end gap-1">
                                            {a.status === "InProgress" ? (
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    disabled={moderate.isPending}
                                                    onClick={() =>
                                                        runModeration(a.publicId, "force-submit")
                                                    }
                                                >
                                                    {t("proctoring.moderation.forceSubmit")}
                                                </Button>
                                            ) : null}
                                            {a.isLocked ? (
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    disabled={moderate.isPending}
                                                    onClick={() =>
                                                        runModeration(a.publicId, "unlock")
                                                    }
                                                >
                                                    {t("proctoring.moderation.unlock")}
                                                </Button>
                                            ) : null}
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                disabled={moderate.isPending}
                                                onClick={() =>
                                                    runModeration(
                                                        a.publicId,
                                                        a.isFlagged ? "unflag" : "flag",
                                                    )
                                                }
                                            >
                                                {a.isFlagged
                                                    ? t("proctoring.moderation.unflag")
                                                    : t("proctoring.moderation.flag")}
                                            </Button>
                                            {GRADABLE_STATUSES.includes(a.status) ? (
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    onClick={() => onGrade(a.publicId)}
                                                >
                                                    {t("roster.grade")}
                                                </Button>
                                            ) : null}
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                )}
                {data && data.totalPages > 1 ? (
                    <div className="mt-3 flex justify-end gap-2">
                        <Button
                            variant="outline"
                            size="sm"
                            disabled={!data.hasPreviousPage}
                            onClick={() => setPage(data.pageNumber - 1)}
                        >
                            {t("roster.prev")}
                        </Button>
                        <Button
                            variant="outline"
                            size="sm"
                            disabled={!data.hasNextPage}
                            onClick={() => setPage(data.pageNumber + 1)}
                        >
                            {t("roster.next")}
                        </Button>
                    </div>
                ) : null}
            </CardContent>
        </Card>
    );
}
