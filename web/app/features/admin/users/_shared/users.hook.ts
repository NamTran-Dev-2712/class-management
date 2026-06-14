import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { userService } from "@/services/user/user.service";
import type { UserListQuery } from "@/services/user/dtos/queries/list/query";
import type { CreateUserRequest } from "@/services/user/dtos/commands/create-user/request";
import type { UpdateUserRequest } from "@/services/user/dtos/commands/update-user/request";

export function useUsers(query: UserListQuery) {
    return useQuery({
        queryKey: queryKeys.users.list(query),
        queryFn: () => userService.list(query),
        // Keep the previous page visible while the next loads (no flash of empty table).
        placeholderData: (prev) => prev,
    });
}

export function useUser(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.users.detail(publicId ?? ""),
        queryFn: () => userService.getById(publicId!),
        enabled: !!publicId,
    });
}

export function useCreateUser() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (payload: CreateUserRequest) => userService.create(payload),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.users.all }),
    });
}

export function useUpdateUser() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: UpdateUserRequest }) =>
            userService.update(input.publicId, input.payload),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.users.all }),
    });
}

export function useLockUser() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (publicId: string) => userService.lock(publicId),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.users.all }),
    });
}

export function useUnlockUser() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (publicId: string) => userService.unlock(publicId),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.users.all }),
    });
}

export function useDeleteUser() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (publicId: string) => userService.remove(publicId),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.users.all }),
    });
}
