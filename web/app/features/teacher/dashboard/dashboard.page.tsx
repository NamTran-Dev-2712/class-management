import { ClipboardList, FileQuestion, FileText, School } from "lucide-react";
import { useTranslation } from "react-i18next";

import { StatCard } from "@/components/shared/dashboard/stat-card";
import type { Route } from "./+types/dashboard.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Teacher · Class Management" }];
}

export default function TeacherDashboardPage() {
    const { t } = useTranslation("dashboard");

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("teacher.welcome")}</h2>
                <p className="text-muted-foreground text-sm">{t("teacher.subtitle")}</p>
            </div>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title={t("nav.classrooms")}
                    value="—"
                    icon={School}
                    hint={t("comingSoon")}
                />
                <StatCard
                    title={t("nav.questionBank")}
                    value="—"
                    icon={FileQuestion}
                    hint={t("comingSoon")}
                />
                <StatCard title={t("nav.exams")} value="—" icon={FileText} hint={t("comingSoon")} />
                <StatCard
                    title={t("nav.assignments")}
                    value="—"
                    icon={ClipboardList}
                    hint={t("comingSoon")}
                />
            </div>
        </div>
    );
}
