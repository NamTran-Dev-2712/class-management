import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

import { DataTable } from "@/components/shared/data-table/data-table";
import { DataTablePagination } from "@/components/shared/data-table/data-table-pagination";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import type { PaymentProvider, SubscriptionStatus } from "@/services/payment/dtos/payment-dtos";
import { getAdminPaymentColumns } from "./admin-payment-columns";
import { getAdminSubscriptionColumns } from "./admin-subscription-columns";
import { ManualSetDialog } from "./manual-set-dialog";
import { useAdminPayments, useAdminSubscriptions } from "./payments.hook";
import { RevenuePanel } from "./revenue-panel";
import type { Route } from "./+types/payments.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Payments · Class Management" }];
}

type Tab = "subscriptions" | "payments" | "revenue";

export default function AdminPaymentsPage() {
    const { t } = useTranslation("payment");
    const [tab, setTab] = useState<Tab>("subscriptions");

    const tabs: { key: Tab; label: string }[] = [
        { key: "subscriptions", label: t("admin.tabs.subscriptions") },
        { key: "payments", label: t("admin.tabs.payments") },
        { key: "revenue", label: t("admin.tabs.revenue") },
    ];

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("admin.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("admin.subtitle")}</p>
            </div>

            <div className="border-border inline-flex gap-1 rounded-lg border p-1">
                {tabs.map((tb) => (
                    <button
                        key={tb.key}
                        type="button"
                        onClick={() => setTab(tb.key)}
                        className={cn(
                            "rounded-md px-3 py-1.5 text-sm font-medium transition-colors",
                            tab === tb.key
                                ? "bg-primary text-primary-foreground"
                                : "text-muted-foreground hover:text-foreground",
                        )}
                    >
                        {tb.label}
                    </button>
                ))}
            </div>

            {tab === "subscriptions" ? <SubscriptionsPanel /> : null}
            {tab === "payments" ? <PaymentsPanel /> : null}
            {tab === "revenue" ? <RevenuePanel /> : null}
        </div>
    );
}

function SubscriptionsPanel() {
    const { t, i18n } = useTranslation("payment");
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [status, setStatus] = useState<string>("all");
    const [manualOpen, setManualOpen] = useState(false);

    const { data, isLoading } = useAdminSubscriptions({
        pageNumber: page,
        pageSize,
        sortOrder: "desc",
        sortBy: "createdat",
        searchTerm: searchTerm || undefined,
        status: status === "all" ? undefined : (status as SubscriptionStatus),
    });

    const columns = useMemo(
        () => getAdminSubscriptionColumns(t, i18n.language),
        [t, i18n.language],
    );

    return (
        <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-2">
                <Input
                    value={searchTerm}
                    onChange={(e) => {
                        setSearchTerm(e.target.value);
                        setPage(1);
                    }}
                    placeholder={t("admin.subscriptions.search")}
                    className="max-w-xs"
                />
                <Select
                    value={status}
                    onValueChange={(v) => {
                        setStatus(v);
                        setPage(1);
                    }}
                >
                    <SelectTrigger className="w-44">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">{t("admin.subscriptions.allStatuses")}</SelectItem>
                        {(["Active", "PastDue", "Cancelled", "Expired"] as const).map((s) => (
                            <SelectItem key={s} value={s}>
                                {t(`subscription.statusValue.${s}`)}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
                <Button className="ml-auto" onClick={() => setManualOpen(true)}>
                    {t("admin.subscriptions.manualSet")}
                </Button>
            </div>

            <DataTable columns={columns} data={data?.items ?? []} isLoading={isLoading} />

            {data && data.items.length > 0 ? (
                <DataTablePagination
                    pageNumber={data.pageNumber}
                    pageSize={data.pageSize}
                    totalPages={data.totalPages}
                    totalCount={data.totalCount}
                    hasPreviousPage={data.hasPreviousPage}
                    hasNextPage={data.hasNextPage}
                    onPageChange={setPage}
                    onPageSizeChange={(s) => {
                        setPageSize(s);
                        setPage(1);
                    }}
                />
            ) : null}

            <ManualSetDialog open={manualOpen} onOpenChange={setManualOpen} />
        </div>
    );
}

function PaymentsPanel() {
    const { t, i18n } = useTranslation("payment");
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [status, setStatus] = useState<string>("all");
    const [provider, setProvider] = useState<string>("all");

    const { data, isLoading } = useAdminPayments({
        pageNumber: page,
        pageSize,
        sortOrder: "desc",
        sortBy: "createdat",
        searchTerm: searchTerm || undefined,
        status: status === "all" ? undefined : status,
        provider: provider === "all" ? undefined : (provider as PaymentProvider),
    });

    const columns = useMemo(() => getAdminPaymentColumns(t, i18n.language), [t, i18n.language]);

    return (
        <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-2">
                <Input
                    value={searchTerm}
                    onChange={(e) => {
                        setSearchTerm(e.target.value);
                        setPage(1);
                    }}
                    placeholder={t("admin.payments.search")}
                    className="max-w-xs"
                />
                <Select
                    value={status}
                    onValueChange={(v) => {
                        setStatus(v);
                        setPage(1);
                    }}
                >
                    <SelectTrigger className="w-40">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">{t("admin.payments.allStatuses")}</SelectItem>
                        {(["Pending", "Completed", "Failed", "Expired"] as const).map((s) => (
                            <SelectItem key={s} value={s}>
                                {t(`status.${s}`)}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
                <Select
                    value={provider}
                    onValueChange={(v) => {
                        setProvider(v);
                        setPage(1);
                    }}
                >
                    <SelectTrigger className="w-40">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">{t("admin.payments.allProviders")}</SelectItem>
                        <SelectItem value="Momo">Momo</SelectItem>
                        <SelectItem value="VnPay">VNPay</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <DataTable columns={columns} data={data?.items ?? []} isLoading={isLoading} />

            {data && data.items.length > 0 ? (
                <DataTablePagination
                    pageNumber={data.pageNumber}
                    pageSize={data.pageSize}
                    totalPages={data.totalPages}
                    totalCount={data.totalCount}
                    hasPreviousPage={data.hasPreviousPage}
                    hasNextPage={data.hasNextPage}
                    onPageChange={setPage}
                    onPageSizeChange={(s) => {
                        setPageSize(s);
                        setPage(1);
                    }}
                />
            ) : null}
        </div>
    );
}
