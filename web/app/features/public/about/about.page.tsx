import { Heart, Target, Users } from "lucide-react";
import { useTranslation } from "react-i18next";

import { FeatureCard } from "@/components/shared/marketing/feature-card";
import { PageHero } from "@/components/shared/marketing/page-hero";
import type { Route } from "./+types/about.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "About · Class Management" }];
}

const values = [
    { icon: Target, key: "mission" },
    { icon: Heart, key: "quality" },
    { icon: Users, key: "community" },
] as const;

export default function AboutPage() {
    const { t } = useTranslation("public");

    return (
        <>
            <PageHero title={t("about.title")} subtitle={t("about.subtitle")} />
            <section className="mx-auto max-w-3xl px-4 py-12 md:px-6">
                <p className="text-muted-foreground leading-relaxed">{t("about.body")}</p>
            </section>
            <section className="mx-auto max-w-6xl px-4 pb-16 md:px-6">
                <div className="grid gap-4 sm:grid-cols-3">
                    {values.map((v) => (
                        <FeatureCard
                            key={v.key}
                            icon={v.icon}
                            title={t(`about.values.${v.key}.title`)}
                            description={t(`about.values.${v.key}.desc`)}
                        />
                    ))}
                </div>
            </section>
        </>
    );
}
