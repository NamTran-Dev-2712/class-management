import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import type { ClassListItem } from "@/services/classroom/dtos/queries/class-list";

interface ColumnOptions {
    t: TFunction;
    locale: string;
}

/** Read-only columns for the admin all-classes overview. */
export function getAdminClassroomColumns({ t, locale }: ColumnOptions): ColumnDef<ClassListItem>[] {
    const formatDate = (iso: string) =>
        new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(iso));

    return [
        {
            accessorKey: "name",
            header: () => t("columns.name"),
            enableSorting: true,
            meta: { label: t("columns.name") },
            cell: ({ row }) => <span className="font-medium">{row.original.name}</span>,
        },
        {
            accessorKey: "subjectName",
            header: () => t("columns.subject"),
            enableSorting: false,
            meta: { label: t("columns.subject") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{row.original.subjectName ?? "—"}</span>
            ),
        },
        {
            accessorKey: "ownerName",
            header: () => t("columns.owner"),
            enableSorting: false,
            meta: { label: t("columns.owner") },
            cell: ({ row }) => row.original.ownerName,
        },
        {
            accessorKey: "status",
            header: () => t("columns.status"),
            enableSorting: false,
            meta: { label: t("columns.status") },
            cell: ({ row }) =>
                row.original.status === "Active" ? (
                    <Badge variant="secondary">{t("status.active")}</Badge>
                ) : (
                    <Badge variant="outline">{t("status.archived")}</Badge>
                ),
        },
        {
            id: "members",
            header: () => t("columns.members"),
            enableSorting: false,
            meta: { label: t("columns.members") },
            cell: ({ row }) => row.original.approvedMemberCount,
        },
        {
            accessorKey: "createdAt",
            header: () => t("columns.createdAt"),
            enableSorting: true,
            meta: { label: t("columns.createdAt") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{formatDate(row.original.createdAt)}</span>
            ),
        },
    ];
}
