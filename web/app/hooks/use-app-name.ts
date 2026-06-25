import { useTranslation } from "react-i18next";

import { useAppConfigStore } from "@/stores/app-config.store";

/**
 * The displayed brand name: the live `app_name` system setting once loaded, falling back to the i18n
 * default (so SSR + the first client render match). Drives sidebar/header/footer branding (MVP-7.5).
 */
export function useAppName(): string {
    const { t } = useTranslation("common");
    const appName = useAppConfigStore((s) => s.appName);
    return appName ?? t("appName");
}
