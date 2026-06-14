import type { SortingState } from "@tanstack/react-table";
import { Plus } from "lucide-react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { DataTable } from "@/components/shared/data-table/data-table";
import { Button } from "@/components/ui/button";
import { useTableParams } from "@/hooks/use-table-params";
import type { Subject } from "@/services/subject/dtos/queries/list/response";
import { getSubjectColumns } from "./subject-columns";
import { SubjectFilters, type ActiveFilter } from "./subject-filters";
import { SubjectForm } from "./subject-form";
import { useDeleteSubject, useSubjects } from "./subjects.hook";
import type { Route } from "./+types/subjects.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Subjects · Class Management" }];
}

export default function SubjectsPage() {
    const { t, i18n } = useTranslation("subject");
    const { params, setPage, setPageSize, setSort, setSearch } = useTableParams({
        pageSize: 10,
        sortBy: "displayOrder",
        sortOrder: "asc",
    });
    const [searchParams, setSearchParams] = useSearchParams();

    const activeValue = (searchParams.get("active") as ActiveFilter | null) ?? "all";
    const isActive = activeValue === "all" ? undefined : activeValue === "true";

    const setActive = (value: ActiveFilter) =>
        setSearchParams(
            (prev) => {
                const next = new URLSearchParams(prev);
                next.delete("page");
                if (value === "all") next.delete("active");
                else next.set("active", value);
                return next;
            },
            { replace: false },
        );

    const query = {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        searchTerm: params.searchTerm,
        isActive,
    };
    const { data, isLoading } = useSubjects(query);
    const deleteSubject = useDeleteSubject();

    const [formOpen, setFormOpen] = useState(false);
    const [editing, setEditing] = useState<Subject | null>(null);
    const [deleting, setDeleting] = useState<Subject | null>(null);

    const sorting: SortingState = params.sortBy
        ? [{ id: params.sortBy, desc: params.sortOrder === "desc" }]
        : [];

    const columns = useMemo(
        () =>
            getSubjectColumns({
                t,
                locale: i18n.language,
                onEdit: (subject) => {
                    setEditing(subject);
                    setFormOpen(true);
                },
                onDelete: (subject) => setDeleting(subject),
            }),
        [t, i18n.language],
    );

    const handleSortingChange = (updater: SortingState | ((s: SortingState) => SortingState)) => {
        const next = typeof updater === "function" ? updater(sorting) : updater;
        const first = next[0];
        if (first) setSort(first.id, first.desc ? "desc" : "asc");
        else setSort(undefined, "asc");
    };

    const confirmDelete = () => {
        if (!deleting) return;
        deleteSubject.mutate(deleting.publicId, {
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
                <Button
                    onClick={() => {
                        setEditing(null);
                        setFormOpen(true);
                    }}
                >
                    <Plus className="size-4" />
                    <span className="hidden sm:inline">{t("actions.create")}</span>
                </Button>
            </div>

            <SubjectFilters
                searchTerm={params.searchTerm ?? ""}
                activeValue={activeValue}
                onSearchChange={setSearch}
                onActiveChange={setActive}
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

            <SubjectForm open={formOpen} onOpenChange={setFormOpen} subject={editing} />

            <ConfirmDialog
                open={deleting !== null}
                onOpenChange={(open) => !open && setDeleting(null)}
                title={t("delete.title")}
                description={t("delete.description", { name: deleting?.name ?? "" })}
                confirmLabel={t("actions.delete")}
                onConfirm={confirmDelete}
                isPending={deleteSubject.isPending}
                destructive
            />
        </div>
    );
}
