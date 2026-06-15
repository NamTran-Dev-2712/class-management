import type { PageQuery } from "@/types/global/paginated";

export type ClassStatus = "Active" | "Archived";

/** Class list row (mirrors backend ClassListDto). */
export interface ClassListItem {
    publicId: string;
    name: string;
    description: string | null;
    subjectName: string | null;
    status: ClassStatus;
    coverImageUrl: string | null;
    ownerName: string;
    approvedMemberCount: number;
    pendingCount: number;
    createdAt: string;
    updatedAt: string;
}

export interface ClassListQuery extends PageQuery {
    status?: ClassStatus;
}
