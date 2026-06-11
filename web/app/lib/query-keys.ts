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
} as const;
