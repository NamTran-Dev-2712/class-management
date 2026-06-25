import { useQuery } from "@tanstack/react-query";
import { ClipboardList, Flag, School, UserPlus, Users } from "lucide-react";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { StatCard } from "@/components/shared/dashboard/stat-card";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { queryKeys } from "@/lib/query-keys";
import { adminService } from "@/services/admin/admin.service";
import { StatusBar } from "./charts/status-bar";
import { StatusDonut } from "./charts/status-donut";
import { UserGrowthChart } from "./charts/user-growth-chart";
import type { Route } from "./+types/dashboard.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Admin · Class Management" }];
}

export default function AdminDashboardPage() {
    const { t, i18n } = useTranslation("dashboard");
    const { t: tReport } = useTranslation("report");
    const { t: tAssignment } = useTranslation("assignment");
    const { data, isLoading } = useQuery({
        queryKey: queryKeys.admin.dashboard(),
        queryFn: () => adminService.dashboard(),
    });

    const value = (n: number | undefined) => (isLoading || n === undefined ? "—" : n.toString());

    const reportData = useMemo(
        () =>
            (data?.reportsByStatus ?? []).map((s) => ({
                name: tReport(`status.${s.status}`, { defaultValue: s.status }),
                value: s.count,
            })),
        [data?.reportsByStatus, tReport],
    );
    const assignmentData = useMemo(
        () =>
            (data?.assignmentsByStatus ?? []).map((s) => ({
                name: tAssignment(`status.${s.status}`, { defaultValue: s.status }),
                value: s.count,
            })),
        [data?.assignmentsByStatus, tAssignment],
    );

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("admin.welcome")}</h2>
                <p className="text-muted-foreground text-sm">{t("admin.subtitle")}</p>
            </div>

            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
                <StatCard
                    title={t("stats.totalUsers")}
                    value={value(data?.totalUsers)}
                    icon={Users}
                />
                <StatCard
                    title={t("stats.newUsers")}
                    value={value(data?.newUsersLast7Days)}
                    icon={UserPlus}
                />
                <StatCard
                    title={t("stats.activeClasses")}
                    value={value(data?.activeClasses)}
                    icon={School}
                />
                <StatCard
                    title={t("stats.openAssignments")}
                    value={value(data?.openAssignments)}
                    icon={ClipboardList}
                />
                <Link to="/admin/reports" className="rounded-xl">
                    <StatCard
                        title={t("stats.pendingReports")}
                        value={value(data?.pendingReports)}
                        icon={Flag}
                        hint={t("stats.reviewNow")}
                    />
                </Link>
            </div>

            <div className="grid gap-4 lg:grid-cols-3">
                <Card className="lg:col-span-2">
                    <CardHeader>
                        <CardTitle className="text-base">{t("charts.userGrowth")}</CardTitle>
                    </CardHeader>
                    <CardContent>
                        {isLoading || !data ? (
                            <Skeleton className="h-[260px] w-full" />
                        ) : (
                            <UserGrowthChart data={data.newUsersDaily} locale={i18n.language} />
                        )}
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-base">{t("charts.reportsByStatus")}</CardTitle>
                    </CardHeader>
                    <CardContent>
                        {isLoading || !data ? (
                            <Skeleton className="h-[260px] w-full" />
                        ) : (
                            <StatusDonut data={reportData} />
                        )}
                    </CardContent>
                </Card>
                <Card className="lg:col-span-3">
                    <CardHeader>
                        <CardTitle className="text-base">
                            {t("charts.assignmentsByStatus")}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        {isLoading || !data ? (
                            <Skeleton className="h-[260px] w-full" />
                        ) : (
                            <StatusBar data={assignmentData} />
                        )}
                    </CardContent>
                </Card>
            </div>
        </div>
    );
}
