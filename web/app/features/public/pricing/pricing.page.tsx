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
import { cn } from "@/lib/utils";
import type { Route } from "./+types/pricing.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Pricing · Class Management" }];
}

const tiers = [
    { key: "free", highlighted: false },
    { key: "pro", highlighted: true },
    { key: "institution", highlighted: false },
] as const;

export default function PricingPage() {
    const { t } = useTranslation("public");

    return (
        <>
            <PageHero title={t("pricing.title")} subtitle={t("pricing.subtitle")} />
            <section className="mx-auto max-w-6xl px-4 py-16 md:px-6">
                <div className="grid gap-6 md:grid-cols-3">
                    {tiers.map((tier) => {
                        const features = t(`pricing.${tier.key}.features`, {
                            returnObjects: true,
                        }) as unknown as string[];

                        return (
                            <Card
                                key={tier.key}
                                className={cn(
                                    "relative",
                                    tier.highlighted && "border-primary ring-primary/30 ring-2",
                                )}
                            >
                                {tier.highlighted ? (
                                    <span className="bg-primary text-primary-foreground absolute -top-3 left-1/2 -translate-x-1/2 rounded-full px-3 py-1 text-xs font-medium">
                                        {t("pricing.popular")}
                                    </span>
                                ) : null}
                                <CardHeader>
                                    <CardTitle>{t(`pricing.${tier.key}.name`)}</CardTitle>
                                    <div className="mt-2">
                                        <span className="text-3xl font-bold">
                                            {t(`pricing.${tier.key}.price`)}
                                        </span>{" "}
                                        <span className="text-muted-foreground text-sm">
                                            {t(`pricing.${tier.key}.period`)}
                                        </span>
                                    </div>
                                    <CardDescription className="mt-2">
                                        {t(`pricing.${tier.key}.desc`)}
                                    </CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <ul className="space-y-2 text-sm">
                                        {features.map((feature) => (
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
                                        variant={tier.highlighted ? "default" : "outline"}
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
