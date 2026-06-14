/** Admin user-list row (mirrors backend UserDto from vw_admin_users). */
export interface User {
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
    roles: string[];
}
