import { useQuery } from "@tanstack/react-query";
import { Check } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { PageHero } from "@/components/shared/marketing/page-hero";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { formatVnd } from "@/lib/currency";
import { queryKeys } from "@/lib/query-keys";
import { cn } from "@/lib/utils";
import { planService } from "@/services/payment/payment.service";
import type { PlanDto } from "@/services/payment/dtos/payment-dtos";
import type { Route } from "./+types/pricing.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Pricing · Class Management" }];
}

function planFeatures(
    plan: PlanDto,
    tp: (k: string, o?: Record<string, unknown>) => string,
): string[] {
    const limit = (count: number | null, key: string, unlimitedKey: string) =>
        count == null ? tp(`plans.limits.${unlimitedKey}`) : tp(`plans.limits.${key}`, { count });
    return [
        limit(plan.maxClasses, "classes", "unlimitedClasses"),
        limit(plan.maxQuestions, "questions", "unlimitedQuestions"),
        limit(plan.maxExams, "exams", "unlimitedExams"),
        ...plan.features,
    ];
}

export default function PricingPage() {
    const { t } = useTranslation("public");
    const { t: tp, i18n } = useTranslation("payment");

    const { data: plans } = useQuery({
        queryKey: queryKeys.plans.list(),
        queryFn: () => planService.list(),
    });

    return (
        <>
            <PageHero title={t("pricing.title")} subtitle={t("pricing.subtitle")} />
            <section className="mx-auto max-w-6xl px-4 py-16 md:px-6">
                <div className="grid gap-6 md:grid-cols-3">
                    {(plans ?? []).map((plan) => {
                        const highlighted = plan.priceVnd > 0 && plan.billingCycle === "Monthly";
                        const period =
                            plan.priceVnd === 0
                                ? ""
                                : plan.billingCycle === "Annual"
                                  ? tp("plans.perYear")
                                  : tp("plans.perMonth");
                        return (
                            <Card
                                key={plan.publicId}
                                className={cn(
                                    "relative",
                                    highlighted && "border-primary ring-primary/30 ring-2",
                                )}
                            >
                                {highlighted ? (
                                    <span className="bg-primary text-primary-foreground absolute -top-3 left-1/2 -translate-x-1/2 rounded-full px-3 py-1 text-xs font-medium">
                                        {t("pricing.popular")}
                                    </span>
                                ) : null}
                                <CardHeader>
                                    <CardTitle>
                                        {plan.name}
                                        {plan.billingCycle
                                            ? ` · ${tp(`plans.billing.${plan.billingCycle}`)}`
                                            : ""}
                                    </CardTitle>
                                    <div className="mt-2">
                                        <span className="text-3xl font-bold">
                                            {plan.priceVnd === 0
                                                ? tp("plans.free")
                                                : formatVnd(plan.priceVnd, i18n.language)}
                                        </span>{" "}
                                        <span className="text-muted-foreground text-sm">
                                            {period}
                                        </span>
                                    </div>
                                    <CardDescription className="mt-2">
                                        {t("pricing.subtitle")}
                                    </CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <ul className="space-y-2 text-sm">
                                        {planFeatures(plan, tp).map((feature) => (
                                            <li key={feature} className="flex items-center gap-2">
                                                <Check className="text-primary size-4 shrink-0" />
                                                {feature}
                                            </li>
                                        ))}
                                    </ul>
                                </CardContent>
                                <CardFooter>
                                    <Button
                                        asChild
                                        className="w-full"
                                        variant={highlighted ? "default" : "outline"}
                                    >
                                        <Link to="/register">{t("pricing.cta")}</Link>
                                    </Button>
                                </CardFooter>
                            </Card>
                        );
                    })}
                </div>
            </section>
        </>
    );
}
