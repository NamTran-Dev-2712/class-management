import type { PageQuery } from "@/types/global/paginated";

export type NotificationStatus = "Unread" | "Read" | "Archived";

export interface NotificationListQuery extends PageQuery {
    status?: NotificationStatus;
}

/** One notification (mirrors backend NotificationDto). */
export interface NotificationItem {
    publicId: string;
    eventType: string;
    title: string;
    body: string | null;
    link: string | null;
    payload: Record<string, unknown> | null;
    status: NotificationStatus;
    createdAt: string;
    readAt: string | null;
}

export interface UnreadCountResponse {
    count: number;
}
