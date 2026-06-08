import { QueryClient } from "@tanstack/react-query";

function makeQueryClient(): QueryClient {
    return new QueryClient({
        defaultOptions: {
            queries: {
                staleTime: 60 * 1000,
                retry: 1,
                refetchOnWindowFocus: false,
            },
        },
    });
}

let browserQueryClient: QueryClient | undefined;

/**
 * SSR-safe accessor: always a fresh client on the server (per request), a
 * stable singleton in the browser (survives re-renders / hydration).
 */
export function getQueryClient(): QueryClient {
    if (typeof window === "undefined") return makeQueryClient();
    if (!browserQueryClient) browserQueryClient = makeQueryClient();
    return browserQueryClient;
}
