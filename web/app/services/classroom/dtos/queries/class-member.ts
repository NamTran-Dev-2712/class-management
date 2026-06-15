import type { PageQuery } from "@/types/global/paginated";

export type MembershipStatus = "Pending" | "Approved" | "Rejected" | "Removed" | "Left";

/** Class roster/pending row (mirrors backend ClassMemberDto). */
export interface ClassMember {
    publicId: string;
    studentPublicId: string;
    studentName: string;
    studentEmail: string;
    status: MembershipStatus;
    joinedAt: string;
    processedAt: string | null;
    rejectionReason: string | null;
}

export interface ClassMemberQuery extends PageQuery {
    status?: MembershipStatus;
}
