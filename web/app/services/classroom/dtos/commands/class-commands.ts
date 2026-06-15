export interface CreateClassRequest {
    name: string;
    description: string | null;
    subjectId: string | null;
    subjectName: string | null;
    coverImageUrl: string | null;
}

export type UpdateClassRequest = CreateClassRequest;

export interface JoinClassRequest {
    inviteCode: string;
}

export interface RejectMemberRequest {
    rejectionReason: string | null;
}
