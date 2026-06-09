import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
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
import { Textarea } from "@/components/ui/textarea";
import { createContactSchema, type ContactFormValues } from "./contact.schema";

export function ContactForm() {
    const { t } = useTranslation("public");
    const [submitting, setSubmitting] = useState(false);

    const schema = useMemo(() => createContactSchema(t), [t]);
    const form = useForm<ContactFormValues>({
        resolver: zodResolver(schema),
        defaultValues: { name: "", email: "", subject: "", message: "" },
    });

    // Front-end-only placeholder — simulates a send so the UX is complete; wire to
    // a backend `/contact` endpoint later without touching this component's shape.
    const onSubmit = async () => {
        setSubmitting(true);
        await new Promise((resolve) => setTimeout(resolve, 700));
        setSubmitting(false);
        toast.success(t("contact.success"));
        form.reset();
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
                <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("contact.form.name")}</FormLabel>
                            <FormControl>
                                <Input placeholder={t("contact.form.namePlaceholder")} {...field} />
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
                            <FormLabel>{t("contact.form.email")}</FormLabel>
                            <FormControl>
                                <Input
                                    type="email"
                                    placeholder={t("contact.form.emailPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <FormField
                    control={form.control}
                    name="subject"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("contact.form.subject")}</FormLabel>
                            <FormControl>
                                <Input
                                    placeholder={t("contact.form.subjectPlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <FormField
                    control={form.control}
                    name="message"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("contact.form.message")}</FormLabel>
                            <FormControl>
                                <Textarea
                                    rows={5}
                                    placeholder={t("contact.form.messagePlaceholder")}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <Button type="submit" disabled={submitting}>
                    {submitting ? <Loader2 className="size-4 animate-spin" /> : null}
                    {t("contact.form.submit")}
                </Button>
            </form>
        </Form>
    );
}
