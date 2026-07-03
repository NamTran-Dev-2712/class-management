import { Link } from "react-router";
import { useTranslation } from "react-i18next";

import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import { formatBytes } from "@/lib/format";
import { useStorageUsage } from "./media.hook";

/** Storage usage meter vs. the teacher's plan cap (0 = unlimited). Mirrors the subscription usage bars. */
export function StorageUsageBar() {
    const { t } = useTranslation("media");
    const { data } = useStorageUsage();
    if (!data) return null;

    const unlimited = data.limitBytes === 0;
    const pct = unlimited ? 0 : Math.min(100, Math.round((data.usedBytes / data.limitBytes) * 100));
    const atLimit = !unlimited && data.usedBytes >= data.limitBytes;

    return (
        <Card>
            <CardContent className="space-y-2 p-4">
                <div className="flex items-center justify-between gap-3">
                    <span className="text-sm font-medium">{t("usage.title")}</span>
                    <span className="text-muted-foreground text-sm">
                        {unlimited
                            ? t("usage.usedUnlimited", { used: formatBytes(data.usedBytes) })
                            : t("usage.used", {
                                  used: formatBytes(data.usedBytes),
                                  limit: formatBytes(data.limitBytes),
                              })}
                    </span>
                </div>
                {unlimited ? (
                    <p className="text-muted-foreground text-xs">{t("usage.unlimited")}</p>
                ) : (
                    <div className="bg-muted h-2 w-full overflow-hidden rounded-full">
                        <div
                            className={cn(
                                "h-full rounded-full transition-all",
                                atLimit ? "bg-destructive" : "bg-primary",
                            )}
                            style={{ width: `${pct}%` }}
                        />
                    </div>
                )}
                {atLimit ? (
                    <div className="flex items-center justify-between gap-2">
                        <span className="text-destructive text-xs">{t("usage.limitReached")}</span>
                        {!data.isPro ? (
                            <Link
                                to="/teacher/subscription"
                                className="text-primary text-xs font-medium underline"
                            >
                                {t("usage.upgrade")}
                            </Link>
                        ) : null}
                    </div>
                ) : null}
            </CardContent>
        </Card>
    );
}
