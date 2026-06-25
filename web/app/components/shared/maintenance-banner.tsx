import { Wrench } from "lucide-react";
import { useTranslation } from "react-i18next";

import { useAppConfigStore } from "@/stores/app-config.store";

/** Thin app-wide banner shown while the platform is in maintenance mode (MVP-7.5). */
export function MaintenanceBanner() {
    const { t } = useTranslation("common");
    const maintenanceMode = useAppConfigStore((s) => s.maintenanceMode);

    if (!maintenanceMode) return null;

    return (
        <div className="bg-amber-500/95 text-amber-950 dark:bg-amber-400 dark:text-amber-950">
            <div className="mx-auto flex max-w-7xl items-center justify-center gap-2 px-4 py-2 text-center text-sm font-medium">
                <Wrench className="size-4 shrink-0" />
                <span>{t("maintenance.banner")}</span>
            </div>
        </div>
    );
}
