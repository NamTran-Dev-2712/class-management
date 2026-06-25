import { useQuery } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { auditLogService } from "@/services/audit-log/audit-log.service";
import type { AuditLogListQuery } from "@/services/audit-log/dtos/queries/audit-log-list";

export function useAuditLogs(query: AuditLogListQuery) {
    return useQuery({
        queryKey: queryKeys.auditLogs.list(query),
        queryFn: () => auditLogService.list(query),
        placeholderData: (prev) => prev,
    });
}
