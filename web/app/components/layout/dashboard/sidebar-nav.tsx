import { useTranslation } from "react-i18next";
import { NavLink } from "react-router";

import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import type { NavItem } from "@/config/nav";
import { cn } from "@/lib/utils";

interface SidebarNavProps {
    items: NavItem[];
    /** Icon-only rail (desktop collapsed). */
    collapsed?: boolean;
    /** Called after navigating — used to close the mobile sheet. */
    onNavigate?: () => void;
}

const rowBase =
    "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors";

export function SidebarNav({ items, collapsed = false, onNavigate }: SidebarNavProps) {
    const { t } = useTranslation("dashboard");

    return (
        <nav className="flex flex-col gap-1 px-2">
            {items.map((item) => {
                const label = t(item.labelKey);
                const Icon = item.icon;

                const inner = (
                    <>
                        <Icon className="size-4 shrink-0" />
                        {!collapsed && <span className="truncate">{label}</span>}
                        {!collapsed && item.disabled && (
                            <span className="bg-muted text-muted-foreground ml-auto rounded px-1.5 py-0.5 text-[10px]">
                                {t("comingSoon")}
                            </span>
                        )}
                    </>
                );

                const row = item.disabled ? (
                    <span
                        aria-disabled="true"
                        className={cn(
                            rowBase,
                            "text-muted-foreground/50 cursor-not-allowed",
                            collapsed && "justify-center",
                        )}
                    >
                        {inner}
                    </span>
                ) : (
                    <NavLink
                        to={item.to}
                        end
                        onClick={onNavigate}
                        className={({ isActive }) =>
                            cn(
                                rowBase,
                                isActive
                                    ? "bg-accent text-accent-foreground"
                                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground",
                                collapsed && "justify-center",
                            )
                        }
                    >
                        {inner}
                    </NavLink>
                );

                if (!collapsed) return <div key={item.to}>{row}</div>;

                return (
                    <Tooltip key={item.to}>
                        <TooltipTrigger asChild>{row}</TooltipTrigger>
                        <TooltipContent side="right">{label}</TooltipContent>
                    </Tooltip>
                );
            })}
        </nav>
    );
}
