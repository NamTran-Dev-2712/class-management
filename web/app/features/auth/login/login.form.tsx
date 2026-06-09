import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { useNavigate, useSearchParams } from "react-router";
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
import { roleHome, safeRedirect } from "@/lib/auth";
import { useLogin } from "./login.hook";
import { createLoginSchema, type LoginFormValues } from "./login.schema";

export function LoginForm() {
    const { t } = useTranslation("auth");
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const login = useLogin();

    const schema = useMemo(() => createLoginSchema(t), [t]);
    const form = useForm<LoginFormValues>({
        resolver: zodResolver(schema),
        defaultValues: { email: "", password: "" },
    });

    const onSubmit = (values: LoginFormValues) => {
        login.mutate(values, {
            onSuccess: (user) => {
                toast.success(t("login.success"));
                const target = safeRedirect(searchParams.get("redirectTo"), roleHome(user.roles));
                void navigate(target, { replace: true });
            },
            onError: (error) => {
                toast.error(error instanceof ApiError ? error.message : t("login.title"));
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
                            <FormLabel>{t("login.email")}</FormLabel>
                            <FormControl>
                                <Input
                                    type="email"
                                    autoComplete="email"
                                    placeholder={t("login.emailPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="password"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("login.password")}</FormLabel>
                            <FormControl>
                                <PasswordInput
                                    autoComplete="current-password"
                                    placeholder={t("login.passwordPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <Button type="submit" className="w-full" disabled={login.isPending}>
                    {login.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                    {t("login.submit")}
                </Button>
            </form>
        </Form>
    );
}
