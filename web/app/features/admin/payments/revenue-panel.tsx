import { useTranslation } from "react-i18next";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatVnd } from "@/lib/currency";
import { useRevenue } from "./payments.hook";

export function RevenuePanel() {
    const { t, i18n } = useTranslation("payment");
    const { data } = useRevenue(12);

    const chartData = (data?.byMonth ?? []).map((m) => ({
        name: m.month,
        value: m.revenueVnd,
    }));

    return (
        <div className="space-y-6">
            <div className="grid gap-4 sm:grid-cols-2">
                <Card>
                    <CardHeader className="pb-2">
                        <CardTitle className="text-sm font-medium">
                            {t("admin.revenue.total")}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">
                            {formatVnd(data?.totalRevenueVnd ?? 0, i18n.language)}
                        </div>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="pb-2">
                        <CardTitle className="text-sm font-medium">
                            {t("admin.revenue.completed")}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{data?.completedPayments ?? 0}</div>
                    </CardContent>
                </Card>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle className="text-base">
                        {t("admin.revenue.byMonth")} · {t("admin.revenue.months", { count: 12 })}
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <ResponsiveContainer width="100%" height={260}>
                        <BarChart
                            data={chartData}
                            margin={{ top: 8, right: 8, bottom: 4, left: 8 }}
                        >
                            <CartesianGrid
                                strokeDasharray="3 3"
                                className="stroke-muted"
                                vertical={false}
                            />
                            <XAxis
                                dataKey="name"
                                tick={{ fontSize: 11 }}
                                interval={0}
                                height={36}
                            />
                            <YAxis
                                tick={{ fontSize: 11 }}
                                width={64}
                                tickFormatter={(v) =>
                                    new Intl.NumberFormat(i18n.language, {
                                        notation: "compact",
                                    }).format(Number(v))
                                }
                            />
                            <Tooltip
                                cursor={{ fill: "var(--color-muted)", opacity: 0.3 }}
                                formatter={(v) => formatVnd(Number(v), i18n.language)}
                            />
                            <Bar
                                dataKey="value"
                                fill="var(--color-primary)"
                                radius={[4, 4, 0, 0]}
                            />
                        </BarChart>
                    </ResponsiveContainer>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle className="text-base">{t("admin.revenue.byPlan")}</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="divide-y">
                        {(data?.byPlan ?? []).map((p) => (
                            <div
                                key={p.planName}
                                className="flex items-center justify-between py-2 text-sm"
                            >
                                <span>
                                    {p.planName}{" "}
                                    <span className="text-muted-foreground">({p.count})</span>
                                </span>
                                <span className="font-medium">
                                    {formatVnd(p.revenueVnd, i18n.language)}
                                </span>
                            </div>
                        ))}
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
