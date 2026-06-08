import { useTranslation } from "react-i18next";

import { LanguageSwitcher } from "@/components/shared/language-switcher";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { LoginForm } from "./login.form";
import type { Route } from "./+types/login.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Sign in · Class Management" }];
}

export default function LoginPage() {
    const { t } = useTranslation("auth");

    return (
        <main className="bg-muted/40 flex min-h-screen items-center justify-center p-6">
            <div className="absolute top-4 right-4">
                <LanguageSwitcher />
            </div>
            <Card className="w-full max-w-sm">
                <CardHeader className="text-center">
                    <CardTitle className="text-2xl">
                        {t("login.title")}
                    </CardTitle>
                    <CardDescription>{t("login.subtitle")}</CardDescription>
                </CardHeader>
                <CardContent>
                    <LoginForm />
                </CardContent>
            </Card>
        </main>
    );
}
