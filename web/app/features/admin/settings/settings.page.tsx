import { useTranslation } from "react-i18next";

import { Skeleton } from "@/components/ui/skeleton";
import { SettingRow } from "./setting-row";
import { useSystemSettings } from "./settings.hook";
import type { Route } from "./+types/settings.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Settings · Class Management" }];
}

export default function AdminSettingsPage() {
    const { t } = useTranslation("settings");
    const { data, isLoading } = useSystemSettings();

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold">{t("title")}</h2>
                <p className="text-muted-foreground text-sm">{t("subtitle")}</p>
            </div>

            <div className="rounded-xl border px-4">
                {isLoading ? (
                    <div className="space-y-3 py-4">
                        {Array.from({ length: 6 }).map((_, i) => (
                            <Skeleton key={i} className="h-12 w-full" />
                        ))}
                    </div>
                ) : (
                    (data ?? []).map((setting) => (
                        <SettingRow key={setting.key} setting={setting} />
                    ))
                )}
            </div>
        </div>
    );
}
