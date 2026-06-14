/** Mirrors the backend `UserProfileDto`. The public GUID is the only exposed id. */
export interface ProfileResponse {
    publicId: string;
    displayName: string;
    phoneNumber?: string | null;
    email: string;
    emailConfirmed: boolean;
    avatarUrl?: string | null;
    bio?: string | null;
    lastLoginAt?: string | null;
    createdAt: string;
    roles: string[];
}
