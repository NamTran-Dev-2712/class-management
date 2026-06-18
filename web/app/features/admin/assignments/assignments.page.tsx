import { useQuery } from "@tanstack/react-query";
import type { SortingState } from "@tanstack/react-table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { useTableParams } from "@/hooks/use-table-params";
import { queryKeys } from "@/lib/query-keys";
import { adminAssignmentService } from "@/services/assignment/assignment.service";
import { getAssignmentColumns } from "@/features/teacher/assignments/_shared/assignment-columns";
import type { Route } from "./+types/assignments.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Assignments · Admin" }];
}

export default function AdminAssignmentsPage() {
    const { t, i18n } = useTranslation("assignment");
    const { params, setPage, setPageSize, setSort } = useTableParams();

    const query = {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
    };
    const { data, isLoading } = useQuery({
        queryKey: queryKeys.assignments.adminList(query),
        queryFn: () => adminAssignmentService.list(query),
        placeholderData: (prev) => prev,
    });

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () => getAssignmentColumns({ t, locale: i18n.language, showOwner: true }),
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
