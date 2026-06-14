import type { SortingState } from "@tanstack/react-table";
import { Plus } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useSearchParams } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Button } from "@/components/ui/button";
import { useTableParams } from "@/hooks/use-table-params";
import type { User } from "@/services/user/dtos/queries/list/response";
import { getUserColumns } from "../_shared/user-columns";
import { UserFilters, type RoleFilter, type StatusFilter } from "../_shared/user-filters";
import { useDeleteUser, useLockUser, useUnlockUser, useUsers } from "../_shared/users.hook";
import type { Route } from "./+types/users.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Users · Class Management" }];
}

export default function UsersPage() {
    const { t, i18n } = useTranslation("user");
    const navigate = useNavigate();
    const { params, setPage, setPageSize, setSort, setSearch } = useTableParams({
        pageSize: 10,
        sortBy: "displayName",
        sortOrder: "asc",
    });
    const [searchParams, setSearchParams] = useSearchParams();

    const statusValue = (searchParams.get("status") as StatusFilter | null) ?? "all";
    const roleValue = (searchParams.get("role") as RoleFilter | null) ?? "all";
    const isLocked = statusValue === "all" ? undefined : statusValue === "locked";
    const role = roleValue === "all" ? undefined : roleValue;

    const setParam = (key: string, value: string) =>
        setSearchParams((prev) => {
            const next = new URLSearchParams(prev);
            next.delete("page");
            if (value === "all") next.delete(key);
            else next.set(key, value);
            return next;
        });

    const query = {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        isLocked,
        role,
    };
    const { data, isLoading } = useUsers(query);
    const lockUser = useLockUser();
    const unlockUser = useUnlockUser();
    const deleteUser = useDeleteUser();

    const [locking, setLocking] = useState<User | null>(null);
    const [deleting, setDeleting] = useState<User | null>(null);

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const handleToggleLock = (user: User) => {
        if (user.isLocked) {
            unlockUser.mutate(user.publicId, {
                onSuccess: () => toast.success(t("toast.unlocked")),
                onError: () => toast.error(t("toast.error")),
            });
        } else {
            setLocking(user);
        }
    };

    const columns = useMemo(
        () =>
            getUserColumns({
                t,
                locale: i18n.language,
                onEdit: (user) => navigate(`/admin/users/${user.publicId}`),
                onToggleLock: handleToggleLock,
                onDelete: (user) => setDeleting(user),
            }),
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [t, i18n.language],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) setSort(first.id, first.desc ? "desc" : "asc");
        else setSort(undefined, "asc");
    };

    const confirmLock = () => {
        if (!locking) return;
        lockUser.mutate(locking.publicId, {
            onSuccess: () => {
                toast.success(t("toast.locked"));
                setLocking(null);
            },
            onError: () => toast.error(t("toast.error")),
        });
    };

    const confirmDelete = () => {
        if (!deleting) return;
        deleteUser.mutate(deleting.publicId, {
            onSuccess: () => {
                toast.success(t("toast.deleted"));
                setDeleting(null);
            },
            onError: () => toast.error(t("toast.error")),
        });
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between gap-4">
                <div>
                    <h2 className="text-xl font-semibold">{t("title")}</h2>
                    <p className="text-muted-foreground text-sm">{t("subtitle")}</p>
                </div>
                <Button onClick={() => navigate("/admin/users/new")}>
                    <Plus className="size-4" />
                    <span className="hidden sm:inline">{t("actions.create")}</span>
                </Button>
            </div>

            <UserFilters
                searchTerm={params.searchTerm ?? ""}
                statusValue={statusValue}
                roleValue={roleValue}
                onSearchChange={setSearch}
                onStatusChange={(v) => setParam("status", v)}
                onRoleChange={(v) => setParam("role", v)}
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

            <ConfirmDialog
                open={locking !== null}
                onOpenChange={(open) => !open && setLocking(null)}
                title={t("lockConfirm.title")}
                description={t("lockConfirm.description", { name: locking?.displayName ?? "" })}
                confirmLabel={t("actions.lock")}
                onConfirm={confirmLock}
                isPending={lockUser.isPending}
                destructive
            />

            <ConfirmDialog
                open={deleting !== null}
                onOpenChange={(open) => !open && setDeleting(null)}
                title={t("delete.title")}
                description={t("delete.description", { name: deleting?.displayName ?? "" })}
                confirmLabel={t("actions.delete")}
                onConfirm={confirmDelete}
                isPending={deleteUser.isPending}
                destructive
            />
        </div>
    );
}
