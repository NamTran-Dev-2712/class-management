/** Payload to update a user. Email is immutable, so it is intentionally omitted. */
export interface UpdateUserRequest {
    displayName: string;
    role: string;
    phoneNumber: string | null;
}
