import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";

import { createNotificationConnection } from "@/lib/signalr";
import { queryKeys } from "@/lib/query-keys";

/**
 * Opens the notifications hub while `enabled` (i.e. the user is authenticated) and refetches the
 * notification queries whenever the server pushes `notificationsChanged`. Best-effort: the 60s polling
 * fallback still delivers if the socket is down.
 */
export function useNotificationRealtime(enabled: boolean) {
    const qc = useQueryClient();

    useEffect(() => {
        if (!enabled || typeof window === "undefined") return;

        const connection = createNotificationConnection();
        let stopped = false;

        connection.on("notificationsChanged", () => {
            void qc.invalidateQueries({ queryKey: queryKeys.notifications.all });
        });

        connection.start().catch(() => {
            // Hub unreachable — polling fallback covers delivery; stay silent.
        });

        return () => {
            stopped = true;
            connection.off("notificationsChanged");
            // stop() is async; ignore errors during teardown.
            void connection.stop().catch(() => {});
            void stopped;
        };
    }, [enabled, qc]);
}
