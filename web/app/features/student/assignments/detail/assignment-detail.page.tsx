import { ArrowLeft, Loader2, Play } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ApiError } from "@/lib/api-error";
import { AssignmentStatusBadge } from "@/features/teacher/assignments/_shared/assignment-status-badge";
import { useStartAttempt, useStudentAssignment } from "../_shared/assignments.hook";
import type { Route } from "./+types/assignment-detail.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Assignment · Class Management" }];
}

export default function StudentAssignmentDetailPage() {
    const { t, i18n } = useTranslation("assignment");
    const navigate = useNavigate();
    const { publicId } = useParams();
    const { data: a, isLoading } = useStudentAssignment(publicId);
    const start = useStartAttempt();

    if (isLoading || !a) return <Skeleton className="h-80 w-full" />;

    const fmt = (iso: string | null) =>
        iso
            ? new Intl.DateTimeFormat(i18n.language, {
                  dateStyle: "medium",
                  timeStyle: "short",
              }).format(new Date(iso))
            : "—";

    const goToAttempt = (attemptId: string) =>
        navigate(`/student/assignments/attempts/${attemptId}`);

    const onStart = () =>
        start.mutate(a.publicId, {
            onSuccess: (res) => goToAttempt(res.attemptId),
            onError: (err) => toast.error(err instanceof ApiError ? err.message : t("toast.error")),
        });

    return (
        <div className="mx-auto max-w-2xl space-y-6">
            <div className="flex items-center gap-3">
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigate("/student/assignments")}
                >
                    <ArrowLeft className="size-4" />
                </Button>
                <div className="flex items-center gap-2">
                    <h2 className="text-xl font-semibold">{a.title}</h2>
                    <AssignmentStatusBadge status={a.status} t={t} />
                </div>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle className="text-base">{a.className}</CardTitle>
                    <p className="text-muted-foreground text-sm">
                        {t("student.teacher", { name: a.teacherName })}
                    </p>
                </CardHeader>
                <CardContent className="grid gap-2 text-sm sm:grid-cols-2">
                    {a.description ? (
                        <p className="text-muted-foreground sm:col-span-2">{a.description}</p>
                    ) : null}
                    <Field label={t("student.fields.opensAt")} value={fmt(a.opensAt)} />
                    <Field label={t("student.fields.closesAt")} value={fmt(a.closesAt)} />
                    <Field
                        label={t("student.fields.timeLimit")}
                        value={
                            a.timeLimitMinutes
                                ? t("student.fields.minutes", { count: a.timeLimitMinutes })
                                : t("student.fields.noLimit")
                        }
                    />
                    <Field label={t("student.fields.questions")} value={a.totalQuestions ?? "—"} />
                    <Field label={t("student.fields.totalPoint")} value={a.totalPoint ?? "—"} />
                    <Field
                        label={t("student.fields.attemptsLeft")}
                        value={`${a.attemptsLeft}/${a.maxAttempts}`}
                    />
                </CardContent>
            </Card>

            <div className="flex justify-end">
                {a.hasInProgress && a.inProgressAttemptPublicId ? (
                    <Button onClick={() => goToAttempt(a.inProgressAttemptPublicId!)}>
                        <Play className="size-4" />
                        {t("student.actions.continue")}
                    </Button>
                ) : a.canStart ? (
                    <Button onClick={onStart} disabled={start.isPending}>
                        {start.isPending ? (
                            <Loader2 className="size-4 animate-spin" />
                        ) : (
                            <Play className="size-4" />
                        )}
                        {t("student.actions.start")}
                    </Button>
                ) : (
                    <Button disabled>{t("student.actions.unavailable")}</Button>
                )}
            </div>
        </div>
    );
}

function Field({ label, value }: { label: string; value: string | number }) {
    return (
        <div className="flex justify-between gap-2 border-b py-1">
            <span className="text-muted-foreground">{label}</span>
            <span className="font-medium">{value}</span>
        </div>
    );
}
