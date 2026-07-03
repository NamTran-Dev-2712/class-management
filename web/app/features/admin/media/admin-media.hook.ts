import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { adminMediaService } from "@/services/media/media.service";
import type { AdminMediaListQuery } from "@/services/media/dtos/media-dtos";

export function useAdminStorageOverview(pageNumber: number, pageSize = 20) {
    return useQuery({
        queryKey: queryKeys.media.adminOverview({ pageNumber, pageSize }),
        queryFn: () => adminMediaService.overview({ pageNumber, pageSize }),
    });
}

export function useAdminMediaList(query: AdminMediaListQuery) {
    return useQuery({
        queryKey: queryKeys.media.adminList(query),
        queryFn: () => adminMediaService.list(query),
    });
}

export function useAdminDeleteMedia() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (publicId: string) => adminMediaService.remove(publicId),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.media.all }),
    });
}
