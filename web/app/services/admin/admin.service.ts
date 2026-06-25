import { http } from "@/lib/axios.config";

export interface DailyCount {
    date: string;
    count: number;
}

export interface StatusCount {
    status: string;
    count: number;
}

export interface AdminDashboard {
    totalUsers: number;
    newUsersLast7Days: number;
    activeClasses: number;
    openAssignments: number;
    pendingReports: number;
    newUsersDaily: DailyCount[];
    reportsByStatus: StatusCount[];
    assignmentsByStatus: StatusCount[];
}

export interface SystemSetting {
    key: string;
    value: string;
    valueType: "string" | "integer" | "boolean" | "json";
    isPublic: boolean;
    description: string | null;
    updatedAt: string;
}

export const adminService = {
    dashboard: () => http.get<AdminDashboard>("/admin/dashboard"),
    settings: () => http.get<SystemSetting[]>("/admin/settings"),
    updateSetting: (key: string, value: string) =>
        http.put<null>(`/admin/settings/${key}`, { value }),
    forceCloseAssignment: (publicId: string) =>
        http.post<null>(`/admin/assignments/${publicId}/force-close`),
};
