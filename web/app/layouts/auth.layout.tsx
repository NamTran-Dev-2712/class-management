import { useTranslation } from "react-i18next";
import { Link, Outlet } from "react-router";

import { LanguageSwitcher } from "@/components/shared/language-switcher";
import { brandIcon as BrandIcon } from "@/config/nav";
import { requireGuest } from "@/guards/require-guest";
import { userContext } from "@/lib/auth-context";
import type { Route } from "./+types/auth.layout";

export function loader({ context }: Route.LoaderArgs) {
    return requireGuest(context.get(userContext));
}

export default function AuthLayout() {
    const { t } = useTranslation("common");

    return (
        <div className="bg-muted/30 flex min-h-screen flex-col">
            <header className="flex h-16 items-center justify-between px-4 md:px-6">
                <Link to="/" className="flex items-center gap-2 font-semibold">
                    <BrandIcon className="text-primary size-6" />
                    <span>{t("appName")}</span>
                </Link>
                <LanguageSwitcher />
            </header>
            <main className="flex flex-1 items-center justify-center p-4 pb-16">
                <Outlet />
            </main>
        </div>
    );
}
