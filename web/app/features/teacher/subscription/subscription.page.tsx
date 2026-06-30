import { Download, Loader2 } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { formatVnd } from "@/lib/currency";
import { teacherSubscriptionService } from "@/services/payment/payment.service";
import type { PlanDto, ResourceUsageItem } from "@/services/payment/dtos/payment-dtos";
import { CheckoutDialog } from "./checkout-dialog";
import {
    useCancelSubscription,
    useInvoices,
    useMySubscription,
    usePlans,
    useReactivateSubscription,
    useUsage,
} from "./subscription.hook";
import type { Route } from "./+types/subscription.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Subscription · Class Management" }];
}

function UsageBar({
    label,
    item,
    t,
}: {
    label: string;
    item: ResourceUsageItem;
    t: ReturnType<typeof useTranslation>["t"];
}) {
    const unlimited = item.limit === 0;
    const pct = unlimited ? 0 : Math.min(100, Math.round((item.used / item.limit) * 100));
    const atLimit = !unlimited && item.used >= item.limit;
    return (
        <div className="space-y-1">
            <div className="flex items-center justify-between text-sm">
                <span>{label}</span>
                <span className="text-muted-foreground">
                    {unlimited
                        ? t("usage.unlimited")
                        : t("usage.ofLimit", { used: item.used, limit: item.limit })}
                </span>
            </div>
            {!unlimited ? (
                <div className="bg-muted h-2 w-full overflow-hidden rounded-full">
                    <div
                        className={atLimit ? "bg-destructive h-full" : "bg-primary h-full"}
                        style={{ width: `${pct}%` }}
                    />
                </div>
            ) : null}
        </div>
    );
}

