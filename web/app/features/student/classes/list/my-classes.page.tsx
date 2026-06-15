import { Plus, Users } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useTableParams } from "@/hooks/use-table-params";
import { ApiError } from "@/lib/api-error";
import type { StudentClass } from "@/services/classroom/dtos/queries/student-class";
import { useLeaveClass, useMyClasses } from "../_shared/classes.hook";
import type { Route } from "./+types/my-classes.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "My Classes · Class Management" }];
}

export default function MyClassesPage() {
    const { t } = useTranslation("classroom");
    const navigate = useNavigate();
    const { params, setPage, setPageSize } = useTableParams({ pageSize: 12 });

    const { data, isLoading } = useMyClasses({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
    });
    const leave = useLeaveClass();
    const [leaving, setLeaving] = useState<StudentClass | null>(null);

    const confirmLeave = () => {
        if (!leaving) return;
        leave.mutate(leaving.classPublicId, {
            onSuccess: () => {
                toast.success(t("student.toast.left"));
                setLeaving(null);
            },
            onError: (e) => toast.error(e instanceof ApiError ? e.message : t("toast.error")),
        });
    };

    const items = data?.items ?? [];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between gap-4">
                <div>
                    <h2 className="text-xl font-semibold">{t("student.title")}</h2>
                    <p className="text-muted-foreground text-sm">{t("student.subtitle")}</p>
                </div>
                <Button onClick={() => navigate("/student/classes/join")}>
                    <Plus className="size-4" />
                    <span className="hidden sm:inline">{t("student.actions.join")}</span>
                </Button>
            </div>

            {isLoading ? (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {Array.from({ length: 6 }).map((_, i) => (
                        <Skeleton key={i} className="h-40 w-full" />
                    ))}
                </div>
            ) : items.length === 0 ? (
                <div className="text-muted-foreground rounded-md border py-16 text-center text-sm">
                    {t("student.empty")}
                </div>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {items.map((cls) => (
                        <Card key={cls.classPublicId} className="flex flex-col">
                            <CardHeader>
                                <CardTitle className="text-base">{cls.className}</CardTitle>
                                <p className="text-muted-foreground text-sm">
                                    {cls.subjectName ?? t("student.noSubject")}
                                </p>
                            </CardHeader>
                            <CardContent className="mt-auto space-y-3">
                                <p className="text-muted-foreground text-sm">
                                    {t("student.teacher", { name: cls.ownerName })}
                                </p>
                                <div className="flex gap-2">
                                    <Button variant="outline" size="sm" asChild>
                                        <Link to={`/student/classes/${cls.classPublicId}/members`}>
                                            <Users className="size-4" />
                                            {t("student.actions.members")}
                                        </Link>
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => setLeaving(cls)}
                                    >
                                        {t("student.actions.leave")}
                                    </Button>
                                </div>
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

            <ConfirmDialog
                open={leaving !== null}
                onOpenChange={(open) => !open && setLeaving(null)}
                title={t("student.leaveConfirm.title")}
                description={t("student.leaveConfirm.description", {
                    name: leaving?.className ?? "",
                })}
                confirmLabel={t("student.actions.leave")}
                onConfirm={confirmLeave}
                isPending={leave.isPending}
                destructive
            />
        </div>
    );
}
