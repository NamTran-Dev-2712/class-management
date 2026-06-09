import { Award, BookOpen, ClipboardList, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";

import { StatCard } from "@/components/shared/dashboard/stat-card";
import type { Route } from "./+types/dashboard.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Student · Class Management" }];
}

export default function StudentDashboardPage() {
    const { t } = useTranslation("dashboard");

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("student.welcome")}</h2>
                <p className="text-muted-foreground text-sm">{t("student.subtitle")}</p>
            </div>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title={t("nav.myClasses")}
                    value="—"
                    icon={BookOpen}
                    hint={t("comingSoon")}
                />
                <StatCard title={t("nav.exams")} value="—" icon={FileText} hint={t("comingSoon")} />
                <StatCard
                    title={t("nav.assignments")}
                    value="—"
                    icon={ClipboardList}
                    hint={t("comingSoon")}
                />
                <StatCard title={t("nav.grades")} value="—" icon={Award} hint={t("comingSoon")} />
            </div>
        </div>
    );
}
