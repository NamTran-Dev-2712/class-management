import { http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    ReportListItem,
    ReportListQuery,
    ReviewReportRequest,
    SubmitReportRequest,
} from "./dtos/report-dtos";

/** User-facing reporting. */
export const reportService = {
    submit: (payload: SubmitReportRequest) => http.post<{ publicId: string }>("/reports", payload),
    mine: (query: ReportListQuery) =>
        http.get<Paginated<ReportListItem>>("/reports/mine", { params: query }),
};

/** Admin moderation. */
export const adminReportService = {
    list: (query: ReportListQuery) =>
        http.get<Paginated<ReportListItem>>("/admin/reports", { params: query }),
    review: (publicId: string, payload: ReviewReportRequest) =>
        http.post<null>(`/admin/reports/${publicId}/review`, payload),
};
