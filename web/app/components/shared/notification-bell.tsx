import { Bell, CheckCheck } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";

import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import {
    useNotificationMutations,
    useNotifications,
    useUnreadCount,
} from "@/features/notifications/notifications.hook";
import type { NotificationItem } from "@/services/notification/dtos/notification-dtos";

export function NotificationBell() {
    const { t, i18n } = useTranslation("notification");
    const navigate = useNavigate();
    const [open, setOpen] = useState(false);

    const { data: unread } = useUnreadCount();
    const { data, isLoading } = useNotifications({ pageNumber: 1, pageSize: 8 }, open);
    const { markRead, markAllRead } = useNotificationMutations();

    const count = unread?.count ?? 0;
    const items = data?.items ?? [];

    const formatTime = (iso: string) =>
        new Intl.DateTimeFormat(i18n.language, { dateStyle: "medium", timeStyle: "short" }).format(
            new Date(iso),
        );

    const localize = (item: NotificationItem, part: "title" | "body") => {
        const payload = (item.payload ?? {}) as Record<string, unknown>;
        const fallback = part === "title" ? item.title : (item.body ?? "");
        return t(`events.${item.eventType}.${part}`, { ...payload, defaultValue: fallback });
    };

    const onItemClick = (item: NotificationItem) => {
        if (item.status === "Unread") markRead.mutate(item.publicId);
        setOpen(false);
        if (item.link) navigate(item.link);
    };

    return (
        <DropdownMenu open={open} onOpenChange={setOpen}>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" className="relative" aria-label={t("title")}>
                    <Bell className="size-5" />
                    {count > 0 ? (
                        <span className="bg-destructive text-destructive-foreground absolute -top-0.5 -right-0.5 flex size-4 min-w-4 items-center justify-center rounded-full px-1 text-[10px] leading-none font-semibold">
                            {count > 99 ? "99+" : count}
                        </span>
                    ) : null}
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-80 p-0">
                <div className="flex items-center justify-between border-b px-3 py-2">
                    <span className="text-sm font-semibold">{t("title")}</span>
                    {count > 0 ? (
                        <button
                            type="button"
                            onClick={() => markAllRead.mutate()}
                            className="text-muted-foreground hover:text-foreground inline-flex items-center gap-1 text-xs"
                        >
                            <CheckCheck className="size-3.5" />
                            {t("markAllRead")}
                        </button>
                    ) : null}
                </div>

                <div className="max-h-96 overflow-y-auto">
                    {isLoading ? (
                        <p className="text-muted-foreground p-4 text-center text-sm">
                            {t("loading")}
                        </p>
                    ) : items.length === 0 ? (
                        <p className="text-muted-foreground p-6 text-center text-sm">
                            {t("empty")}
                        </p>
                    ) : (
                        items.map((item) => (
                            <button
                                key={item.publicId}
                                type="button"
                                onClick={() => onItemClick(item)}
                                className={cn(
                                    "hover:bg-accent flex w-full flex-col items-start gap-0.5 border-b px-3 py-2.5 text-left last:border-b-0",
                                    item.status === "Unread" && "bg-accent/40",
                                )}
                            >
                                <div className="flex w-full items-center gap-2">
                                    {item.status === "Unread" ? (
                                        <span className="bg-primary size-2 shrink-0 rounded-full" />
                                    ) : null}
                                    <span className="flex-1 text-sm font-medium">
                                        {localize(item, "title")}
                                    </span>
                                </div>
                                {localize(item, "body") ? (
                                    <span className="text-muted-foreground line-clamp-2 text-xs">
                                        {localize(item, "body")}
                                    </span>
                                ) : null}
                                <span className="text-muted-foreground text-[11px]">
                                    {formatTime(item.createdAt)}
                                </span>
                            </button>
                        ))
                    )}
                </div>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
