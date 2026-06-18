import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";

import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useTableParams } from "@/hooks/use-table-params";
import { AssignmentStatusBadge } from "@/features/teacher/assignments/_shared/assignment-status-badge";
import { useStudentAssignments } from "../_shared/assignments.hook";
import type { Route } from "./+types/assignments.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Assignments · Class Management" }];
}

export default function StudentAssignmentsPage() {
    const { t, i18n } = useTranslation("assignment");
    const navigate = useNavigate();
    const { params, setPage, setPageSize } = useTableParams({ pageSize: 12 });
    const { data, isLoading } = useStudentAssignments({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
    });

    const items = data?.items ?? [];
    const fmt = (iso: string | null) =>
        iso
            ? new Intl.DateTimeFormat(i18n.language, {
                  dateStyle: "medium",
                  timeStyle: "short",
              }).format(new Date(iso))
            : "—";

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("student.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("student.subtitle")}</p>
            </div>

            {isLoading ? (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {Array.from({ length: 6 }).map((_, i) => (
                        <Skeleton key={i} className="h-44 w-full" />
                    ))}
                </div>
            ) : items.length === 0 ? (
                <div className="text-muted-foreground rounded-md border py-16 text-center text-sm">
                    {t("student.empty")}
                </div>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {items.map((a) => (
                        <Card key={a.publicId} className="flex flex-col">
                            <CardHeader>
                                <div className="flex items-start justify-between gap-2">
                                    <CardTitle className="text-base">{a.title}</CardTitle>
                                    <AssignmentStatusBadge status={a.status} t={t} />
                                </div>
                                <p className="text-muted-foreground text-sm">{a.className}</p>
                            </CardHeader>
                            <CardContent className="mt-auto space-y-2 text-sm">
                                <p className="text-muted-foreground">
                                    {t("student.due", { date: fmt(a.closesAt) })}
                                </p>
                                <p className="text-muted-foreground">
                                    {t("student.attemptsLeft", { count: a.attemptsLeft })}
                                </p>
                                <Button
                                    className="w-full"
                                    onClick={() => navigate(`/student/assignments/${a.publicId}`)}
                                >
                                    {a.hasInProgress
                                        ? t("student.actions.continue")
                                        : t("student.actions.view")}
                                </Button>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            )}

            {data && items.length > 0 ? (
                <DataTablePagination
                    pageNumber={data.pageNumber}
                    pageSize={data.pageSize}
                    totalPages={data.totalPages}
                    totalCount={data.totalCount}
                    hasPreviousPage={data.hasPreviousPage}
                    hasNextPage={data.hasNextPage}
                    onPageChange={setPage}
                    onPageSizeChange={setPageSize}
                />
            ) : null}
        </div>
    );
}
