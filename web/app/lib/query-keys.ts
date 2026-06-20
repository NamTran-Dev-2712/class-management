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
    questions: {
        all: ["questions"] as const,
        teacherList: (params: unknown) =>
            [...queryKeys.questions.all, "teacher", "list", params] as const,
        publicList: (params: unknown) =>
            [...queryKeys.questions.all, "public", "list", params] as const,
        adminList: (params: unknown) =>
            [...queryKeys.questions.all, "admin", "list", params] as const,
        detail: (publicId: string) => [...queryKeys.questions.all, "detail", publicId] as const,
        publicDetail: (publicId: string) =>
            [...queryKeys.questions.all, "public", "detail", publicId] as const,
        adminDetail: (publicId: string) =>
            [...queryKeys.questions.all, "admin", "detail", publicId] as const,
    },
    exams: {
        all: ["exams"] as const,
        teacherList: (params: unknown) =>
            [...queryKeys.exams.all, "teacher", "list", params] as const,
        publicList: (params: unknown) =>
            [...queryKeys.exams.all, "public", "list", params] as const,
        adminList: (params: unknown) => [...queryKeys.exams.all, "admin", "list", params] as const,
        detail: (publicId: string) => [...queryKeys.exams.all, "detail", publicId] as const,
        publicDetail: (publicId: string) =>
            [...queryKeys.exams.all, "public", "detail", publicId] as const,
        adminDetail: (publicId: string) =>
            [...queryKeys.exams.all, "admin", "detail", publicId] as const,
        preview: (publicId: string) => [...queryKeys.exams.all, "preview", publicId] as const,
    },
    assignments: {
        all: ["assignments"] as const,
        teacherList: (params: unknown) =>
            [...queryKeys.assignments.all, "teacher", "list", params] as const,
        adminList: (params: unknown) =>
            [...queryKeys.assignments.all, "admin", "list", params] as const,
        studentList: (params: unknown) =>
            [...queryKeys.assignments.all, "student", "list", params] as const,
        detail: (publicId: string) => [...queryKeys.assignments.all, "detail", publicId] as const,
        studentDetail: (publicId: string) =>
            [...queryKeys.assignments.all, "student", "detail", publicId] as const,
        preview: (publicId: string) => [...queryKeys.assignments.all, "preview", publicId] as const,
        attempts: (publicId: string, params: unknown) =>
            [...queryKeys.assignments.all, "attempts", publicId, params] as const,
        myAttempts: (params: unknown) =>
            [...queryKeys.assignments.all, "my-attempts", params] as const,
        taking: (attemptId: string) => [...queryKeys.assignments.all, "taking", attemptId] as const,
        result: (attemptId: string) => [...queryKeys.assignments.all, "result", attemptId] as const,
        grading: (attemptId: string) =>
            [...queryKeys.assignments.all, "grading", attemptId] as const,
        report: (publicId: string) => [...queryKeys.assignments.all, "report", publicId] as const,
    },
} as const;
