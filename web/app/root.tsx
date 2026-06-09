import { QueryClientProvider } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import {
    isRouteErrorResponse,
    Links,
    Meta,
    Outlet,
    Scripts,
    ScrollRestoration,
} from "react-router";
import type { Route } from "./+types/root";
import "./app.css";
import { AuthProvider } from "@/components/providers/auth-provider";
import { Toaster } from "@/components/ui/sonner";
import { userContext } from "@/lib/auth-context";
import { authenticateRequest } from "@/lib/auth.server";
import { getQueryClient } from "@/lib/query-client";
import { useAuthStore } from "@/stores/auth.store";

export const links: Route.LinksFunction = () => [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
];

/**
 * Runs server-side on every document/data request. Resolves the user (refreshing
 * tokens when the access token expired) and shares it via context; any rotated
 * cookies from a refresh are appended to the outgoing response — including
 * redirects, so a refreshed user bounced off a guest page keeps the new session.
 */
export const middleware: Route.MiddlewareFunction[] = [
    async ({ request, context }, next) => {
        const { user, setCookies } = await authenticateRequest(request);
        context.set(userContext, user);

        const response = await next();
        for (const cookie of setCookies) response.headers.append("Set-Cookie", cookie);
        return response;
    },
];

export function loader({ context }: Route.LoaderArgs) {
    return { user: context.get(userContext) };
}

export function Layout({ children }: { children: React.ReactNode }) {
    const { i18n } = useTranslation();

    return (
        <html lang={i18n.language} dir={i18n.dir()}>
            <head>
                <meta charSet="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <Meta />
                <Links />
            </head>
            <body>
                {children}
                <Toaster richColors position="top-right" />
                <ScrollRestoration />
                <Scripts />
            </body>
        </html>
    );
}

export default function App({ loaderData }: Route.ComponentProps) {
    const [queryClient] = useState(getQueryClient);

    // Load persisted auth state from localStorage once on the client (the store
    // uses `skipHydration` to stay SSR-safe). The AuthProvider then overrides it
    // with the authoritative server-resolved user.
    useEffect(() => {
        void useAuthStore.persist.rehydrate();
    }, []);

    return (
        <QueryClientProvider client={queryClient}>
            <AuthProvider user={loaderData.user}>
                <Outlet />
            </AuthProvider>
        </QueryClientProvider>
    );
}

export function ErrorBoundary({ error }: Route.ErrorBoundaryProps) {
    let message = "Oops!";
    let details = "An unexpected error occurred.";

    if (isRouteErrorResponse(error)) {
        message = error.status === 404 ? "404" : "Error";
        details =
            error.status === 404
                ? "The requested page could not be found."
                : error.statusText || details;
    } else if (import.meta.env.DEV && error && error instanceof Error) {
        details = error.message;
    }

    return (
        <main className="mx-auto flex min-h-screen max-w-md flex-col items-center justify-center gap-2 p-6 text-center">
            <h1 className="text-3xl font-semibold">{message}</h1>
            <p className="text-muted-foreground">{details}</p>
        </main>
    );
}
