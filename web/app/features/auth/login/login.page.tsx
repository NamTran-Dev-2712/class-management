import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
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
        <Card className="w-full max-w-sm">
            <CardHeader className="text-center">
                <CardTitle className="text-2xl">{t("login.title")}</CardTitle>
                <CardDescription>{t("login.subtitle")}</CardDescription>
            </CardHeader>
            <CardContent>
                <LoginForm />
            </CardContent>
            <CardFooter className="justify-center text-sm">
                <span className="text-muted-foreground">{t("login.noAccount")}&nbsp;</span>
                <Link to="/register" className="font-medium hover:underline">
                    {t("login.signUp")}
                </Link>
            </CardFooter>
        </Card>
    );
}
