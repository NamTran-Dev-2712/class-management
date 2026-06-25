import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { Loader2 } from "lucide-react";
import { useEffect, useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

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
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { ApiError } from "@/lib/api-error";
import { reportService } from "@/services/report/report.service";
import type { ReportReason, ReportTargetType } from "@/services/report/dtos/report-dtos";

const REASONS: ReportReason[] = [
    "InappropriateContent",
    "Spam",
    "Copyright",
    "IncorrectAnswer",
    "Other",
];

interface ReportDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    targetType: ReportTargetType;
    targetPublicId: string;
}

export function ReportDialog({
    open,
    onOpenChange,
    targetType,
    targetPublicId,
}: ReportDialogProps) {
    const { t } = useTranslation("report");

    const schema = useMemo(
        () =>
            z.object({
                reason: z.enum([
                    "InappropriateContent",
                    "Spam",
                    "Copyright",
                    "IncorrectAnswer",
                    "Other",
                ]),
                description: z.string().max(2000, t("form.descriptionMax")).optional(),
            }),
        [t],
    );
    type FormValues = z.infer<typeof schema>;

    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: { reason: "InappropriateContent", description: "" },
    });

    useEffect(() => {
        if (open) form.reset({ reason: "InappropriateContent", description: "" });
    }, [open, form]);

    const submit = useMutation({
        mutationFn: (values: FormValues) =>
            reportService.submit({
                targetType,
                targetPublicId,
                reason: values.reason,
                description: values.description?.trim() || null,
            }),
        onSuccess: () => {
            toast.success(t("toast.submitted"));
            onOpenChange(false);
        },
        onError: (error: unknown) =>
            toast.error(error instanceof ApiError ? error.message : t("toast.error")),
    });

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{t("dialog.title")}</DialogTitle>
                    <DialogDescription>{t("dialog.description")}</DialogDescription>
                </DialogHeader>
                <Form {...form}>
                    <form
                        onSubmit={form.handleSubmit((v) => submit.mutate(v))}
                        className="grid gap-4"
                    >
                        <FormField
                            control={form.control}
                            name="reason"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>{t("form.reason")}</FormLabel>
                                    <Select value={field.value} onValueChange={field.onChange}>
                                        <FormControl>
                                            <SelectTrigger>
                                                <SelectValue />
                                            </SelectTrigger>
                                        </FormControl>
                                        <SelectContent>
                                            {REASONS.map((r) => (
                                                <SelectItem key={r} value={r}>
                                                    {t(`reasons.${r}`)}
                                                </SelectItem>
                                            ))}
                                        </SelectContent>
                                    </Select>
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
                                            rows={4}
                                            placeholder={t("form.descriptionPlaceholder")}
                                            {...field}
                                        />
                                    </FormControl>
                                    <FormMessage />
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
                            <Button type="submit" disabled={submit.isPending}>
                                {submit.isPending && <Loader2 className="size-4 animate-spin" />}
                                {t("form.submit")}
                            </Button>
                        </DialogFooter>
                    </form>
                </Form>
            </DialogContent>
        </Dialog>
    );
}
