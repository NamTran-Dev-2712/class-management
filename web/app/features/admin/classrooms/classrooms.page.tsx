import type { SortingState } from "@tanstack/react-table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { useTableParams } from "@/hooks/use-table-params";
import {
    ClassroomFilters,
    type ClassStatusFilter,
} from "@/features/teacher/classrooms/_shared/classroom-filters";
import type { ClassStatus } from "@/services/classroom/dtos/queries/class-list";
import { getAdminClassroomColumns } from "./classroom-columns";
import { useAdminClasses } from "./classrooms.hook";
import type { Route } from "./+types/classrooms.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Classes · Class Management" }];
}

export default function AdminClassroomsPage() {
    const { t, i18n } = useTranslation("classroom");
    const { params, setPage, setPageSize, setSort, setSearch } = useTableParams({
        pageSize: 10,
        sortBy: "createdAt",
        sortOrder: "desc",
    });
    const [searchParams, setSearchParams] = useSearchParams();

    const statusValue = (searchParams.get("status") as ClassStatusFilter | null) ?? "all";
    const status = statusValue === "all" ? undefined : (statusValue as ClassStatus);

    const setStatus = (value: ClassStatusFilter) =>
        setSearchParams((prev) => {
            const next = new URLSearchParams(prev);
            next.delete("page");
            if (value === "all") next.delete("status");
            else next.set("status", value);
            return next;
        });

    const { data, isLoading } = useAdminClasses({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        status,
    });

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () => getAdminClassroomColumns({ t, locale: i18n.language }),
        [t, i18n.language],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) setSort(first.id, first.desc ? "desc" : "asc");
        else setSort(undefined, "asc");
    };

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("admin.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("admin.subtitle")}</p>
            </div>

            <ClassroomFilters
                searchTerm={params.searchTerm ?? ""}
                statusValue={statusValue}
                onSearchChange={setSearch}
                onStatusChange={setStatus}
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
