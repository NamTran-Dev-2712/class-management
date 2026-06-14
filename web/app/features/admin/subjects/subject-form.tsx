import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useEffect, useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
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
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { ApiError } from "@/lib/api-error";
import type { Subject } from "@/services/subject/dtos/queries/list/response";
import { useCreateSubject, useUpdateSubject } from "./subjects.hook";
import { createSubjectSchema, type SubjectFormValues } from "./subject.schema";

interface SubjectFormProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    /** Subject to edit, or null to create a new one. */
    subject: Subject | null;
}

const emptyValues: SubjectFormValues = {
    name: "",
    description: "",
    displayOrder: 0,
    isActive: true,
};

export function SubjectForm({ open, onOpenChange, subject }: SubjectFormProps) {
    const { t } = useTranslation("subject");
    const create = useCreateSubject();
    const update = useUpdateSubject();
    const isEdit = subject !== null;

    const schema = useMemo(() => createSubjectSchema(t), [t]);
    const form = useForm<SubjectFormValues>({
        resolver: zodResolver(schema),
        defaultValues: emptyValues,
    });

    // Sync form when opening for create vs edit.
    useEffect(() => {
        if (!open) return;
        form.reset(
            subject
                ? {
                      name: subject.name,
                      description: subject.description ?? "",
                      displayOrder: subject.displayOrder,
                      isActive: subject.isActive,
                  }
                : emptyValues,
        );
    }, [open, subject, form]);

    const isPending = create.isPending || update.isPending;

    const onSubmit = (values: SubjectFormValues) => {
        const payload = {
            name: values.name.trim(),
            description: values.description?.trim() ? values.description.trim() : null,
            isActive: values.isActive,
            displayOrder: values.displayOrder,
        };

        const onSuccess = () => {
            toast.success(isEdit ? t("toast.updated") : t("toast.created"));
            onOpenChange(false);
        };
        const onError = (error: unknown) => {
            toast.error(error instanceof ApiError ? error.message : t("toast.error"));
        };

        if (isEdit) update.mutate({ publicId: subject.publicId, payload }, { onSuccess, onError });
        else create.mutate(payload, { onSuccess, onError });
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>
                        {isEdit ? t("form.editTitle") : t("form.createTitle")}
                    </DialogTitle>
                    <DialogDescription>{t("form.subtitle")}</DialogDescription>
                </DialogHeader>

                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
                        <FormField
                            control={form.control}
                            name="name"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>{t("form.name")}</FormLabel>
                                    <FormControl>
                                        <Input placeholder={t("form.namePlaceholder")} {...field} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="description"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>{t("form.description")}</FormLabel>
                                    <FormControl>
                                        <Textarea
                                            rows={3}
                                            placeholder={t("form.descriptionPlaceholder")}
                                            {...field}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="displayOrder"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>{t("form.displayOrder")}</FormLabel>
                                    <FormControl>
                                        <Input type="number" min={0} {...field} />
                                    </FormControl>
                                    <FormDescription>{t("form.displayOrderHint")}</FormDescription>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="isActive"
                            render={({ field }) => (
                                <FormItem className="flex items-center justify-between rounded-md border p-3">
                                    <div className="space-y-0.5">
                                        <FormLabel>{t("form.isActive")}</FormLabel>
                                        <FormDescription>{t("form.isActiveHint")}</FormDescription>
                                    </div>
                                    <FormControl>
                                        <Switch
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                </FormItem>
                            )}
                        />

                        <DialogFooter>
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => onOpenChange(false)}
                            >
                                {t("form.cancel")}
                            </Button>
                            <Button type="submit" disabled={isPending}>
                                {isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                                {t("form.save")}
                            </Button>
                        </DialogFooter>
                    </form>
                </Form>
            </DialogContent>
        </Dialog>
    );
}
