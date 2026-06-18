import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";
import { Eye, MoreHorizontal, Pencil, Trash2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { AssignmentListItem } from "@/services/assignment/dtos/queries/assignment-list";
import { AssignmentStatusBadge } from "./assignment-status-badge";

interface ColumnOptions {
    t: TFunction;
    locale: string;
    showOwner?: boolean;
    onOpen?: (a: AssignmentListItem) => void;
    onDelete?: (a: AssignmentListItem) => void;
}

export function getAssignmentColumns({
    t,
    locale,
    showOwner,
    onOpen,
    onDelete,
}: ColumnOptions): ColumnDef<AssignmentListItem>[] {
    const formatDate = (iso: string | null) =>
        iso
            ? new Intl.DateTimeFormat(locale, { dateStyle: "medium", timeStyle: "short" }).format(
                  new Date(iso),
              )
            : "—";

    const columns: ColumnDef<AssignmentListItem>[] = [
        {
            accessorKey: "title",
            header: () => t("columns.title"),
            enableSorting: true,
            meta: { label: t("columns.title") },
            cell: ({ row }) =>
                onOpen ? (
                    <button
                        type="button"
                        onClick={() => onOpen(row.original)}
                        className="hover:text-primary line-clamp-2 max-w-md text-left font-medium"
                    >
                        {row.original.title}
                    </button>
                ) : (
                    <span className="line-clamp-2 max-w-md font-medium">{row.original.title}</span>
                ),
        },
        {
            accessorKey: "className",
            header: () => t("columns.class"),
            enableSorting: false,
            meta: { label: t("columns.class") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{row.original.className}</span>
            ),
        },
        {
            accessorKey: "status",
            header: () => t("columns.status"),
            enableSorting: true,
            meta: { label: t("columns.status") },
            cell: ({ row }) => <AssignmentStatusBadge status={row.original.status} t={t} />,
        },
        {
            accessorKey: "closesAt",
            header: () => t("columns.closesAt"),
            enableSorting: true,
            meta: { label: t("columns.closesAt") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{formatDate(row.original.closesAt)}</span>
            ),
        },
        {
            id: "submissions",
            header: () => t("columns.submissions"),
            enableSorting: false,
            meta: { label: t("columns.submissions") },
            cell: ({ row }) => (
                <span className="tabular-nums">
                    {row.original.submittedCount}/{row.original.attemptCount}
                </span>
            ),
        },
    ];

    if (showOwner) {
        columns.push({
            accessorKey: "teacherName",
            header: () => t("columns.owner"),
            enableSorting: false,
            meta: { label: t("columns.owner") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{row.original.teacherName}</span>
            ),
        });
    }

    if (onOpen || onDelete) {
        columns.push({
            id: "actions",
            header: () => <span className="sr-only">{t("columns.actions")}</span>,
            enableSorting: false,
            meta: { hideOnMobile: true },
            cell: ({ row }) => {
                const a = row.original;
                return (
                    <div className="text-right">
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="icon" className="size-8">
                                    <MoreHorizontal className="size-4" />
                                    <span className="sr-only">{t("columns.actions")}</span>
                                </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                                {onOpen ? (
                                    <DropdownMenuItem onSelect={() => onOpen(a)}>
                                        <Eye className="size-4" />
                                        {t("actions.open")}
                                    </DropdownMenuItem>
                                ) : null}
                                {onDelete && a.status === "Draft" ? (
                                    <>
                                        <DropdownMenuSeparator />
                                        <DropdownMenuItem
                                            variant="destructive"
                                            onSelect={() => onDelete(a)}
                                        >
                                            <Trash2 className="size-4" />
                                            {t("actions.delete")}
                                        </DropdownMenuItem>
                                    </>
                                ) : null}
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </div>
                );
            },
        });
    }

    return columns;
}
