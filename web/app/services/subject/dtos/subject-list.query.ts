import type { PageQuery } from "@/types/global/paginated";

export interface SubjectListQuery extends PageQuery {
    isActive?: boolean;
}
