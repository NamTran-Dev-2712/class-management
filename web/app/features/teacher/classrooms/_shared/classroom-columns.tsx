import type { ColumnDef } from "@tanstack/react-table";
import { Archive, ArchiveRestore, MoreHorizontal, Pencil, Users } from "lucide-react";
import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { ClassListItem } from "@/services/classroom/dtos/queries/class-list";

interface ColumnOptions {
    t: TFunction;
    locale: string;
    onEdit: (cls: ClassListItem) => void;
    onMembers: (cls: ClassListItem) => void;
    onArchiveToggle: (cls: ClassListItem) => void;
}

export function getClassroomColumns({
    t,
    locale,
    onEdit,
    onMembers,
    onArchiveToggle,
}: ColumnOptions): ColumnDef<ClassListItem>[] {
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
            cell: ({ row }) => (
                <div className="flex items-center gap-2">
                    <span>{row.original.approvedMemberCount}</span>
                    {row.original.pendingCount > 0 ? (
                        <Badge variant="outline">
                            {t("columns.pendingBadge", { count: row.original.pendingCount })}
                        </Badge>
                    ) : null}
                </div>
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
            cell: ({ row }) => {
                const cls = row.original;
                const isActive = cls.status === "Active";
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
                                <DropdownMenuItem onSelect={() => onMembers(cls)}>
                                    <Users className="size-4" />
                                    {t("actions.members")}
                                </DropdownMenuItem>
                                <DropdownMenuItem onSelect={() => onEdit(cls)}>
                                    <Pencil className="size-4" />
                                    {t("actions.edit")}
                                </DropdownMenuItem>
                                <DropdownMenuItem onSelect={() => onArchiveToggle(cls)}>
                                    {isActive ? (
                                        <>
                                            <Archive className="size-4" />
                                            {t("actions.archive")}
                                        </>
                                    ) : (
                                        <>
                                            <ArchiveRestore className="size-4" />
                                            {t("actions.unarchive")}
                                        </>
                                    )}
                                </DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </div>
                );
            },
        },
    ];
}
