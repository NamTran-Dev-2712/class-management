import { useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, Loader2, XCircle } from "lucide-react";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Link, useSearchParams } from "react-router";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { apiClient } from "@/lib/axios.config";
import { queryKeys } from "@/lib/query-keys";
import { usePaymentStatus } from "./subscription.hook";
import type { Route } from "./+types/subscription-return.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Payment · Class Management" }];
}

export default function SubscriptionReturnPage() {
    const { t } = useTranslation("payment");
    const [searchParams] = useSearchParams();
    const paymentId = searchParams.get("paymentId");
    const orderRef = searchParams.get("orderRef");
    const simulated = searchParams.get("simulated") === "1";
    const qc = useQueryClient();

    const { data } = usePaymentStatus(paymentId);
    const status = data?.status ?? "Pending";

    useEffect(() => {
        if (status === "Completed") {
            qc.invalidateQueries({ queryKey: queryKeys.subscriptions.all });
        }
    }, [status, qc]);

    // Dev-only: the Fake provider has no real gateway/webhook, so let the developer drive the
    // server-to-server webhook from the browser to exercise the whole flow.
    const simulate = async (outcome: "Succeeded" | "Failed") => {
        if (!orderRef) return;
        await apiClient.post("/payments/webhook/momo", { orderRef, outcome });
        qc.invalidateQueries({ queryKey: queryKeys.subscriptions.paymentStatus(paymentId ?? "") });
    };

    return (
        <div className="mx-auto flex max-w-md flex-col gap-6 py-12">
            <Card>
                <CardHeader>
                    <CardTitle>{t("checkout.return.title")}</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4 text-center">
                    {status === "Pending" ? (
                        <div className="text-muted-foreground flex flex-col items-center gap-3">
                            <Loader2 className="size-8 animate-spin" />
                            <p>{t("checkout.return.pending")}</p>
                        </div>
                    ) : null}
                    {status === "Completed" ? (
                        <div className="flex flex-col items-center gap-3 text-green-600">
                            <CheckCircle2 className="size-8" />
                            <p>{t("checkout.return.completed")}</p>
                        </div>
                    ) : null}
                    {status === "Failed" ? (
                        <div className="text-destructive flex flex-col items-center gap-3">
                            <XCircle className="size-8" />
                            <p>{t("checkout.return.failed")}</p>
                        </div>
                    ) : null}
                    {status === "Expired" ? (
                        <div className="text-muted-foreground flex flex-col items-center gap-3">
                            <XCircle className="size-8" />
                            <p>{t("checkout.return.expired")}</p>
                        </div>
                    ) : null}

                    {simulated && status === "Pending" ? (
                        <div className="flex justify-center gap-2 pt-2">
                            <Button size="sm" onClick={() => simulate("Succeeded")}>
                                Simulate success
                            </Button>
                            <Button size="sm" variant="outline" onClick={() => simulate("Failed")}>
                                Simulate failure
                            </Button>
                        </div>
                    ) : null}

                    <Button asChild variant="ghost" className="w-full">
                        <Link to="/teacher/subscription">{t("checkout.return.back")}</Link>
                    </Button>
                </CardContent>
            </Card>
        </div>
    );
}
