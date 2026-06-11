import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2, MailCheck } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { Button } from "@/components/ui/button";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { ApiError } from "@/lib/api-error";
import { useForgotPassword } from "./forgot-password.hook";
import { createForgotPasswordSchema, type ForgotPasswordValues } from "./forgot-password.schema";

export function ForgotPasswordForm() {
    const { t } = useTranslation("auth");
    const forgot = useForgotPassword();
    const [sentTo, setSentTo] = useState<string | null>(null);

    const schema = useMemo(() => createForgotPasswordSchema(t), [t]);
    const form = useForm<ForgotPasswordValues>({
        resolver: zodResolver(schema),
        defaultValues: { email: "" },
    });

    const onSubmit = (values: ForgotPasswordValues) => {
        forgot.mutate(values, {
            onSuccess: () => setSentTo(values.email),
            onError: (error) => {
                // Backend is generic; only surface unexpected/rate-limit errors.
                if (error instanceof ApiError) form.setError("email", { message: error.message });
            },
        });
    };

    if (sentTo) {
        return (
            <div className="grid gap-4 text-center">
                <div className="bg-primary/10 text-primary mx-auto flex size-12 items-center justify-center rounded-full">
                    <MailCheck className="size-6" />
                </div>
                <p className="text-muted-foreground text-sm">
                    {t("forgotPassword.sent", { email: sentTo })}
                </p>
                <Button asChild variant="outline" className="w-full">
                    <Link to="/login">{t("forgotPassword.backToLogin")}</Link>
                </Button>
            </div>
        );
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
                <FormField
                    control={form.control}
                    name="email"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("forgotPassword.email")}</FormLabel>
                            <FormControl>
                                <Input
                                    type="email"
                                    autoComplete="email"
                                    placeholder={t("forgotPassword.emailPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <Button type="submit" className="w-full" disabled={forgot.isPending}>
                    {forgot.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                    {t("forgotPassword.submit")}
                </Button>
            </form>
        </Form>
    );
}
