import { useQuery } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { adminClassService } from "@/services/classroom/classroom.service";
import type { ClassListQuery } from "@/services/classroom/dtos/queries/class-list";

export function useAdminClasses(query: ClassListQuery) {
    return useQuery({
        queryKey: queryKeys.classrooms.adminList(query),
        queryFn: () => adminClassService.list(query),
        placeholderData: (prev) => prev,
    });
}
