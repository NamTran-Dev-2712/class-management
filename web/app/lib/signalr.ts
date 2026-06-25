import {
    HubConnection,
    HubConnectionBuilder,
    HttpTransportType,
    LogLevel,
} from "@microsoft/signalr";

/**
 * Builds the notifications hub connection. The hub lives at the API origin (no `/api` prefix); auth
 * rides on the httpOnly JWT cookie via `withCredentials`. Browser-only (SignalR has no SSR use).
 */
export function createNotificationConnection(): HubConnection {
    const apiUrl = import.meta.env.VITE_API_URL ?? "/api";
    const origin = apiUrl.replace(/\/api\/?$/, "");
    const hubUrl = `${origin}/hubs/notifications`;

    return new HubConnectionBuilder()
        .withUrl(hubUrl, {
            withCredentials: true,
            // Cookies flow on the WS upgrade + negotiate; allow fallback transports too.
            transport:
                HttpTransportType.WebSockets |
                HttpTransportType.ServerSentEvents |
                HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build();
}