export default function SubscriptionPage() {
    const { t, i18n } = useTranslation("payment");
    const subscription = useMySubscription();
    const usage = useUsage();
    const plans = usePlans();
    const invoices = useInvoices();
    const cancel = useCancelSubscription();
    const reactivate = useReactivateSubscription();

    const [checkoutPlan, setCheckoutPlan] = useState<PlanDto | null>(null);
    const [confirmCancel, setConfirmCancel] = useState(false);
    const [downloadingId, setDownloadingId] = useState<string | null>(null);

    const sub = subscription.data;
    const isPro = sub?.isPro ?? false;
    const isCancelled = !!sub?.cancelledAt;

    const onDownload = async (invoiceId: string, invoiceNumber: string) => {
        setDownloadingId(invoiceId);
        try {
            const blob = await teacherSubscriptionService.downloadInvoice(invoiceId);
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = `${invoiceNumber}.pdf`;
            a.click();
            URL.revokeObjectURL(url);
        } catch {
            toast.error(t("subscription.toast.checkoutFailed"));
        } finally {
            setDownloadingId(null);
        }
    };

    const proPlans = (plans.data ?? []).filter((p) => p.priceVnd > 0);

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("subscription.title")}</h2>
                <p className="text-muted-foreground text-sm">{t("subscription.subtitle")}</p>
            </div>

            <div className="grid gap-6 lg:grid-cols-2">
                {/* Current plan + status */}
                <Card>
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            {t("subscription.currentPlan")}
                            <Badge variant={isPro ? "default" : "secondary"}>
                                {isPro ? t("subscription.pro") : t("subscription.free")}
                            </Badge>
                        </CardTitle>
                        <CardDescription>
                            {sub
                                ? t(`subscription.statusValue.${sub.status}`, {
                                      defaultValue: sub.status,
                                  })
                                : ""}
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-3 text-sm">
                        {sub?.expiresAt ? (
                            <div className="flex justify-between">
                                <span className="text-muted-foreground">
                                    {isCancelled
                                        ? t("subscription.expiresOn")
                                        : t("subscription.renewsOn")}
                                </span>
                                <span>
                                    {new Date(sub.expiresAt).toLocaleDateString(i18n.language)}
                                </span>
                            </div>
                        ) : null}

                        {sub?.status === "PastDue" && sub.gracePeriodEndsAt ? (
                            <p className="text-destructive">
                                {t("subscription.graceNote", {
                                    date: new Date(sub.gracePeriodEndsAt).toLocaleDateString(
                                        i18n.language,
                                    ),
                                })}
                            </p>
                        ) : null}

                        {isCancelled && sub?.expiresAt ? (
                            <p className="text-muted-foreground">
                                {t("subscription.cancelledNote", {
                                    date: new Date(sub.expiresAt).toLocaleDateString(i18n.language),
                                })}
                            </p>
                        ) : null}

                        <div className="flex gap-2 pt-2">
                            {isPro && !isCancelled ? (
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setConfirmCancel(true)}
                                >
                                    {t("subscription.actions.cancel")}
                                </Button>
                            ) : null}
                            {isPro && isCancelled ? (
                                <Button
                                    variant="outline"
                                    size="sm"
                                    disabled={reactivate.isPending}
                                    onClick={() =>
                                        reactivate.mutate(undefined, {
                                            onSuccess: () =>
                                                toast.success(t("subscription.toast.reactivated")),
                                        })
                                    }
                                >
                                    {reactivate.isPending ? (
                                        <Loader2 className="size-4 animate-spin" />
                                    ) : null}
                                    {t("subscription.actions.reactivate")}
                                </Button>
                            ) : null}
                        </div>
                    </CardContent>
                </Card>

                {/* Usage */}
                <Card>
                    <CardHeader>
                        <CardTitle>{t("usage.title")}</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        {usage.data ? (
                            <>
                                <UsageBar
                                    label={t("usage.classes")}
                                    item={usage.data.classes}
                                    t={t}
                                />
                                <UsageBar
                                    label={t("usage.questions")}
                                    item={usage.data.questions}
                                    t={t}
                                />
                                <UsageBar label={t("usage.exams")} item={usage.data.exams} t={t} />
                            </>
                        ) : (
                            <p className="text-muted-foreground text-sm">…</p>
                        )}
                    </CardContent>
                </Card>
            </div>

            {/* Plans */}
            <Card>
                <CardHeader>
                    <CardTitle>{t("plans.title")}</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="grid gap-4 md:grid-cols-2">
                        {proPlans.map((plan) => (
                            <div
                                key={plan.publicId}
                                className="flex flex-col rounded-lg border p-4"
                            >
                                <div className="flex items-baseline justify-between">
                                    <span className="font-medium">
                                        {plan.name} ·{" "}
                                        {plan.billingCycle
                                            ? t(`plans.billing.${plan.billingCycle}`)
                                            : ""}
                                    </span>
                                </div>
                                <div className="mt-1 text-2xl font-bold">
                                    {formatVnd(plan.priceVnd, i18n.language)}
                                    <span className="text-muted-foreground ml-1 text-sm font-normal">
                                        {plan.billingCycle === "Annual"
                                            ? t("plans.perYear")
                                            : t("plans.perMonth")}
                                    </span>
                                </div>
                                <ul className="text-muted-foreground mt-3 flex-1 space-y-1 text-sm">
                                    <li>{t("plans.limits.unlimitedClasses")}</li>
                                    <li>{t("plans.limits.unlimitedQuestions")}</li>
                                    <li>{t("plans.limits.unlimitedExams")}</li>
                                </ul>
                                <Button className="mt-4" onClick={() => setCheckoutPlan(plan)}>
                                    {isPro ? t("subscription.actions.renew") : t("plans.select")}
                                </Button>
                            </div>
                        ))}
                    </div>
                </CardContent>
            </Card>

            {/* Invoices */}
            <Card>
                <CardHeader>
                    <CardTitle>{t("invoices.title")}</CardTitle>
                </CardHeader>
                <CardContent>
                    {invoices.data && invoices.data.length > 0 ? (
                        <div className="divide-y">
                            {invoices.data.map((inv) => (
                                <div
                                    key={inv.publicId}
                                    className="flex items-center justify-between py-2 text-sm"
                                >
                                    <div>
                                        <div className="font-medium">{inv.invoiceNumber}</div>
                                        <div className="text-muted-foreground">
                                            {new Date(inv.issuedAt).toLocaleDateString(
                                                i18n.language,
                                            )}{" "}
                                            · {inv.planName}
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-3">
                                        <span>{formatVnd(inv.amountVnd, i18n.language)}</span>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            disabled={downloadingId === inv.publicId}
                                            onClick={() =>
                                                onDownload(inv.publicId, inv.invoiceNumber)
                                            }
                                        >
                                            {downloadingId === inv.publicId ? (
                                                <Loader2 className="size-4 animate-spin" />
                                            ) : (
                                                <Download className="size-4" />
                                            )}
                                            {t("invoices.download")}
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <p className="text-muted-foreground text-sm">{t("invoices.empty")}</p>
                    )}
                </CardContent>
            </Card>

            <CheckoutDialog plan={checkoutPlan} onOpenChange={(o) => !o && setCheckoutPlan(null)} />

            <ConfirmDialog
                open={confirmCancel}
                onOpenChange={setConfirmCancel}
                title={t("subscription.cancelConfirm.title")}
                description={t("subscription.cancelConfirm.description")}
                confirmLabel={t("subscription.cancelConfirm.confirm")}
                destructive
                isPending={cancel.isPending}
                onConfirm={() =>
                    cancel.mutate(undefined, {
                        onSuccess: () => {
                            toast.success(t("subscription.toast.cancelled"));
                            setConfirmCancel(false);
                        },
                    })
                }
            />
        </div>
    );
}
