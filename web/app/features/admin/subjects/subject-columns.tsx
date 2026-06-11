import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";
import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { Subject } from "@/services/subject/dtos/subject";

interface ColumnOptions {
    t: TFunction;
    locale: string;
    onEdit: (subject: Subject) => void;
    onDelete: (subject: Subject) => void;
}

export function getSubjectColumns({
    t,
    locale,
    onEdit,
    onDelete,
}: ColumnOptions): ColumnDef<Subject>[] {
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
            accessorKey: "description",
            header: () => t("columns.description"),
            enableSorting: false,
            meta: { label: t("columns.description") },
            cell: ({ row }) => (
                <span className="text-muted-foreground line-clamp-1 max-w-[28rem]">
                    {row.original.description ?? "—"}
                </span>
            ),
        },
        {
            accessorKey: "displayOrder",
            header: () => t("columns.displayOrder"),
            enableSorting: true,
            meta: { label: t("columns.displayOrder") },
            cell: ({ row }) => row.original.displayOrder,
        },
        {
            accessorKey: "isActive",
            header: () => t("columns.status"),
            enableSorting: false,
            meta: { label: t("columns.status") },
            cell: ({ row }) =>
                row.original.isActive ? (
                    <Badge variant="secondary">{t("status.active")}</Badge>
                ) : (
                    <Badge variant="outline">{t("status.inactive")}</Badge>
                ),
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
        {
            id: "actions",
            header: () => <span className="sr-only">{t("columns.actions")}</span>,
            enableSorting: false,
            meta: { hideOnMobile: true },
            cell: ({ row }) => (
                <div className="text-right">
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon" className="size-8">
                                <MoreHorizontal className="size-4" />
                                <span className="sr-only">{t("columns.actions")}</span>
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                            <DropdownMenuItem onSelect={() => onEdit(row.original)}>
                                <Pencil className="size-4" />
                                {t("actions.edit")}
                            </DropdownMenuItem>
                            <DropdownMenuItem
                                variant="destructive"
                                onSelect={() => onDelete(row.original)}
                            >
                                <Trash2 className="size-4" />
                                {t("actions.delete")}
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            ),
        },
    ];
}
