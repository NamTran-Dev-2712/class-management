import { useNotificationRealtime } from "@/features/notifications/use-notification-realtime";
import { useAuthStore } from "@/stores/auth.store";

/** Keeps a notifications SignalR connection open while the user is authenticated (MVP-7.5). */
export function NotificationRealtimeProvider({ children }: { children: React.ReactNode }) {
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    useNotificationRealtime(isAuthenticated);
    return <>{children}</>;
}
