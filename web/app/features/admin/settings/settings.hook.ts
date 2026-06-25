import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { adminService } from "@/services/admin/admin.service";

export function useSystemSettings() {
    return useQuery({
        queryKey: queryKeys.admin.settings(),
        queryFn: () => adminService.settings(),
    });
}

export function useUpdateSetting() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: ({ key, value }: { key: string; value: string }) =>
            adminService.updateSetting(key, value),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.admin.settings() }),
    });
}
