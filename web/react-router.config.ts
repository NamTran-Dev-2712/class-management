import type { Config } from "@react-router/dev/config";

export default {
    // Server-side render by default, then hydrate on the client.
    ssr: true,
    // Opt in to React Router v8 behavior early so the dev/build future-flag
    // warnings are silenced and the app is ready for the v8 upgrade.
    future: {
        v8_middleware: true,
        v8_splitRouteModules: true,
        v8_viteEnvironmentApi: true,
        v8_passThroughRequests: true,
        v8_trailingSlashAwareDataRequests: true,
    },
} satisfies Config;
