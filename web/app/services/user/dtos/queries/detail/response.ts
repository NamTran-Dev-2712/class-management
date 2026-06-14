/** Single-user detail for the Admin edit page (mirrors backend UserDetailDto). */
export interface UserDetail {
    publicId: string;
    displayName: string;
    email: string;
    emailConfirmed: boolean;
    phoneNumber: string | null;
    isActive: boolean;
    isLocked: boolean;
    lockedAt: string | null;
    lastLoginAt: string | null;
    createdAt: string;
    updatedAt: string;
    roles: string[];
}
