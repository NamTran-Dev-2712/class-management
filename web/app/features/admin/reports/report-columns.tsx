import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";
import { MoreHorizontal } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { ReportListItem, ReportStatus } from "@/services/report/dtos/report-dtos";

const STATUS_VARIANT: Record<ReportStatus, "secondary" | "outline" | "default" | "destructive"> = {
    Pending: "default",
    Reviewing: "secondary",
    Resolved: "outline",
    Rejected: "destructive",
};

interface ColumnOptions {
    t: TFunction;
    locale: string;
    onReview: (report: ReportListItem) => void;
}

export function getReportColumns({
    t,
    locale,
    onReview,
}: ColumnOptions): ColumnDef<ReportListItem>[] {
    const formatDate = (iso: string) =>
        new Intl.DateTimeFormat(locale, { dateStyle: "medium", timeStyle: "short" }).format(
            new Date(iso),
        );

    return [
        {
            accessorKey: "targetType",
            header: () => t("columns.target"),
            enableSorting: true,
            meta: { label: t("columns.target") },
            cell: ({ row }) => <Badge variant="outline">{row.original.targetType}</Badge>,
        },
        {
            accessorKey: "reason",
            header: () => t("columns.reason"),
            enableSorting: false,
            meta: { label: t("columns.reason") },
            cell: ({ row }) => t(`reasons.${row.original.reason}`),
        },
        {
            accessorKey: "reporterName",
            header: () => t("columns.reporter"),
            enableSorting: false,
            meta: { label: t("columns.reporter") },
            cell: ({ row }) => (
                <div className="flex flex-col">
                    <span>{row.original.reporterName ?? "—"}</span>
                    {row.original.reporterEmail ? (
                        <span className="text-muted-foreground text-xs">
                            {row.original.reporterEmail}
                        </span>
                    ) : null}
                </div>
            ),
        },
        {
            accessorKey: "status",
            header: () => t("columns.status"),
            enableSorting: true,
            meta: { label: t("columns.status") },
            cell: ({ row }) => (
                <Badge variant={STATUS_VARIANT[row.original.status]}>
                    {t(`status.${row.original.status}`)}
                </Badge>
            ),
        },
        {
            accessorKey: "createdAt",
            header: () => t("columns.createdAt"),
            enableSorting: true,
            meta: { label: t("columns.createdAt") },
            cell: ({ row }) => (
                <span className="text-muted-foreground whitespace-nowrap">
                    {formatDate(row.original.createdAt)}
                </span>
            ),
        },
        {
            id: "actions",
            header: () => <span className="sr-only">{t("columns.actions")}</span>,
            enableSorting: false,
            meta: { hideOnMobile: true },
            cell: ({ row }) => {
                const closed =
                    row.original.status === "Resolved" || row.original.status === "Rejected";
                return (
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                                <MoreHorizontal className="size-4" />
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                            <DropdownMenuItem
                                disabled={closed}
                                onSelect={() => onReview(row.original)}
                            >
                                {t("actions.review")}
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                );
            },
        },
    ];
}
