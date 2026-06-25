import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { adminReportService } from "@/services/report/report.service";
import type { ReportListQuery, ReviewReportRequest } from "@/services/report/dtos/report-dtos";

export function useAdminReports(query: ReportListQuery) {
    return useQuery({
        queryKey: queryKeys.reports.adminList(query),
        queryFn: () => adminReportService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useReviewReport() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: ({ publicId, payload }: { publicId: string; payload: ReviewReportRequest }) =>
            adminReportService.review(publicId, payload),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.reports.all }),
    });
}
