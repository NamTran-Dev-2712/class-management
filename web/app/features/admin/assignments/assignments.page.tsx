import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { SortingState } from "@tanstack/react-table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { useTableParams } from "@/hooks/use-table-params";
import { ApiError } from "@/lib/api-error";
import { queryKeys } from "@/lib/query-keys";
import { adminService } from "@/services/admin/admin.service";
import { adminAssignmentService } from "@/services/assignment/assignment.service";
import type { AssignmentListItem } from "@/services/assignment/dtos/queries/assignment-list";
import { getAssignmentColumns } from "@/features/teacher/assignments/_shared/assignment-columns";
import type { Route } from "./+types/assignments.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Assignments · Admin" }];
}

export default function AdminAssignmentsPage() {
    const { t, i18n } = useTranslation("assignment");
    const { params, setPage, setPageSize, setSort } = useTableParams();
    const qc = useQueryClient();
    const [forceClosing, setForceClosing] = useState<AssignmentListItem | null>(null);

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

    const forceClose = useMutation({
        mutationFn: (publicId: string) => adminService.forceCloseAssignment(publicId),
        onSuccess: () => {
            toast.success(t("toast.forceClosed"));
            setForceClosing(null);
            qc.invalidateQueries({ queryKey: queryKeys.assignments.all });
        },
        onError: (error: unknown) =>
            toast.error(error instanceof ApiError ? error.message : t("toast.error")),
    });

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getAssignmentColumns({
                t,
                locale: i18n.language,
                showOwner: true,
                onForceClose: setForceClosing,
            }),
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

            <ConfirmDialog
                open={forceClosing !== null}
                onOpenChange={(open) => !open && setForceClosing(null)}
                title={t("forceClose.title")}
                description={t("forceClose.description", { title: forceClosing?.title ?? "" })}
                confirmLabel={t("actions.forceClose")}
                destructive
                isPending={forceClose.isPending}
                onConfirm={() => forceClosing && forceClose.mutate(forceClosing.publicId)}
            />
        </div>
    );
}
