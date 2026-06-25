import { Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";

export interface ChartDatum {
    name: string;
    value: number;
}

const PALETTE = ["#6366f1", "#22c55e", "#f59e0b", "#ef4444", "#06b6d4", "#a855f7"];

/** Responsive donut of a status breakdown (zero-value slices are dropped). */
export function StatusDonut({ data }: { data: ChartDatum[] }) {
    const slices = data.filter((d) => d.value > 0);

    if (slices.length === 0) {
        return null;
    }

    return (
        <ResponsiveContainer width="100%" height={260}>
            <PieChart>
                <Pie
                    data={slices}
                    dataKey="value"
                    nameKey="name"
                    innerRadius={55}
                    outerRadius={90}
                    paddingAngle={2}
                >
                    {slices.map((entry, i) => (
                        <Cell key={entry.name} fill={PALETTE[i % PALETTE.length]} />
                    ))}
                </Pie>
                <Tooltip />
                <Legend verticalAlign="bottom" height={28} wrapperStyle={{ fontSize: 12 }} />
            </PieChart>
        </ResponsiveContainer>
    );
}
