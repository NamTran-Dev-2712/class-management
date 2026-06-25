import { http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type { AuditLogListItem, AuditLogListQuery } from "./dtos/queries/audit-log-list";

const ADMIN = "/admin/audit-logs";

/** Admin read-only audit trail. */
export const auditLogService = {
    list: (query: AuditLogListQuery) =>
        http.get<Paginated<AuditLogListItem>>(ADMIN, { params: query }),
};
