/**
 * Centralized, typed query-key factory. Group keys by domain so invalidation
 * stays predictable (e.g. `queryClient.invalidateQueries({ queryKey: queryKeys.auth.all })`).
 */
export const queryKeys = {
    auth: {
        all: ["auth"] as const,
        profile: () => [...queryKeys.auth.all, "profile"] as const,
    },
    subjects: {
        all: ["subjects"] as const,
        list: (params: unknown) => [...queryKeys.subjects.all, "list", params] as const,
        detail: (publicId: string) => [...queryKeys.subjects.all, "detail", publicId] as const,
    },
    users: {
        all: ["users"] as const,
        list: (params: unknown) => [...queryKeys.users.all, "list", params] as const,
        detail: (publicId: string) => [...queryKeys.users.all, "detail", publicId] as const,
    },
    classrooms: {
        all: ["classrooms"] as const,
        teacherList: (params: unknown) =>
            [...queryKeys.classrooms.all, "teacher", "list", params] as const,
        adminList: (params: unknown) =>
            [...queryKeys.classrooms.all, "admin", "list", params] as const,
        studentList: (params: unknown) =>
            [...queryKeys.classrooms.all, "student", "list", params] as const,
        requests: (params: unknown) =>
            [...queryKeys.classrooms.all, "student", "requests", params] as const,
        detail: (publicId: string) => [...queryKeys.classrooms.all, "detail", publicId] as const,
        members: (publicId: string, params: unknown) =>
            [...queryKeys.classrooms.all, "members", publicId, params] as const,
    },
} as const;
