import type { PageQuery } from "@/types/global/paginated";

/** Query params for the admin audit log list (mirrors backend GetAuditLogsQuery). */
export interface AuditLogListQuery extends PageQuery {
    action?: string;
    targetType?: string;
    actorPublicId?: string;
    fromUtc?: string;
    toUtc?: string;
}

/** One audit log row (mirrors backend AuditLogListDto). */
export interface AuditLogListItem {
    action: string;
    actorPublicId: string | null;
    actorName: string | null;
    actorEmail: string | null;
    actorRole: string | null;
    targetType: string | null;
    targetPublicId: string | null;
    metadata: string | null;
    ipAddress: string | null;
    createdAt: string;
}
