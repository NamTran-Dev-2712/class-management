import type { SortingState } from "@tanstack/react-table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { useTableParams } from "@/hooks/use-table-params";
import { getAuditLogColumns } from "./audit-log-columns";
import { AuditLogFilters, type TargetTypeFilter } from "./audit-log-filters";
import { useAuditLogs } from "./audit-logs.hook";
import type { Route } from "./+types/audit-logs.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Audit Log · Class Management" }];
}

export default function AdminAuditLogsPage() {
    const { t, i18n } = useTranslation("audit");
    const { params, setPage, setPageSize, setSort, setSearch } = useTableParams({
        pageSize: 20,
        sortBy: "createdAt",
        sortOrder: "desc",
    });
    const [searchParams, setSearchParams] = useSearchParams();

    const targetType = (searchParams.get("targetType") as TargetTypeFilter | null) ?? "all";

    const setTargetType = (value: TargetTypeFilter) =>
        setSearchParams((prev) => {
            const next = new URLSearchParams(prev);
            next.delete("page");
            if (value === "all") next.delete("targetType");
            else next.set("targetType", value);
            return next;
        });

    const { data, isLoading } = useAuditLogs({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        targetType: targetType === "all" ? undefined : targetType,
    });

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () => getAuditLogColumns({ t, locale: i18n.language }),
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
                <h2 className="text-xl font-semibold">{t("title")}</h2>
                <p className="text-muted-foreground text-sm">{t("subtitle")}</p>
            </div>

            <AuditLogFilters
                searchTerm={params.searchTerm ?? ""}
                targetType={targetType}
                onSearchChange={setSearch}
                onTargetTypeChange={setTargetType}
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
        </div>
    );
}
