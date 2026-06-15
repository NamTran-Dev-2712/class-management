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
import { Textarea } from "@/components/ui/textarea";
import { ApiError } from "@/lib/api-error";
import type { CreateClassRequest } from "@/services/classroom/dtos/commands/class-commands";
import type { ClassDetail } from "@/services/classroom/dtos/queries/class-detail";
import {
    createClassSchema,
    SUBJECT_CUSTOM,
    SUBJECT_NONE,
    type ClassFormValues,
} from "./classroom.schema";
import { useActiveSubjects, useCreateClass, useUpdateClass } from "./classrooms.hook";

interface ClassroomFormProps {
    /** Class to edit, or undefined to create a new one. */
    cls?: ClassDetail;
}

export function ClassroomForm({ cls }: ClassroomFormProps) {
    const { t } = useTranslation("classroom");
    const navigate = useNavigate();
    const create = useCreateClass();
    const update = useUpdateClass();
    const { data: subjects } = useActiveSubjects();
    const isEdit = cls !== undefined;

    const schema = useMemo(() => createClassSchema(t), [t]);
    const form = useForm<ClassFormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            name: cls?.name ?? "",
            description: cls?.description ?? "",
            subjectChoice: cls?.subjectId
                ? cls.subjectId
                : cls?.subjectName
                  ? SUBJECT_CUSTOM
                  : SUBJECT_NONE,
            customSubject: cls?.subjectId ? "" : (cls?.subjectName ?? ""),
        },
    });

    const subjectChoice = form.watch("subjectChoice");
    const isPending = create.isPending || update.isPending;

    const onSubmit = (values: ClassFormValues) => {
        const payload: CreateClassRequest = {
            name: values.name.trim(),
            description: values.description?.trim() ? values.description.trim() : null,
            subjectId:
                values.subjectChoice === SUBJECT_NONE || values.subjectChoice === SUBJECT_CUSTOM
                    ? null
                    : values.subjectChoice,
            subjectName:
                values.subjectChoice === SUBJECT_CUSTOM
                    ? (values.customSubject?.trim() ?? null)
                    : null,
            coverImageUrl: cls?.coverImageUrl ?? null,
        };

        const onSuccess = () => {
            toast.success(isEdit ? t("toast.updated") : t("toast.created"));
            navigate("/teacher/classrooms");
        };
        const onError = (error: unknown) => {
            toast.error(error instanceof ApiError ? error.message : t("toast.error"));
        };

        if (isEdit) update.mutate({ publicId: cls.publicId, payload }, { onSuccess, onError });
        else create.mutate(payload, { onSuccess, onError });
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-5">
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
                    name="subjectChoice"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("form.subject")}</FormLabel>
                            <Select value={field.value} onValueChange={field.onChange}>
                                <FormControl>
                                    <SelectTrigger>
                                        <SelectValue placeholder={t("form.subjectPlaceholder")} />
                                    </SelectTrigger>
                                </FormControl>
                                <SelectContent>
                                    <SelectItem value={SUBJECT_NONE}>
                                        {t("form.subjectNone")}
                                    </SelectItem>
                                    {(subjects?.items ?? []).map((s) => (
                                        <SelectItem key={s.publicId} value={s.publicId}>
                                            {s.name}
                                        </SelectItem>
                                    ))}
                                    <SelectItem value={SUBJECT_CUSTOM}>
                                        {t("form.subjectCustom")}
                                    </SelectItem>
                                </SelectContent>
                            </Select>
                            <FormDescription>{t("form.subjectHint")}</FormDescription>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                {subjectChoice === SUBJECT_CUSTOM ? (
                    <FormField
                        control={form.control}
                        name="customSubject"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{t("form.customSubject")}</FormLabel>
                                <FormControl>
                                    <Input
                                        placeholder={t("form.customSubjectPlaceholder")}
                                        {...field}
                                    />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                ) : null}

                <div className="flex justify-end gap-2">
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => navigate("/teacher/classrooms")}
                    >
                        {t("form.cancel")}
                    </Button>
                    <Button type="submit" disabled={isPending}>
                        {isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                        {t("form.save")}
                    </Button>
                </div>
            </form>
        </Form>
    );
}
