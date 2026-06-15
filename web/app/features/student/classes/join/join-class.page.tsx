import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft, Loader2 } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router";
import { toast } from "sonner";
import { z } from "zod";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
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
import { useJoinClass } from "../_shared/classes.hook";
import type { Route } from "./+types/join-class.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Join a class · Class Management" }];
}

export default function JoinClassPage() {
    const { t } = useTranslation("classroom");
    const navigate = useNavigate();
    const join = useJoinClass();

    const schema = useMemo(
        () =>
            z.object({
                inviteCode: z.string().regex(/^[A-Za-z0-9]{6,8}$/, t("join.validation.codeFormat")),
            }),
        [t],
    );
    type Values = z.infer<typeof schema>;

    const form = useForm<Values>({
        resolver: zodResolver(schema),
        defaultValues: { inviteCode: "" },
    });

    const onSubmit = (values: Values) => {
        join.mutate(
            { inviteCode: values.inviteCode.trim().toUpperCase() },
            {
                onSuccess: () => {
                    toast.success(t("join.toast.sent"));
                    navigate("/student/classes/requests");
                },
                onError: (error: unknown) =>
                    toast.error(error instanceof ApiError ? error.message : t("toast.error")),
            },
        );
    };

    return (
        <div className="mx-auto w-full max-w-md space-y-6">
            <Link
                to="/student/classes"
                className="text-muted-foreground hover:text-foreground inline-flex items-center gap-1.5 text-sm"
            >
                <ArrowLeft className="size-4" />
                {t("student.title")}
            </Link>

            <Card>
                <CardHeader>
                    <CardTitle>{t("join.title")}</CardTitle>
                    <CardDescription>{t("join.subtitle")}</CardDescription>
                </CardHeader>
                <CardContent>
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
                            <FormField
                                control={form.control}
                                name="inviteCode"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>{t("join.inviteCode")}</FormLabel>
                                        <FormControl>
                                            <Input
                                                placeholder={t("join.placeholder")}
                                                autoComplete="off"
                                                className="tracking-[0.3em] uppercase"
                                                {...field}
                                            />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <Button type="submit" disabled={join.isPending}>
                                {join.isPending ? (
                                    <Loader2 className="size-4 animate-spin" />
                                ) : null}
                                {t("join.submit")}
                            </Button>
                        </form>
                    </Form>
                </CardContent>
            </Card>
        </div>
    );
}
