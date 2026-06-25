import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import type { AuditLogListItem } from "@/services/audit-log/dtos/queries/audit-log-list";

interface ColumnOptions {
    t: TFunction;
    locale: string;
}

/** Read-only columns for the admin audit log. Actions are technical identifiers, shown verbatim. */
export function getAuditLogColumns({ t, locale }: ColumnOptions): ColumnDef<AuditLogListItem>[] {
    const formatDateTime = (iso: string) =>
        new Intl.DateTimeFormat(locale, { dateStyle: "medium", timeStyle: "short" }).format(
            new Date(iso),
        );

    return [
        {
            accessorKey: "createdAt",
            header: () => t("columns.time"),
            enableSorting: true,
            meta: { label: t("columns.time") },
            cell: ({ row }) => (
                <span className="text-muted-foreground whitespace-nowrap">
                    {formatDateTime(row.original.createdAt)}
                </span>
            ),
        },
        {
            accessorKey: "action",
            header: () => t("columns.action"),
            enableSorting: true,
            meta: { label: t("columns.action") },
            cell: ({ row }) => (
                <Badge variant="secondary" className="font-mono text-xs">
                    {row.original.action}
                </Badge>
            ),
        },
        {
            accessorKey: "actorName",
            header: () => t("columns.actor"),
            enableSorting: false,
            meta: { label: t("columns.actor") },
            cell: ({ row }) =>
                row.original.actorName ? (
                    <div className="flex flex-col">
                        <span className="font-medium">{row.original.actorName}</span>
                        {row.original.actorEmail ? (
                            <span className="text-muted-foreground text-xs">
                                {row.original.actorEmail}
                            </span>
                        ) : null}
                    </div>
                ) : (
                    <span className="text-muted-foreground">{t("system")}</span>
                ),
        },
        {
            accessorKey: "actorRole",
            header: () => t("columns.role"),
            enableSorting: false,
            meta: { label: t("columns.role") },
            cell: ({ row }) =>
                row.original.actorRole ? (
                    <Badge variant="outline">{row.original.actorRole}</Badge>
                ) : (
                    <span className="text-muted-foreground">—</span>
                ),
        },
        {
            accessorKey: "targetType",
            header: () => t("columns.target"),
            enableSorting: false,
            meta: { label: t("columns.target") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{row.original.targetType ?? "—"}</span>
            ),
        },
        {
            accessorKey: "metadata",
            header: () => t("columns.details"),
            enableSorting: false,
            meta: { label: t("columns.details") },
            cell: ({ row }) =>
                row.original.metadata ? (
                    <code
                        className="text-muted-foreground line-clamp-2 block max-w-xs text-xs"
                        title={row.original.metadata}
                    >
                        {row.original.metadata}
                    </code>
                ) : (
                    <span className="text-muted-foreground">—</span>
                ),
        },
        {
            accessorKey: "ipAddress",
            header: () => t("columns.ip"),
            enableSorting: false,
            meta: { label: t("columns.ip") },
            cell: ({ row }) => (
                <span className="text-muted-foreground font-mono text-xs">
                    {row.original.ipAddress ?? "—"}
                </span>
            ),
        },
    ];
}
