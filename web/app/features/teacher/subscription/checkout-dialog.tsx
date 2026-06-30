import { Loader2 } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { formatVnd } from "@/lib/currency";
import type { PaymentProvider, PlanDto } from "@/services/payment/dtos/payment-dtos";
import { ApiError } from "@/lib/api-error";
import { useCheckout } from "./subscription.hook";

interface CheckoutDialogProps {
    plan: PlanDto | null;
    onOpenChange: (open: boolean) => void;
}

export function CheckoutDialog({ plan, onOpenChange }: CheckoutDialogProps) {
    const { t, i18n } = useTranslation("payment");
    const [provider, setProvider] = useState<PaymentProvider>("Momo");
    const checkout = useCheckout();

    const onPay = () => {
        if (!plan) return;
        checkout.mutate(
            { planPublicId: plan.publicId, provider },
            {
                onSuccess: (result) => {
                    // Redirect to the gateway (Fake provider returns a local redirect URL).
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else {
                        onOpenChange(false);
                    }
                },
                onError: (err) =>
                    toast.error(
                        err instanceof ApiError
                            ? err.message
                            : t("subscription.toast.checkoutFailed"),
                    ),
            },
        );
    };

    return (
        <Dialog open={!!plan} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>{t("checkout.title")}</DialogTitle>
                    <DialogDescription>
                        {plan ? `${plan.name} · ${formatVnd(plan.priceVnd, i18n.language)}` : ""}
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-3">
                    <Label>{t("checkout.provider")}</Label>
                    <RadioGroup
                        value={provider}
                        onValueChange={(v) => setProvider(v as PaymentProvider)}
                        className="gap-3"
                    >
                        <Label className="flex items-center gap-3 rounded-md border p-3">
                            <RadioGroupItem value="Momo" />
                            {t("checkout.momo")}
                        </Label>
                        <Label className="flex items-center gap-3 rounded-md border p-3">
                            <RadioGroupItem value="VnPay" />
                            {t("checkout.vnpay")}
                        </Label>
                    </RadioGroup>
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        {t("actions.cancel", { ns: "common" })}
                    </Button>
                    <Button onClick={onPay} disabled={checkout.isPending}>
                        {checkout.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                        {plan
                            ? t("checkout.pay", { amount: formatVnd(plan.priceVnd, i18n.language) })
                            : t("checkout.title")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
