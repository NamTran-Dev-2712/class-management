import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";

import { Badge } from "@/components/ui/badge";
import type { AdminSubscriptionDto } from "@/services/payment/dtos/payment-dtos";

const STATUS_VARIANT: Record<string, "secondary" | "outline" | "default" | "destructive"> = {
    Active: "default",
    PastDue: "secondary",
    Cancelled: "outline",
    Expired: "destructive",
};

export function getAdminSubscriptionColumns(
    t: TFunction,
    locale: string,
): ColumnDef<AdminSubscriptionDto>[] {
    const formatDate = (iso: string | null) =>
        iso ? new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(iso)) : "—";

    return [
        {
            accessorKey: "teacherName",
            header: () => t("admin.subscriptions.teacher"),
            enableSorting: false,
            meta: { label: t("admin.subscriptions.teacher") },
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
            accessorKey: "planName",
            header: () => t("admin.subscriptions.plan"),
            enableSorting: true,
            meta: { label: t("admin.subscriptions.plan") },
            cell: ({ row }) => (
                <span>
                    {row.original.planName}
                    {row.original.billingCycle
                        ? ` · ${t(`plans.billing.${row.original.billingCycle}`)}`
                        : ""}
                </span>
            ),
        },
        {
            accessorKey: "status",
            header: () => t("admin.subscriptions.status"),
            enableSorting: true,
            meta: { label: t("admin.subscriptions.status") },
            cell: ({ row }) => (
                <Badge variant={STATUS_VARIANT[row.original.status] ?? "secondary"}>
                    {t(`subscription.statusValue.${row.original.status}`, {
                        defaultValue: row.original.status,
                    })}
                </Badge>
            ),
        },
        {
            accessorKey: "paymentType",
            header: () => t("admin.subscriptions.type"),
            enableSorting: false,
            meta: { label: t("admin.subscriptions.type") },
            cell: ({ row }) => (
                <Badge variant="outline">
                    {t(`admin.subscriptions.paymentType.${row.original.paymentType}`, {
                        defaultValue: row.original.paymentType,
                    })}
                </Badge>
            ),
        },
        {
            accessorKey: "expiresAt",
            header: () => t("admin.subscriptions.expires"),
            enableSorting: true,
            meta: { label: t("admin.subscriptions.expires") },
            cell: ({ row }) => (
                <span className="text-muted-foreground whitespace-nowrap">
                    {formatDate(row.original.expiresAt)}
                </span>
            ),
        },
    ];
}
