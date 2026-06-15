import type { ClassStatus } from "./class-list";

/** Single class detail (mirrors backend ClassDetailDto). */
export interface ClassDetail {
    publicId: string;
    name: string;
    description: string | null;
    subjectId: string | null;
    subjectName: string | null;
    inviteCode: string;
    status: ClassStatus;
    coverImageUrl: string | null;
    ownerName: string;
    ownerEmail: string;
    approvedMemberCount: number;
    pendingCount: number;
    createdAt: string;
    updatedAt: string;
}
