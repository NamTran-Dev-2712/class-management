import type { NavItem } from "@/config/nav";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";
import { MobileSidebar } from "./mobile-sidebar";
import { Sidebar } from "./sidebar";
import { SidebarProvider } from "./sidebar-context";
import { Topbar } from "./topbar";

interface DashboardShellProps {
    items: NavItem[];
    user: ProfileResponse;
    titleKey?: string;
    children: React.ReactNode;
}

/**
 * Shared application shell for every role dashboard: collapsible sidebar (desktop)
 * / slide-in sheet (mobile) + sticky topbar. Only the nav `items` and title differ
 * per role, so all dashboards look and behave consistently.
 */
export function DashboardShell({ items, user, titleKey, children }: DashboardShellProps) {
    return (
        <SidebarProvider>
            <div className="flex min-h-screen">
                <Sidebar items={items} />
                <MobileSidebar items={items} />
                <div className="flex min-w-0 flex-1 flex-col">
                    <Topbar titleKey={titleKey} user={user} />
                    <main className="flex-1 overflow-y-auto p-4 md:p-6">{children}</main>
                </div>
            </div>
        </SidebarProvider>
    );
}
