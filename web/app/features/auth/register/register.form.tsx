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
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Roles } from "@/config/roles";
import { ApiError } from "@/lib/api-error";
import { roleHome } from "@/lib/auth";
import { cn } from "@/lib/utils";
import { useRegister } from "./register.hook";
import { createRegisterSchema, type RegisterFormValues } from "./register.schema";

export function RegisterForm() {
    const { t } = useTranslation("auth");
    const navigate = useNavigate();
    const register = useRegister();

    const schema = useMemo(() => createRegisterSchema(t), [t]);
    const form = useForm<RegisterFormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            displayName: "",
            email: "",
            password: "",
            confirmPassword: "",
            role: Roles.Student,
        },
    });

    const onSubmit = (values: RegisterFormValues) => {
        register.mutate(values, {
            onSuccess: (user) => {
                toast.success(t("register.success"));
                void navigate(roleHome(user.roles), { replace: true });
            },
            onError: (error) => {
                toast.error(error instanceof ApiError ? error.message : t("register.title"));
            },
        });
    };

    const roleOptions = [
        { value: Roles.Student, label: t("register.roleStudent") },
        { value: Roles.Teacher, label: t("register.roleTeacher") },
    ];

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
                <FormField
                    control={form.control}
                    name="displayName"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("register.displayName")}</FormLabel>
                            <FormControl>
                                <Input
                                    autoComplete="name"
                                    placeholder={t("register.displayNamePlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="email"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("register.email")}</FormLabel>
                            <FormControl>
                                <Input
                                    type="email"
                                    autoComplete="email"
                                    placeholder={t("register.emailPlaceholder")}
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
                            <FormLabel>{t("register.password")}</FormLabel>
                            <FormControl>
                                <PasswordInput
                                    autoComplete="new-password"
                                    placeholder={t("register.passwordPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="confirmPassword"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("register.confirmPassword")}</FormLabel>
                            <FormControl>
                                <PasswordInput
                                    autoComplete="new-password"
                                    placeholder={t("register.passwordPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="role"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("register.role")}</FormLabel>
                            <FormControl>
                                <RadioGroup
                                    value={field.value}
                                    onValueChange={field.onChange}
                                    className="grid grid-cols-2 gap-3"
                                >
                                    {roleOptions.map((option) => (
                                        <FormLabel
                                            key={option.value}
                                            className={cn(
                                                "flex cursor-pointer items-center gap-2 rounded-md border p-3 font-normal",
                                                field.value === option.value &&
                                                    "border-primary ring-primary/30 ring-2",
                                            )}
                                        >
                                            <RadioGroupItem value={option.value} />
                                            {option.label}
                                        </FormLabel>
                                    ))}
                                </RadioGroup>
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <Button type="submit" className="w-full" disabled={register.isPending}>
                    {register.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                    {t("register.submit")}
                </Button>
            </form>
        </Form>
    );
}
