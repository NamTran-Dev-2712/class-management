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
import { ForgotPasswordForm } from "./forgot-password.form";
import type { Route } from "./+types/forgot-password.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Forgot password · Class Management" }];
}

export default function ForgotPasswordPage() {
    const { t } = useTranslation("auth");

    return (
        <Card className="w-full max-w-sm">
            <CardHeader className="text-center">
                <CardTitle className="text-2xl">{t("forgotPassword.title")}</CardTitle>
                <CardDescription>{t("forgotPassword.subtitle")}</CardDescription>
            </CardHeader>
            <CardContent>
                <ForgotPasswordForm />
            </CardContent>
            <CardFooter className="justify-center text-sm">
                <Link to="/login" className="text-muted-foreground hover:text-foreground">
                    {t("forgotPassword.backToLogin")}
                </Link>
            </CardFooter>
        </Card>
    );
}
