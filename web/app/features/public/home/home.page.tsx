import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { LanguageSwitcher } from "@/components/shared/language-switcher";
import { Button } from "@/components/ui/button";
import type { Route } from "./+types/home.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Class Management" }];
}

export default function HomePage() {
    const { t } = useTranslation(["public", "common"]);

    return (
        <main className="flex min-h-screen flex-col">
            <header className="flex items-center justify-between border-b px-6 py-4">
                <span className="text-lg font-semibold">
                    {t("appName", { ns: "common" })}
                </span>
                <LanguageSwitcher />
            </header>

            <section className="mx-auto flex max-w-2xl flex-1 flex-col items-center justify-center gap-6 px-6 text-center">
                <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">
                    {t("home.title")}
                </h1>
                <p className="text-muted-foreground text-lg">
                    {t("home.tagline")}
                </p>
                <Button asChild size="lg">
                    <Link to="/login">{t("home.cta")}</Link>
                </Button>
            </section>
        </main>
    );
}
