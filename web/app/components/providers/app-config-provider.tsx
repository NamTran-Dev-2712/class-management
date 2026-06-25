import { useQuery } from "@tanstack/react-query";
import { useEffect } from "react";

import { MaintenanceBanner } from "@/components/shared/maintenance-banner";
import { publicService } from "@/services/public/public.service";
import { useAppConfigStore } from "@/stores/app-config.store";

/**
 * Loads the public app config (brand name + maintenance flag) on the client and mirrors it into the
 * app-config store, then renders the global maintenance banner + children (MVP-7.5).
 */
export function AppConfigProvider({ children }: { children: React.ReactNode }) {
    const setConfig = useAppConfigStore((s) => s.setConfig);

    const { data } = useQuery({
        queryKey: ["public", "app-config"],
        queryFn: () => publicService.appConfig(),
        staleTime: 5 * 60 * 1000,
        refetchOnWindowFocus: false,
    });

    useEffect(() => {
        if (data) setConfig(data);
    }, [data, setConfig]);

    return (
        <>
            <MaintenanceBanner />
            {children}
        </>
    );
}
