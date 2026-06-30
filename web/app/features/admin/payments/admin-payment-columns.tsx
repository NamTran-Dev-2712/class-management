import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import { formatVnd } from "@/lib/currency";
import type { AdminPaymentDto } from "@/services/payment/dtos/payment-dtos";

const STATUS_VARIANT: Record<string, "secondary" | "outline" | "default" | "destructive"> = {
    Pending: "secondary",
    Completed: "default",
    Failed: "destructive",
    Expired: "outline",
};

export function getAdminPaymentColumns(t: TFunction, locale: string): ColumnDef<AdminPaymentDto>[] {
    const formatDate = (iso: string | null) =>
        iso
            ? new Intl.DateTimeFormat(locale, { dateStyle: "medium", timeStyle: "short" }).format(
                  new Date(iso),
              )
            : "—";

    return [
        {
            accessorKey: "teacherName",
            header: () => t("admin.payments.teacher"),
            enableSorting: false,
            meta: { label: t("admin.payments.teacher") },
            cell: ({ row }) => (
                <div className="flex flex-col">
                    <span>{row.original.teacherName ?? "—"}</span>
                    {row.original.teacherEmail ? (
                        <span className="text-muted-foreground text-xs">
                            {row.original.teacherEmail}
                        </span>
                    ) : null}
                </div>
            ),
        },
        {
            accessorKey: "provider",
            header: () => t("admin.payments.provider"),
            enableSorting: false,
            meta: { label: t("admin.payments.provider") },
            cell: ({ row }) => <Badge variant="outline">{row.original.provider}</Badge>,
        },
        {
            accessorKey: "amountVnd",
            header: () => t("admin.payments.amount"),
            enableSorting: true,
            meta: { label: t("admin.payments.amount") },
            cell: ({ row }) => (
                <span className="whitespace-nowrap font-medium">
                    {formatVnd(row.original.amountVnd, locale)}
                </span>
            ),
        },
        {
            accessorKey: "status",
            header: () => t("admin.payments.status"),
            enableSorting: true,
            meta: { label: t("admin.payments.status") },
            cell: ({ row }) => (
                <Badge variant={STATUS_VARIANT[row.original.status] ?? "secondary"}>
                    {t(`status.${row.original.status}`, { defaultValue: row.original.status })}
                </Badge>
            ),
        },
        {
            accessorKey: "createdAt",
            header: () => t("admin.payments.created"),
            enableSorting: true,
            meta: { label: t("admin.payments.created") },
            cell: ({ row }) => (
                <span className="text-muted-foreground whitespace-nowrap">
                    {formatDate(row.original.createdAt)}
                </span>
            ),
        },
    ];
}
