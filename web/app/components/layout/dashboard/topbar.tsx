import { Menu, PanelLeft } from "lucide-react";
import { useTranslation } from "react-i18next";

import { LanguageSwitcher } from "@/components/shared/language-switcher";
import { NotificationBell } from "@/components/shared/notification-bell";
import { Button } from "@/components/ui/button";
import { UserMenu } from "@/components/shared/user-menu";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";
import { useSidebar } from "./sidebar-context";

interface TopbarProps {
    /** Translation key (in the `dashboard` namespace) for the page title. */
    titleKey?: string;
    user: ProfileResponse;
}

export function Topbar({ titleKey, user }: TopbarProps) {
    const { t } = useTranslation("dashboard");
    const { toggleCollapsed, setMobileOpen } = useSidebar();

    return (
        <header className="bg-background sticky top-0 z-30 flex h-14 items-center gap-2 border-b px-4">
            <Button
                variant="ghost"
                size="icon"
                className="md:hidden"
                aria-label={t("openMenu")}
                onClick={() => setMobileOpen(true)}
            >
                <Menu className="size-5" />
            </Button>
            <Button
                variant="ghost"
                size="icon"
                className="hidden md:inline-flex"
                aria-label={t("toggleSidebar")}
                onClick={toggleCollapsed}
            >
                <PanelLeft className="size-5" />
            </Button>

            {titleKey ? (
                <h1 className="truncate text-sm font-semibold md:text-base">{t(titleKey)}</h1>
            ) : null}

            <div className="ml-auto flex items-center gap-2">
                <NotificationBell />
                <LanguageSwitcher />
                <UserMenu user={user} />
            </div>
        </header>
    );
}
