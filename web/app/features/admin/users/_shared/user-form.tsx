import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { toast } from "sonner";

import { Roles } from "@/config/roles";
import { Button } from "@/components/ui/button";
import {
    Form,
    FormControl,
    FormDescription,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { ApiError } from "@/lib/api-error";
import type { UserDetail } from "@/services/user/dtos/queries/detail/response";
import { useCreateUser, useUpdateUser } from "./users.hook";
import { createUserSchema, type UserFormValues } from "./user.schema";

interface UserFormProps {
    /** Existing user to edit, or undefined to create a new one. */
    user?: UserDetail;
}

const USERS_PATH = "/admin/users";

export function UserForm({ user }: UserFormProps) {
    const { t } = useTranslation("user");
    const navigate = useNavigate();
    const create = useCreateUser();
    const update = useUpdateUser();
    const isEdit = user !== undefined;

    const schema = useMemo(() => createUserSchema(t), [t]);
    const form = useForm<UserFormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            displayName: user?.displayName ?? "",
            email: user?.email ?? "",
            role: (user?.roles[0] as UserFormValues["role"]) ?? Roles.Student,
            phoneNumber: user?.phoneNumber ?? "",
        },
    });

    const isPending = create.isPending || update.isPending;

    const onSubmit = (values: UserFormValues) => {
        const phoneNumber = values.phoneNumber?.trim() ? values.phoneNumber.trim() : null;

        const onError = (error: unknown) => {
            toast.error(error instanceof ApiError ? error.message : t("toast.error"));
        };
        const goBack = () => void navigate(USERS_PATH);

        if (isEdit) {
            update.mutate(
                {
                    publicId: user.publicId,
                    payload: {
                        displayName: values.displayName.trim(),
                        role: values.role,
                        phoneNumber,
                    },
                },
                {
                    onSuccess: () => {
                        toast.success(t("toast.updated"));
                        goBack();
                    },
                    onError,
                },
            );
        } else {
            create.mutate(
                {
                    displayName: values.displayName.trim(),
                    email: values.email.trim(),
                    role: values.role,
                    phoneNumber,
                },
                {
                    onSuccess: () => {
                        toast.success(t("toast.created"));
                        goBack();
                    },
                    onError,
                },
            );
        }
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-5">
                <FormField
                    control={form.control}
                    name="displayName"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("form.displayName")}</FormLabel>
                            <FormControl>
                                <Input
                                    placeholder={t("form.displayNamePlaceholder")}
                                    autoComplete="name"
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
                            <FormLabel>{t("form.email")}</FormLabel>
                            <FormControl>
                                <Input
                                    type="email"
                                    autoComplete="email"
                                    placeholder={t("form.emailPlaceholder")}
                                    disabled={isEdit}
                                    {...field}
                                />
                            </FormControl>
                            <FormDescription>
                                {isEdit ? t("form.emailReadonlyHint") : t("form.emailHint")}
                            </FormDescription>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="grid gap-5 sm:grid-cols-2">
                    <FormField
                        control={form.control}
                        name="role"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{t("form.role")}</FormLabel>
                                <Select value={field.value} onValueChange={field.onChange}>
                                    <FormControl>
                                        <SelectTrigger className="w-full">
                                            <SelectValue placeholder={t("form.rolePlaceholder")} />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        <SelectItem value={Roles.Admin}>
                                            {t("roles.Admin")}
                                        </SelectItem>
                                        <SelectItem value={Roles.Teacher}>
                                            {t("roles.Teacher")}
                                        </SelectItem>
                                        <SelectItem value={Roles.Student}>
                                            {t("roles.Student")}
                                        </SelectItem>
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="phoneNumber"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{t("form.phoneNumber")}</FormLabel>
                                <FormControl>
                                    <Input
                                        type="tel"
                                        autoComplete="tel"
                                        placeholder={t("form.phoneNumberPlaceholder")}
                                        {...field}
                                    />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <div className="flex justify-end gap-3 border-t pt-5">
                    <Button type="button" variant="outline" onClick={() => navigate(USERS_PATH)}>
                        {t("form.cancel")}
                    </Button>
                    <Button type="submit" disabled={isPending}>
                        {isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                        {isEdit ? t("form.save") : t("form.create")}
                    </Button>
                </div>
            </form>
        </Form>
    );
}
