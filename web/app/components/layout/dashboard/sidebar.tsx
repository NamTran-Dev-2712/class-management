import type { NavItem } from "@/config/nav";
import { cn } from "@/lib/utils";
import { SidebarBrand } from "./sidebar-brand";
import { SidebarNav } from "./sidebar-nav";
import { useSidebar } from "./sidebar-context";

/** Desktop sidebar — collapses to an icon rail. Hidden below `md`. */
export function Sidebar({ items }: { items: NavItem[] }) {
    const { collapsed } = useSidebar();

    return (
        <aside
            className={cn(
                "bg-card hidden shrink-0 flex-col border-r transition-[width] duration-300 ease-in-out md:flex",
                collapsed ? "md:w-16" : "md:w-64",
            )}
        >
            <SidebarBrand collapsed={collapsed} />
            <div className="flex-1 overflow-y-auto py-2">
                <SidebarNav items={items} collapsed={collapsed} />
            </div>
        </aside>
    );
}
