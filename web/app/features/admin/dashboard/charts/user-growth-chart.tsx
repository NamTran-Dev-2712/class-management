import {
    Area,
    AreaChart,
    CartesianGrid,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis,
} from "recharts";

import type { DailyCount } from "@/services/admin/admin.service";

/** Responsive area chart of new-user registrations per day (last 30 days). */
export function UserGrowthChart({ data, locale }: { data: DailyCount[]; locale: string }) {
    const fmt = new Intl.DateTimeFormat(locale, { month: "short", day: "numeric" });
    const points = data.map((d) => ({
        label: fmt.format(new Date(`${d.date}T00:00:00`)),
        count: d.count,
    }));

    return (
        <ResponsiveContainer width="100%" height={260}>
            <AreaChart data={points} margin={{ top: 8, right: 8, bottom: 4, left: -16 }}>
                <defs>
                    <linearGradient id="userGrowthFill" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="var(--color-primary)" stopOpacity={0.35} />
                        <stop offset="95%" stopColor="var(--color-primary)" stopOpacity={0} />
                    </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" vertical={false} />
                <XAxis dataKey="label" tick={{ fontSize: 11 }} interval={4} minTickGap={16} />
                <YAxis allowDecimals={false} tick={{ fontSize: 11 }} width={32} />
                <Tooltip cursor={{ stroke: "var(--color-muted)" }} />
                <Area
                    type="monotone"
                    dataKey="count"
                    stroke="var(--color-primary)"
                    strokeWidth={2}
                    fill="url(#userGrowthFill)"
                />
            </AreaChart>
        </ResponsiveContainer>
    );
}
