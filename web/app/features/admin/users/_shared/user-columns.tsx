import type { ColumnDef } from "@tanstack/react-table";
import { Lock, MoreHorizontal, Pencil, Trash2, Unlock } from "lucide-react";
import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { User } from "@/services/user/dtos/queries/list/response";

interface ColumnOptions {
    t: TFunction;
    locale: string;
    onEdit: (user: User) => void;
    onToggleLock: (user: User) => void;
    onDelete: (user: User) => void;
}

export function getUserColumns({
    t,
    locale,
    onEdit,
    onToggleLock,
    onDelete,
}: ColumnOptions): ColumnDef<User>[] {
    const formatDate = (iso: string) =>
        new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(iso));

    const roleLabel = (role: string) => t(`roles.${role}`, { defaultValue: role });

    return [
        {
            accessorKey: "displayName",
            header: () => t("columns.displayName"),
            enableSorting: true,
            meta: { label: t("columns.displayName") },
            cell: ({ row }) => <span className="font-medium">{row.original.displayName}</span>,
        },
        {
            accessorKey: "email",
            header: () => t("columns.email"),
            enableSorting: true,
            meta: { label: t("columns.email") },
            cell: ({ row }) => <span className="text-muted-foreground">{row.original.email}</span>,
        },
        {
            accessorKey: "roles",
            header: () => t("columns.role"),
            enableSorting: false,
            meta: { label: t("columns.role") },
            cell: ({ row }) => (
                <div className="flex flex-wrap gap-1">
                    {row.original.roles.map((role) => (
                        <Badge key={role} variant="secondary">
                            {roleLabel(role)}
                        </Badge>
                    ))}
                </div>
            ),
        },
        {
            accessorKey: "isLocked",
            header: () => t("columns.status"),
            enableSorting: false,
            meta: { label: t("columns.status") },
            cell: ({ row }) =>
                row.original.isLocked ? (
                    <Badge variant="destructive">{t("status.locked")}</Badge>
                ) : (
                    <Badge variant="secondary">{t("status.active")}</Badge>
                ),
        },
        {
            accessorKey: "lastLoginAt",
            header: () => t("columns.lastLoginAt"),
            enableSorting: true,
            meta: { label: t("columns.lastLoginAt") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">
                    {row.original.lastLoginAt
                        ? formatDate(row.original.lastLoginAt)
                        : t("empty.lastLogin")}
                </span>
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
                            <DropdownMenuItem onSelect={() => onToggleLock(row.original)}>
                                {row.original.isLocked ? (
                                    <>
                                        <Unlock className="size-4" />
                                        {t("actions.unlock")}
                                    </>
                                ) : (
                                    <>
                                        <Lock className="size-4" />
                                        {t("actions.lock")}
                                    </>
                                )}
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
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
