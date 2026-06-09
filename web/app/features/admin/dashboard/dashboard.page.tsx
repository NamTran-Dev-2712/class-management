import { BarChart3, CreditCard, School, Users } from "lucide-react";
import { useTranslation } from "react-i18next";

import { StatCard } from "@/components/shared/dashboard/stat-card";
import type { Route } from "./+types/dashboard.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Admin · Class Management" }];
}

export default function AdminDashboardPage() {
    const { t } = useTranslation("dashboard");

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("admin.welcome")}</h2>
                <p className="text-muted-foreground text-sm">{t("admin.subtitle")}</p>
            </div>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <StatCard title={t("nav.users")} value="—" icon={Users} hint={t("comingSoon")} />
                <StatCard
                    title={t("nav.classrooms")}
                    value="—"
                    icon={School}
                    hint={t("comingSoon")}
                />
                <StatCard
                    title={t("nav.payments")}
                    value="—"
                    icon={CreditCard}
                    hint={t("comingSoon")}
                />
                <StatCard
                    title={t("nav.reports")}
                    value="—"
                    icon={BarChart3}
                    hint={t("comingSoon")}
                />
            </div>
        </div>
    );
}
