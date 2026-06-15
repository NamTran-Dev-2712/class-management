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
import type { ClassListItem, ClassStatus } from "@/services/classroom/dtos/queries/class-list";
import { getClassroomColumns } from "../_shared/classroom-columns";
import { ClassroomFilters, type ClassStatusFilter } from "../_shared/classroom-filters";
import { useArchiveClass, useTeacherClasses } from "../_shared/classrooms.hook";
import type { Route } from "./+types/classrooms.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "My Classes · Class Management" }];
}

export default function ClassroomsPage() {
    const { t, i18n } = useTranslation("classroom");
    const navigate = useNavigate();
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

    const query = {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        status,
    };
    const { data, isLoading } = useTeacherClasses(query);
    const archive = useArchiveClass();

    const [archiving, setArchiving] = useState<ClassListItem | null>(null);

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getClassroomColumns({
                t,
                locale: i18n.language,
                onEdit: (cls) => navigate(`/teacher/classrooms/${cls.publicId}`),
                onMembers: (cls) => navigate(`/teacher/classrooms/${cls.publicId}/members`),
                onArchiveToggle: (cls) => {
                    if (cls.status === "Active") {
                        setArchiving(cls);
                    } else {
                        archive.mutate(
                            { publicId: cls.publicId, archive: false },
                            {
                                onSuccess: () => toast.success(t("toast.unarchived")),
                                onError: () => toast.error(t("toast.error")),
                            },
                        );
                    }
                },
            }),
        [t, i18n.language, navigate, archive],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) setSort(first.id, first.desc ? "desc" : "asc");
        else setSort(undefined, "asc");
    };

    const confirmArchive = () => {
        if (!archiving) return;
        archive.mutate(
            { publicId: archiving.publicId, archive: true },
            {
                onSuccess: () => {
                    toast.success(t("toast.archived"));
                    setArchiving(null);
                },
                onError: () => toast.error(t("toast.error")),
            },
        );
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between gap-4">
                <div>
                    <h2 className="text-xl font-semibold">{t("title")}</h2>
                    <p className="text-muted-foreground text-sm">{t("subtitle")}</p>
                </div>
                <Button onClick={() => navigate("/teacher/classrooms/new")}>
                    <Plus className="size-4" />
                    <span className="hidden sm:inline">{t("actions.create")}</span>
                </Button>
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

            <ConfirmDialog
                open={archiving !== null}
                onOpenChange={(open) => !open && setArchiving(null)}
                title={t("archiveConfirm.title")}
                description={t("archiveConfirm.description", { name: archiving?.name ?? "" })}
                confirmLabel={t("actions.archive")}
                onConfirm={confirmArchive}
                isPending={archive.isPending}
            />
        </div>
    );
}
