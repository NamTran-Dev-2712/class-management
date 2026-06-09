/** Application roles — mirror the backend `ApplicationRoles` constants. */
export const Roles = {
    Admin: "Admin",
    Teacher: "Teacher",
    Student: "Student",
} as const;

export type Role = (typeof Roles)[keyof typeof Roles];

/** Roles a user may pick when self-registering (Admin is provisioned internally). */
export const SELF_REGISTERABLE_ROLES: Role[] = [Roles.Student, Roles.Teacher];
