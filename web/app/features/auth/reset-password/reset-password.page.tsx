import { useTranslation } from "react-i18next";
import { Link, useSearchParams } from "react-router";

import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { ResetPasswordForm } from "./reset-password.form";
import type { Route } from "./+types/reset-password.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Reset password · Class Management" }];
}

export default function ResetPasswordPage() {
    const { t } = useTranslation("auth");
    const [searchParams] = useSearchParams();

    return (
        <Card className="w-full max-w-sm">
            <CardHeader className="text-center">
                <CardTitle className="text-2xl">{t("resetPassword.title")}</CardTitle>
                <CardDescription>{t("resetPassword.subtitle")}</CardDescription>
            </CardHeader>
            <CardContent>
                <ResetPasswordForm
                    defaultEmail={searchParams.get("email") ?? ""}
                    defaultOtp={searchParams.get("otp") ?? ""}
                />
            </CardContent>
            <CardFooter className="justify-center text-sm">
                <Link to="/login" className="text-muted-foreground hover:text-foreground">
                    {t("resetPassword.backToLogin")}
                </Link>
            </CardFooter>
        </Card>
    );
}
