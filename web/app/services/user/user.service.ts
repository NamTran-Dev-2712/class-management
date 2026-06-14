import { http } from "@/lib/axios.config";
import type { CreateUserRequest } from "@/services/user/dtos/commands/create-user/request";
import type { UpdateUserRequest } from "@/services/user/dtos/commands/update-user/request";
import type { UserListQuery } from "@/services/user/dtos/queries/list/query";
import type { User } from "@/services/user/dtos/queries/list/response";
import type { UserDetail } from "@/services/user/dtos/queries/detail/response";
import type { Paginated } from "@/types/global/paginated";

export const userService = {
    list: (query: UserListQuery) => http.get<Paginated<User>>("/admin/users", { params: query }),
    getById: (publicId: string) => http.get<UserDetail>(`/admin/users/${publicId}`),
    create: (payload: CreateUserRequest) =>
        http.post<{ publicId: string }>("/admin/users", payload),
    update: (publicId: string, payload: UpdateUserRequest) =>
        http.put<null>(`/admin/users/${publicId}`, payload),
    lock: (publicId: string) => http.post<null>(`/admin/users/${publicId}/lock`),
    unlock: (publicId: string) => http.post<null>(`/admin/users/${publicId}/unlock`),
    remove: (publicId: string) => http.delete<null>(`/admin/users/${publicId}`),
};
