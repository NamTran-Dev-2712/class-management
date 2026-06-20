import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

import type { ReportBucket } from "@/services/assignment/dtos/queries/assignment-detail";

/** Responsive score-distribution histogram (students per score range). */
export function GradeHistogram({ buckets, yLabel }: { buckets: ReportBucket[]; yLabel: string }) {
    const data = buckets.map((b) => ({ label: b.label, count: b.count }));

    return (
        <ResponsiveContainer width="100%" height={260}>
            <BarChart data={data} margin={{ top: 8, right: 8, bottom: 4, left: -16 }}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" vertical={false} />
                <XAxis
                    dataKey="label"
                    tick={{ fontSize: 11 }}
                    interval={0}
                    angle={-30}
                    textAnchor="end"
                    height={48}
                />
                <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                <Tooltip cursor={{ fill: "var(--color-muted)", opacity: 0.3 }} />
                <Bar
                    dataKey="count"
                    name={yLabel}
                    fill="var(--color-primary)"
                    radius={[4, 4, 0, 0]}
                />
            </BarChart>
        </ResponsiveContainer>
    );
}
