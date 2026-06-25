import { Link } from "react-router";

import { brandIcon as BrandIcon } from "@/config/nav";
import { useAppName } from "@/hooks/use-app-name";
import { cn } from "@/lib/utils";

/** Logo + app name shown at the top of the sidebar (and mobile sheet). */
export function SidebarBrand({ collapsed = false }: { collapsed?: boolean }) {
    const appName = useAppName();

    return (
        <Link
            to="/"
            className={cn(
                "flex h-14 items-center gap-2 border-b px-4",
                collapsed && "justify-center px-0",
            )}
        >
            <BrandIcon className="text-primary size-6 shrink-0" />
            {!collapsed && <span className="truncate font-semibold">{appName}</span>}
        </Link>
    );
}
