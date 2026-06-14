import type { LucideIcon } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface StatCardProps {
    title: string;
    value: string;
    icon: LucideIcon;
    hint?: string;
}

export function StatCard({ title, value, icon: Icon, hint }: StatCardProps) {
    return (
        <Card>
            <CardHeader className="flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-muted-foreground text-sm font-medium">{title}</CardTitle>
                <Icon className="text-muted-foreground size-4" />
            </CardHeader>
            <CardContent>
                <div className="text-2xl font-bold">{value}</div>
                {hint ? <p className="text-muted-foreground mt-1 text-xs">{hint}</p> : null}
            </CardContent>
        </Card>
    );
}
