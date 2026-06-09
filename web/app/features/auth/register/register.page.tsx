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
import { RegisterForm } from "./register.form";
import type { Route } from "./+types/register.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Sign up · Class Management" }];
}

export default function RegisterPage() {
    const { t } = useTranslation("auth");

    return (
        <Card className="w-full max-w-md">
            <CardHeader className="text-center">
                <CardTitle className="text-2xl">{t("register.title")}</CardTitle>
                <CardDescription>{t("register.subtitle")}</CardDescription>
            </CardHeader>
            <CardContent>
                <RegisterForm />
            </CardContent>
            <CardFooter className="justify-center text-sm">
                <span className="text-muted-foreground">{t("register.haveAccount")}&nbsp;</span>
                <Link to="/login" className="font-medium hover:underline">
                    {t("register.signIn")}
                </Link>
            </CardFooter>
        </Card>
    );
}
