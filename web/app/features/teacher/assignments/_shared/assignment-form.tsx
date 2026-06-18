import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { toast } from "sonner";

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
import type {
    CreateAssignmentRequest,
    UpdateAssignmentRequest,
} from "@/services/assignment/dtos/commands/assignment-commands";
import type { AssignmentDetail } from "@/services/assignment/dtos/queries/assignment-detail";
import { createAssignmentSchema, type AssignmentFormValues } from "./assignment.schema";
import {
    useClassOptions,
    useCreateAssignment,
    useExamOptions,
    useUpdateAssignment,
} from "./assignments.hook";

interface AssignmentFormProps {
    assignment?: AssignmentDetail;
}

function toLocalInput(iso: string | null | undefined): string {
    if (!iso) return "";
    const d = new Date(iso);
    const local = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
    return local.toISOString().slice(0, 16);
}

function toIso(local: string | undefined): string | null {
    if (!local) return null;
    const d = new Date(local);
    return Number.isNaN(d.getTime()) ? null : d.toISOString();
}

export function AssignmentForm({ assignment }: AssignmentFormProps) {
    const { t } = useTranslation("assignment");
    const navigate = useNavigate();
    const create = useCreateAssignment();
    const update = useUpdateAssignment();
    const { data: exams } = useExamOptions();
    const { data: classes } = useClassOptions();
    const isEdit = assignment !== undefined;

    const schema = useMemo(() => createAssignmentSchema(t), [t]);
    const form = useForm<AssignmentFormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            examId: assignment?.examPublicId ?? "",
            classId: assignment?.classPublicId ?? "",
            title: assignment?.title ?? "",
            description: assignment?.description ?? "",
            opensAt: toLocalInput(assignment?.opensAt),
            closesAt: toLocalInput(assignment?.closesAt),
            timeLimitMinutes: assignment?.timeLimitMinutes ?? undefined,
            maxAttempts: assignment?.maxAttempts ?? 1,
            scorePolicy: assignment?.scorePolicy ?? "Highest",
            allowLate: assignment?.allowLate ?? false,
            gradePublishPolicy: assignment?.gradePublishPolicy ?? "AfterDeadline",
            shuffleQuestions: assignment?.shuffleQuestions ?? false,
            shuffleOptions: assignment?.shuffleOptions ?? false,
            showAnswersAfterGrade: assignment?.showAnswersAfterGrade ?? false,
        },
    });

    const isPending = create.isPending || update.isPending;

    const onError = (error: unknown) =>
        toast.error(error instanceof ApiError ? error.message : t("toast.error"));

    const onSubmit = (values: AssignmentFormValues) => {
        const timeLimit =
            typeof values.timeLimitMinutes === "number" && !Number.isNaN(values.timeLimitMinutes)
                ? values.timeLimitMinutes
                : null;
        const common = {
            title: values.title.trim(),
            description: values.description?.trim() ? values.description.trim() : null,
            opensAt: toIso(values.opensAt),
            closesAt: toIso(values.closesAt),
            timeLimitMinutes: timeLimit,
            maxAttempts: values.maxAttempts,
            scorePolicy: values.scorePolicy,
            allowLate: values.allowLate,
            gradePublishPolicy: values.gradePublishPolicy,
            shuffleQuestions: values.shuffleQuestions,
            shuffleOptions: values.shuffleOptions,
            showAnswersAfterGrade: values.showAnswersAfterGrade,
        };

        if (isEdit) {
            const payload: UpdateAssignmentRequest = common;
            update.mutate(
                { publicId: assignment.publicId, payload },
                { onSuccess: () => toast.success(t("toast.updated")), onError },
            );
        } else {
            const payload: CreateAssignmentRequest = {
                ...common,
                examId: values.examId,
                classId: values.classId,
            };
            create.mutate(payload, {
                onSuccess: (res) => {
                    toast.success(t("toast.created"));
                    navigate(`/teacher/assignments/${res.publicId}`);
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
                        {!isEdit && (
                            <div className="grid gap-5 sm:grid-cols-2">
                                <FormField
                                    control={form.control}
                                    name="examId"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>{t("form.exam")}</FormLabel>
                                            <Select
                                                value={field.value}
                                                onValueChange={field.onChange}
                                            >
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue
                                                            placeholder={t("form.examPlaceholder")}
                                                        />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    {(exams?.items ?? []).map((e) => (
                                                        <SelectItem
                                                            key={e.publicId}
                                                            value={e.publicId}
                                                        >
                                                            {e.title}
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
                                    name="classId"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>{t("form.class")}</FormLabel>
                                            <Select
                                                value={field.value}
                                                onValueChange={field.onChange}
                                            >
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue
                                                            placeholder={t("form.classPlaceholder")}
                                                        />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    {(classes?.items ?? []).map((c) => (
                                                        <SelectItem
                                                            key={c.publicId}
                                                            value={c.publicId}
                                                        >
                                                            {c.name}
                                                        </SelectItem>
                                                    ))}
                                                </SelectContent>
                                            </Select>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>
                        )}

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
                                        <Textarea {...field} rows={3} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <div className="grid gap-5 sm:grid-cols-2">
                            <FormField
                                control={form.control}
                                name="opensAt"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>{t("form.opensAt")}</FormLabel>
                                        <FormControl>
                                            <Input type="datetime-local" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="closesAt"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>{t("form.closesAt")}</FormLabel>
                                        <FormControl>
                                            <Input type="datetime-local" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        <div className="grid gap-5 sm:grid-cols-2">
                            <FormField
                                control={form.control}
                                name="timeLimitMinutes"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>{t("form.timeLimit")}</FormLabel>
                                        <FormControl>
                                            <Input
                                                type="number"
                                                min={1}
                                                max={1440}
                                                value={field.value ?? ""}
                                                onChange={(e) =>
                                                    field.onChange(
                                                        e.target.value === ""
                                                            ? undefined
                                                            : Number(e.target.value),
                                                    )
                                                }
                                                placeholder={t("form.timeLimitPlaceholder")}
                                            />
                                        </FormControl>
                                        <FormDescription>{t("form.timeLimitHint")}</FormDescription>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="maxAttempts"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>{t("form.maxAttempts")}</FormLabel>
                                        <FormControl>
                                            <Input
                                                type="number"
                                                min={1}
                                                max={100}
                                                value={field.value}
                                                onChange={(e) =>
                                                    field.onChange(Number(e.target.value))
                                                }
                                            />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        <div className="grid gap-5 sm:grid-cols-2">
                            <FormField
                                control={form.control}
                                name="scorePolicy"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>{t("form.scorePolicy")}</FormLabel>
                                        <Select value={field.value} onValueChange={field.onChange}>
                                            <FormControl>
                                                <SelectTrigger>
                                                    <SelectValue />
                                                </SelectTrigger>
                                            </FormControl>
                                            <SelectContent>
                                                <SelectItem value="Highest">
                                                    {t("scorePolicy.Highest")}
                                                </SelectItem>
                                                <SelectItem value="Latest">
                                                    {t("scorePolicy.Latest")}
                                                </SelectItem>
                                            </SelectContent>
                                        </Select>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="gradePublishPolicy"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>{t("form.gradePublishPolicy")}</FormLabel>
                                        <Select value={field.value} onValueChange={field.onChange}>
                                            <FormControl>
                                                <SelectTrigger>
                                                    <SelectValue />
                                                </SelectTrigger>
                                            </FormControl>
                                            <SelectContent>
                                                <SelectItem value="Immediate">
                                                    {t("gradePublishPolicy.Immediate")}
                                                </SelectItem>
                                                <SelectItem value="AfterDeadline">
                                                    {t("gradePublishPolicy.AfterDeadline")}
                                                </SelectItem>
                                                <SelectItem value="Manual">
                                                    {t("gradePublishPolicy.Manual")}
                                                </SelectItem>
                                            </SelectContent>
                                        </Select>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        <ToggleField
                            control={form.control}
                            name="allowLate"
                            label={t("form.allowLate")}
                            hint={t("form.allowLateHint")}
                        />
                        <ToggleField
                            control={form.control}
                            name="shuffleQuestions"
                            label={t("form.shuffleQuestions")}
                            hint={t("form.shuffleQuestionsHint")}
                        />
                        <ToggleField
                            control={form.control}
                            name="shuffleOptions"
                            label={t("form.shuffleOptions")}
                            hint={t("form.shuffleOptionsHint")}
                        />
                        <ToggleField
                            control={form.control}
                            name="showAnswersAfterGrade"
                            label={t("form.showAnswers")}
                            hint={t("form.showAnswersHint")}
                        />

                        <div className="flex justify-end gap-2">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => navigate("/teacher/assignments")}
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

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function ToggleField({
    control,
    name,
    label,
    hint,
}: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    control: any;
    name: "allowLate" | "shuffleQuestions" | "shuffleOptions" | "showAnswersAfterGrade";
    label: string;
    hint: string;
}) {
    return (
        <FormField
            control={control}
            name={name}
            render={({ field }) => (
                <FormItem className="flex items-center justify-between rounded-md border p-3">
                    <div className="space-y-0.5">
                        <FormLabel>{label}</FormLabel>
                        <FormDescription>{hint}</FormDescription>
                    </div>
                    <FormControl>
                        <Switch checked={field.value} onCheckedChange={field.onChange} />
                    </FormControl>
                </FormItem>
            )}
        />
    );
}
