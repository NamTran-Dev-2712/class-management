import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { toast } from "sonner";

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
import { PasswordInput } from "@/components/ui/password-input";
import { ApiError } from "@/lib/api-error";
import { useResetPassword } from "./reset-password.hook";
import { createResetPasswordSchema, type ResetPasswordValues } from "./reset-password.schema";

interface ResetPasswordFormProps {
    defaultEmail: string;
    defaultOtp: string;
}

export function ResetPasswordForm({ defaultEmail, defaultOtp }: ResetPasswordFormProps) {
    const { t } = useTranslation("auth");
    const navigate = useNavigate();
    const reset = useResetPassword();

    const schema = useMemo(() => createResetPasswordSchema(t), [t]);
    const form = useForm<ResetPasswordValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            email: defaultEmail,
            otp: defaultOtp,
            newPassword: "",
            confirmNewPassword: "",
        },
    });

    const onSubmit = (values: ResetPasswordValues) => {
        reset.mutate(values, {
            onSuccess: () => {
                toast.success(t("resetPassword.success"));
                void navigate("/login", { replace: true });
            },
            onError: (error) => {
                toast.error(error instanceof ApiError ? error.message : t("resetPassword.title"));
            },
        });
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
                <FormField
                    control={form.control}
                    name="email"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("resetPassword.email")}</FormLabel>
                            <FormControl>
                                <Input type="email" autoComplete="email" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <FormField
                    control={form.control}
                    name="otp"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("resetPassword.otp")}</FormLabel>
                            <FormControl>
                                <Input
                                    inputMode="numeric"
                                    maxLength={6}
                                    placeholder={t("resetPassword.otpPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <FormField
                    control={form.control}
                    name="newPassword"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("resetPassword.newPassword")}</FormLabel>
                            <FormControl>
                                <PasswordInput autoComplete="new-password" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <FormField
                    control={form.control}
                    name="confirmNewPassword"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("resetPassword.confirmPassword")}</FormLabel>
                            <FormControl>
                                <PasswordInput autoComplete="new-password" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <Button type="submit" className="w-full" disabled={reset.isPending}>
                    {reset.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                    {t("resetPassword.submit")}
                </Button>
            </form>
        </Form>
    );
}
