import type { PageQuery } from "@/types/global/paginated";

export type ReportTargetType = "Question" | "Exam" | "Assignment" | "Class" | "User";
export type ReportReason =
    | "InappropriateContent"
    | "Spam"
    | "Copyright"
    | "IncorrectAnswer"
    | "Other";
export type ReportStatus = "Pending" | "Reviewing" | "Resolved" | "Rejected";
export type ReportAdminAction =
    | "Dismiss"
    | "WarnUser"
    | "HideContent"
    | "DeleteContent"
    | "BanUser";

export interface ReportListQuery extends PageQuery {
    targetType?: ReportTargetType;
    status?: ReportStatus;
}

export interface ReportListItem {
    publicId: string;
    reporterPublicId: string | null;
    reporterName: string | null;
    reporterEmail: string | null;
    targetType: ReportTargetType;
    targetPublicId: string | null;
    reason: ReportReason;
    description: string | null;
    status: ReportStatus;
    adminName: string | null;
    adminAction: ReportAdminAction | null;
    adminNote: string | null;
    resolvedAt: string | null;
    createdAt: string;
}

export interface SubmitReportRequest {
    targetType: ReportTargetType;
    targetPublicId: string;
    reason: ReportReason;
    description?: string | null;
}

export interface ReviewReportRequest {
    status: ReportStatus;
    adminAction?: ReportAdminAction | null;
    adminNote?: string | null;
}
