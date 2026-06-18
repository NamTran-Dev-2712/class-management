import type { SortingState } from "@tanstack/react-table";
import { Plus } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { useTableParams } from "@/hooks/use-table-params";
import { ApiError } from "@/lib/api-error";
import type {
    AssignmentListItem,
    AssignmentStatus,
} from "@/services/assignment/dtos/queries/assignment-list";
import { getAssignmentColumns } from "../_shared/assignment-columns";
import { useDeleteAssignment, useTeacherAssignments } from "../_shared/assignments.hook";
import type { Route } from "./+types/assignments.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Assignments · Class Management" }];
}

const STATUSES: AssignmentStatus[] = ["Draft", "Scheduled", "Open", "Closed", "Archived"];
const ALL = "all";

export default function AssignmentsPage() {
    const { t, i18n } = useTranslation("assignment");
    const navigate = useNavigate();
    const { params, setPage, setPageSize, setSort, setSearch } = useTableParams();
    const [status, setStatus] = useState<string>(ALL);
    const remove = useDeleteAssignment();
    const [deleting, setDeleting] = useState<AssignmentListItem | null>(null);

    const query = {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        status: status === ALL ? undefined : (status as AssignmentStatus),
    };
    const { data, isLoading } = useTeacherAssignments(query);

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getAssignmentColumns({
                t,
                locale: i18n.language,
                onOpen: (a) => navigate(`/teacher/assignments/${a.publicId}`),
                onDelete: (a) => setDeleting(a),
            }),
        [t, i18n.language, navigate],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) setSort(first.id, first.desc ? "desc" : "asc");
        else setSort(undefined, "asc");
    };

    const confirmDelete = () => {
        if (!deleting) return;
        remove.mutate(deleting.publicId, {
            onSuccess: () => {
                toast.success(t("toast.deleted"));
                setDeleting(null);
            },
            onError: (err) => toast.error(err instanceof ApiError ? err.message : t("toast.error")),
        });
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between gap-4">
                <div>
                    <h2 className="text-xl font-semibold">{t("title")}</h2>
                    <p className="text-muted-foreground text-sm">{t("subtitle")}</p>
                </div>
                <Button onClick={() => navigate("/teacher/assignments/new")}>
                    <Plus className="size-4" />
                    <span className="hidden sm:inline">{t("actions.create")}</span>
                </Button>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row">
                <Input
                    placeholder={t("filters.search")}
                    defaultValue={params.searchTerm ?? ""}
                    onChange={(e) => setSearch(e.target.value || undefined)}
                    className="sm:max-w-xs"
                />
                <Select value={status} onValueChange={setStatus}>
                    <SelectTrigger className="sm:w-44">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value={ALL}>{t("filters.allStatuses")}</SelectItem>
                        {STATUSES.map((s) => (
                            <SelectItem key={s} value={s}>
                                {t(`status.${s}`)}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
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
                open={deleting !== null}
                onOpenChange={(open) => !open && setDeleting(null)}
                title={t("deleteConfirm.title")}
                description={t("deleteConfirm.description")}
                confirmLabel={t("actions.delete")}
                onConfirm={confirmDelete}
                isPending={remove.isPending}
                destructive
            />
        </div>
    );
}
