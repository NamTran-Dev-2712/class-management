import type { PageQuery } from "@/types/global/paginated";
import type { ClassStatus } from "./class-list";
import type { MembershipStatus } from "./class-member";

/** A class the student is an approved member of (mirrors backend StudentClassDto). */
export interface StudentClass {
    classPublicId: string;
    className: string;
    subjectName: string | null;
    ownerName: string;
    classStatus: ClassStatus;
    joinedAt: string;
}

/** A student's join request and its status (mirrors backend MembershipRequestDto). */
export interface MembershipRequest {
    publicId: string;
    classPublicId: string;
    className: string;
    subjectName: string | null;
    ownerName: string;
    status: MembershipStatus;
    joinedAt: string;
    processedAt: string | null;
    rejectionReason: string | null;
}

export interface MembershipRequestQuery extends PageQuery {
    status?: MembershipStatus;
}
