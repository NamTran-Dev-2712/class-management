import {
    ArrowLeft,
    Award,
    CheckCircle2,
    Download,
    Loader2,
    TrendingDown,
    TrendingUp,
    Users,
} from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";

import { StatCard } from "@/components/shared/dashboard/stat-card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { ApiError } from "@/lib/api-error";
import { teacherAssignmentService } from "@/services/assignment/assignment.service";
import type { AssignmentReport } from "@/services/assignment/dtos/queries/assignment-detail";
import { useAssignmentReport } from "../_shared/grading.hook";
import { GradeHistogram } from "./grade-histogram";
import type { Route } from "./+types/report.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Report · Class Management" }];
}

export default function AssignmentReportPage() {
    const { t } = useTranslation("assignment");
    const navigate = useNavigate();
    const { publicId } = useParams();
    const { data, isLoading } = useAssignmentReport(publicId);

    if (isLoading || !data) {
        return <Skeleton className="h-96 w-full" />;
    }

    return (
        <ReportView
            report={data}
            publicId={publicId!}
            onBack={() => navigate(`/teacher/assignments/${publicId}`)}
            t={t}
        />
    );
}

function ReportView({
    report,
    publicId,
    onBack,
    t,
}: {
    report: AssignmentReport;
    publicId: string;
    onBack: () => void;
    t: (k: string, o?: Record<string, unknown>) => string;
}) {
    const [exporting, setExporting] = useState(false);
    const fmt = (n: number | null) => (n != null ? String(n) : "—");

    const onExport = async () => {
        setExporting(true);
        try {
            const blob = await teacherAssignmentService.exportGrades(publicId);
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = `grades-${report.title.replace(/\s+/g, "-").toLowerCase()}.csv`;
            a.click();
            URL.revokeObjectURL(url);
        } catch (err) {
            toast.error(err instanceof ApiError ? err.message : t("report.exportError"));
        } finally {
            setExporting(false);
        }
    };

    return (
        <div className="mx-auto max-w-5xl space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex items-center gap-3">
                    <Button variant="ghost" size="icon" onClick={onBack}>
                        <ArrowLeft className="size-4" />
                    </Button>
                    <div>
                        <h2 className="text-xl font-semibold">{report.title}</h2>
                        <p className="text-muted-foreground text-sm">{t("report.subtitle")}</p>
                    </div>
                </div>
                <Button variant="outline" onClick={onExport} disabled={exporting}>
                    {exporting ? (
                        <Loader2 className="size-4 animate-spin" />
                    ) : (
                        <Download className="size-4" />
                    )}
                    {t("report.export")}
                </Button>
            </div>

            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title={t("report.stats.totalStudents")}
                    value={String(report.totalStudents)}
                    icon={Users}
                />
                <StatCard
                    title={t("report.stats.submitted")}
                    value={String(report.submittedCount)}
                    icon={CheckCircle2}
                    hint={t("report.stats.notSubmitted") + `: ${report.notSubmittedCount}`}
                />
                <StatCard
                    title={t("report.stats.graded")}
                    value={String(report.gradedCount)}
                    icon={Award}
                    hint={t("report.stats.pending") + `: ${report.pendingGradingCount}`}
                />
                <StatCard
                    title={t("report.stats.average")}
                    value={fmt(report.averageScore)}
                    icon={TrendingUp}
                    hint={report.totalPoint != null ? `/ ${report.totalPoint}` : undefined}
                />
                <StatCard
                    title={t("report.stats.min")}
                    value={fmt(report.minScore)}
                    icon={TrendingDown}
                />
                <StatCard
                    title={t("report.stats.max")}
                    value={fmt(report.maxScore)}
                    icon={TrendingUp}
                />
            </div>

            {report.buckets.length > 0 ? (
                <Card>
                    <CardHeader>
                        <CardTitle className="text-base">{t("report.distribution")}</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <GradeHistogram
                            buckets={report.buckets}
                            yLabel={t("report.studentsAxis")}
                        />
                    </CardContent>
                </Card>
            ) : null}

            <Card>
                <CardHeader>
                    <CardTitle className="text-base">{t("report.table.title")}</CardTitle>
                </CardHeader>
                <CardContent>
                    {report.students.length === 0 ? (
                        <p className="text-muted-foreground py-8 text-center text-sm">
                            {t("report.table.empty")}
                        </p>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>{t("report.table.student")}</TableHead>
                                    <TableHead>{t("report.table.status")}</TableHead>
                                    <TableHead className="text-center">
                                        {t("report.table.attempts")}
                                    </TableHead>
                                    <TableHead className="text-right">
                                        {t("report.table.score")}
                                    </TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {report.students.map((s) => (
                                    <TableRow key={s.studentPublicId}>
                                        <TableCell className="font-medium">
                                            {s.studentName}
                                        </TableCell>
                                        <TableCell>
                                            <Badge variant={s.submitted ? "outline" : "secondary"}>
                                                {t(`attemptStatus.${s.status}`)}
                                            </Badge>
                                        </TableCell>
                                        <TableCell className="text-center tabular-nums">
                                            {s.attemptCount}
                                        </TableCell>
                                        <TableCell className="text-right tabular-nums">
                                            {s.effectiveScore != null ? s.effectiveScore : "—"}
                                            {s.effectiveScore != null && report.totalPoint != null
                                                ? ` / ${report.totalPoint}`
                                                : ""}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
