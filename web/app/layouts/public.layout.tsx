import { Outlet } from "react-router";

import { PublicFooter } from "@/components/layout/public/public-footer";
import { PublicHeader } from "@/components/layout/public/public-header";
import { userContext } from "@/lib/auth-context";
import type { Route } from "./+types/public.layout";

export function loader({ context }: Route.LoaderArgs) {
    return { user: context.get(userContext) };
}

export default function PublicLayout({ loaderData }: Route.ComponentProps) {
    return (
        <div className="flex min-h-screen flex-col">
            <PublicHeader user={loaderData.user} />
            <main className="flex flex-1 flex-col">
                <Outlet />
            </main>
            <PublicFooter />
        </div>
    );
}
