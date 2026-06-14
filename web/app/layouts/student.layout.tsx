import { Outlet } from "react-router";

import { DashboardShell } from "@/components/layout/dashboard/dashboard-shell";
import { dashboardNav } from "@/config/nav";
import { Roles } from "@/config/roles";
import { requireRole } from "@/guards/require-role";
import { userContext } from "@/lib/auth-context";
import type { Route } from "./+types/student.layout";

export function loader({ context, request }: Route.LoaderArgs) {
    const user = requireRole(context.get(userContext), [Roles.Student], request);
    return { user };
}

export default function StudentLayout({ loaderData }: Route.ComponentProps) {
    return (
        <DashboardShell
            items={dashboardNav[Roles.Student]}
            user={loaderData.user}
            titleKey="student.title"
        >
            <Outlet />
        </DashboardShell>
    );
}
