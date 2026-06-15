import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useTableParams } from "@/hooks/use-table-params";
import type { MembershipStatus } from "@/services/classroom/dtos/queries/class-member";
import { useMyRequests } from "../_shared/classes.hook";
import type { Route } from "./+types/requests.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Join requests · Class Management" }];
}

function statusVariant(status: MembershipStatus): "secondary" | "outline" | "destructive" {
    if (status === "Approved") return "secondary";
    if (status === "Rejected") return "destructive";
    return "outline";
}

export default function RequestsPage() {
    const { t, i18n } = useTranslation("classroom");
    const { params, setPage, setPageSize } = useTableParams({ pageSize: 10 });

    const { data, isLoading } = useMyRequests({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
    });
    const items = data?.items ?? [];

    const formatDate = (iso: string) =>
        new Intl.DateTimeFormat(i18n.language, { dateStyle: "medium" }).format(new Date(iso));

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between gap-4">
                <div>
                    <h2 className="text-xl font-semibold">{t("requests.title")}</h2>
                    <p className="text-muted-foreground text-sm">{t("requests.subtitle")}</p>
                </div>
                <Button variant="outline" asChild>
                    <Link to="/student/classes/join">{t("student.actions.join")}</Link>
                </Button>
            </div>

            {isLoading ? (
                <div className="space-y-2">
                    {Array.from({ length: 4 }).map((_, i) => (
                        <Skeleton key={i} className="h-16 w-full" />
                    ))}
                </div>
            ) : items.length === 0 ? (
                <div className="text-muted-foreground rounded-md border py-16 text-center text-sm">
                    {t("requests.empty")}
                </div>
            ) : (
                <div className="space-y-2">
                    {items.map((r) => (
                        <div
                            key={r.publicId}
                            className="flex flex-col gap-2 rounded-lg border p-4 sm:flex-row sm:items-center sm:justify-between"
                        >
                            <div>
                                <p className="font-medium">{r.className}</p>
                                <p className="text-muted-foreground text-sm">
                                    {(r.subjectName ?? t("student.noSubject")) +
                                        " · " +
                                        t("student.teacher", { name: r.ownerName }) +
                                        " · " +
                                        formatDate(r.joinedAt)}
                                </p>
                                {r.status === "Rejected" && r.rejectionReason ? (
                                    <p className="text-muted-foreground mt-1 text-xs italic">
                                        {r.rejectionReason}
                                    </p>
                                ) : null}
                            </div>
                            <Badge variant={statusVariant(r.status)}>
                                {t(`requests.status.${r.status}`)}
                            </Badge>
                        </div>
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
