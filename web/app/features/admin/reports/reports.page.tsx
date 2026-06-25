import type { SortingState } from "@tanstack/react-table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { useTableParams } from "@/hooks/use-table-params";
import type {
    ReportListItem,
    ReportStatus,
    ReportTargetType,
} from "@/services/report/dtos/report-dtos";
import { getReportColumns } from "./report-columns";
import { ReportFilters } from "./report-filters";
import { ReportReviewDialog } from "./report-review-dialog";
import { useAdminReports } from "./reports.hook";
import type { Route } from "./+types/reports.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Reports · Class Management" }];
}

export default function AdminReportsPage() {
    const { t, i18n } = useTranslation("report");
    const { params, setPage, setPageSize, setSort, setSearch } = useTableParams({
        pageSize: 10,
        sortBy: "createdAt",
        sortOrder: "desc",
    });
    const [searchParams, setSearchParams] = useSearchParams();
    const [reviewing, setReviewing] = useState<ReportListItem | null>(null);

    const status = searchParams.get("status") ?? "all";
    const targetType = searchParams.get("targetType") ?? "all";

    const setParam = (key: string, value: string) =>
        setSearchParams((prev) => {
            const next = new URLSearchParams(prev);
            next.delete("page");
            if (value === "all") next.delete(key);
            else next.set(key, value);
            return next;
        });

    const { data, isLoading } = useAdminReports({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        status: status === "all" ? undefined : (status as ReportStatus),
        targetType: targetType === "all" ? undefined : (targetType as ReportTargetType),
    });

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () => getReportColumns({ t, locale: i18n.language, onReview: setReviewing }),
        [t, i18n.language],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) setSort(first.id, first.desc ? "desc" : "asc");
        else setSort(undefined, "desc");
    };

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("admin.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("admin.subtitle")}</p>
            </div>

            <ReportFilters
                searchTerm={params.searchTerm ?? ""}
                status={status}
                targetType={targetType}
                onSearchChange={setSearch}
                onStatusChange={(v) => setParam("status", v)}
                onTargetTypeChange={(v) => setParam("targetType", v)}
            />

            <DataTable
                columns={columns}
                data={data?.items ?? []}
                isLoading={isLoading}
                sorting={sorting}
                onSortingChange={handleSortingChange}
            />

            {data && data.items.length > 0 ? (
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

            <ReportReviewDialog
                report={reviewing}
                onOpenChange={(open) => !open && setReviewing(null)}
            />
        </div>
    );
}
