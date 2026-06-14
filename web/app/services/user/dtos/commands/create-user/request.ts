/** Payload to create a user. The password is generated server-side and emailed. */
export interface CreateUserRequest {
    displayName: string;
    email: string;
    role: string;
    phoneNumber: string | null;
}
