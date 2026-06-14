import type { PageQuery } from "@/types/global/paginated";

export interface UserListQuery extends PageQuery {
    isLocked?: boolean;
    role?: string;
}
