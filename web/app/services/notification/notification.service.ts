import { http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    NotificationItem,
    NotificationListQuery,
    UnreadCountResponse,
} from "./dtos/notification-dtos";

const BASE = "/notifications";

/** Current user's in-app notifications. */
export const notificationService = {
    list: (query: NotificationListQuery) =>
        http.get<Paginated<NotificationItem>>(BASE, { params: query }),
    unreadCount: () => http.get<UnreadCountResponse>(`${BASE}/unread-count`),
    markRead: (publicId: string) => http.post<null>(`${BASE}/${publicId}/read`),
    markAllRead: () => http.post<null>(`${BASE}/read-all`),
    archive: (publicId: string) => http.post<null>(`${BASE}/${publicId}/archive`),
};
