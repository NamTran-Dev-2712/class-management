export interface RegisterRequest {
    displayName: string;
    email: string;
    password: string;
    /** "Student" | "Teacher" — Admin is provisioned internally, not self-registered. */
    role: string;
}
