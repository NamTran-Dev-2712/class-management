import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { toast } from "sonner";

import { useActiveSubjects } from "@/features/teacher/questions/_shared/questions.hook";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
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
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { ApiError } from "@/lib/api-error";
import type { CreateExamRequest } from "@/services/exam/dtos/commands/exam-commands";
import type { ExamDetail } from "@/services/exam/dtos/queries/exam-detail";
import { createExamSchema, SUBJECT_NONE, type ExamFormValues } from "./exam.schema";
import { useCreateExam, useUpdateExam } from "./exams.hook";

interface ExamFormProps {
    /** Exam to edit, or undefined to create a new one. */
    exam?: ExamDetail;
}

export function ExamForm({ exam }: ExamFormProps) {
    const { t } = useTranslation("exam");
    const navigate = useNavigate();
    const create = useCreateExam();
    const update = useUpdateExam();
    const { data: subjects } = useActiveSubjects();
    const isEdit = exam !== undefined;

    const schema = useMemo(() => createExamSchema(t), [t]);
    const form = useForm<ExamFormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            subjectId: exam?.subjectPublicId ?? SUBJECT_NONE,
            visibility: exam?.visibility ?? "Private",
            title: exam?.title ?? "",
            description: exam?.description ?? "",
        },
    });

    const isPending = create.isPending || update.isPending;

    // Active catalog subjects, plus the exam's current subject if it has since been deactivated.
    const subjectOptions = useMemo(() => {
        const items = (subjects?.items ?? []).map((s) => ({ publicId: s.publicId, name: s.name }));
        if (exam?.subjectPublicId && !items.some((s) => s.publicId === exam.subjectPublicId)) {
            items.unshift({
                publicId: exam.subjectPublicId,
                name: exam.subjectName ?? exam.subjectPublicId,
            });
        }
        return items;
    }, [subjects, exam]);

    const onSubmit = (values: ExamFormValues) => {
        const payload: CreateExamRequest = {
            subjectId: values.subjectId === SUBJECT_NONE ? null : values.subjectId,
            title: values.title.trim(),
            description: values.description?.trim() ? values.description.trim() : null,
            visibility: values.visibility,
        };

        const onError = (error: unknown) =>
            toast.error(error instanceof ApiError ? error.message : t("toast.error"));

        if (isEdit) {
            update.mutate(
                { publicId: exam.publicId, payload },
                {
                    onSuccess: () => toast.success(t("toast.updated")),
                    onError,
                },
            );
        } else {
            create.mutate(payload, {
                onSuccess: (res) => {
                    toast.success(t("toast.created"));
                    // Continue to the editor to add questions.
                    navigate(`/teacher/exams/${res.publicId}`);
                },
                onError,
            });
        }
    };

    return (
        <Card>
            <CardContent className="pt-6">
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-5">
                        <FormField
                            control={form.control}
                            name="title"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>{t("form.title")}</FormLabel>
                                    <FormControl>
                                        <Input
                                            {...field}
                                            placeholder={t("form.titlePlaceholder")}
                                        />
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
                                            {...field}
                                            rows={3}
                                            placeholder={t("form.descriptionPlaceholder")}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <FormField
                            control={form.control}
                            name="subjectId"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>{t("form.subject")}</FormLabel>
                                    <Select value={field.value} onValueChange={field.onChange}>
                                        <FormControl>
                                            <SelectTrigger>
                                                <SelectValue
                                                    placeholder={t("form.subjectPlaceholder")}
                                                />
                                            </SelectTrigger>
                                        </FormControl>
                                        <SelectContent>
                                            <SelectItem value={SUBJECT_NONE}>
                                                {t("form.subjectNone")}
                                            </SelectItem>
                                            {subjectOptions.map((s) => (
                                                <SelectItem key={s.publicId} value={s.publicId}>
                                                    {s.name}
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
                            name="visibility"
                            render={({ field }) => (
                                <FormItem className="flex items-center justify-between rounded-md border p-3">
                                    <div className="space-y-0.5">
                                        <FormLabel>{t("form.makePublic")}</FormLabel>
                                        <FormDescription>
                                            {t("form.makePublicHint")}
                                        </FormDescription>
                                    </div>
                                    <FormControl>
                                        <Switch
                                            checked={field.value === "Public"}
                                            onCheckedChange={(c) =>
                                                field.onChange(c ? "Public" : "Private")
                                            }
                                        />
                                    </FormControl>
                                </FormItem>
                            )}
                        />

                        <div className="flex justify-end gap-2">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => navigate("/teacher/exams")}
                            >
                                {t("form.cancel")}
                            </Button>
                            <Button type="submit" disabled={isPending}>
                                {isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                                {isEdit ? t("form.save") : t("form.saveAndContinue")}
                            </Button>
                        </div>
                    </form>
                </Form>
            </CardContent>
        </Card>
    );
}
