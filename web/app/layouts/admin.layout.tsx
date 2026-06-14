import { Outlet } from "react-router";

import { DashboardShell } from "@/components/layout/dashboard/dashboard-shell";
import { dashboardNav } from "@/config/nav";
import { Roles } from "@/config/roles";
import { requireRole } from "@/guards/require-role";
import { userContext } from "@/lib/auth-context";
import type { Route } from "./+types/admin.layout";

export function loader({ context, request }: Route.LoaderArgs) {
    const user = requireRole(context.get(userContext), [Roles.Admin], request);
    return { user };
}

export default function AdminLayout({ loaderData }: Route.ComponentProps) {
    return (
        <DashboardShell
            items={dashboardNav[Roles.Admin]}
            user={loaderData.user}
            titleKey="admin.title"
        >
            <Outlet />
        </DashboardShell>
    );
}
