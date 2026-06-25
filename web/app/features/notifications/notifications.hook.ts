import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { notificationService } from "@/services/notification/notification.service";
import type { NotificationListQuery } from "@/services/notification/dtos/notification-dtos";

/** Polling fallback cadence for the bell badge — SignalR pushes updates instantly (MVP-7.5); this is a
 *  safety net for when the socket is unavailable. */
const POLL_INTERVAL_MS = 60_000;

export function useUnreadCount(enabled = true) {
    return useQuery({
        queryKey: queryKeys.notifications.unreadCount(),
        queryFn: () => notificationService.unreadCount(),
        refetchInterval: POLL_INTERVAL_MS,
        refetchOnWindowFocus: true,
        enabled,
    });
}

export function useNotifications(query: NotificationListQuery, enabled = true) {
    return useQuery({
        queryKey: queryKeys.notifications.list(query),
        queryFn: () => notificationService.list(query),
        placeholderData: (prev) => prev,
        enabled,
    });
}

export function useNotificationMutations() {
    const qc = useQueryClient();
    const invalidate = () => qc.invalidateQueries({ queryKey: queryKeys.notifications.all });

    const markRead = useMutation({
        mutationFn: (publicId: string) => notificationService.markRead(publicId),
        onSuccess: invalidate,
    });
    const markAllRead = useMutation({
        mutationFn: () => notificationService.markAllRead(),
        onSuccess: invalidate,
    });
    const archive = useMutation({
        mutationFn: (publicId: string) => notificationService.archive(publicId),
        onSuccess: invalidate,
    });

    return { markRead, markAllRead, archive };
}
