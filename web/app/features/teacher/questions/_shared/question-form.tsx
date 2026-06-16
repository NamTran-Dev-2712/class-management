import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { toast } from "sonner";

import {
    MarkdownEditor,
    type MarkdownToolbarLabels,
} from "@/components/shared/markdown/markdown-editor";
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
import { Switch } from "@/components/ui/switch";
import { ApiError } from "@/lib/api-error";
import type {
    CreateQuestionRequest,
    QuestionOptionInput,
} from "@/services/question/dtos/commands/question-commands";
import type { QuestionDetail } from "@/services/question/dtos/queries/question-detail";
import type { QuestionType } from "@/services/question/dtos/queries/question-list";
import { QuestionOptionsField } from "./question-options-field";
import { QuestionTagsField } from "./question-tags-field";
import {
    createQuestionSchema,
    DIFFICULTIES,
    isChoiceType,
    QUESTION_TYPES,
    SUBJECT_NONE,
    type QuestionFormValues,
} from "./question.schema";
import { useActiveSubjects, useCreateQuestion, useUpdateQuestion } from "./questions.hook";

interface QuestionFormProps {
    /** Question to edit, or undefined to create a new one. */
    question?: QuestionDetail;
}

function defaultOptionsForType(
    type: QuestionType,
    current: QuestionOptionInput[],
): QuestionOptionInput[] {
    if (type === "TrueFalse")
        return [
            { content: "True", isCorrect: true },
            { content: "False", isCorrect: false },
        ];
    if (type === "ShortWriting" || type === "LongWriting") return [];
    if (current.length >= 2) return current;
    return [
        { content: "", isCorrect: false },
        { content: "", isCorrect: false },
    ];
}

export function QuestionForm({ question }: QuestionFormProps) {
    const { t } = useTranslation("question");
    const navigate = useNavigate();
    const create = useCreateQuestion();
    const update = useUpdateQuestion();
    const { data: subjects } = useActiveSubjects();
    const isEdit = question !== undefined;

    const schema = useMemo(() => createQuestionSchema(t), [t]);
    const form = useForm<QuestionFormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            subjectId: question?.subjectPublicId ?? SUBJECT_NONE,
            type: question?.type ?? "SingleChoice",
            difficulty: question?.difficulty ?? "Medium",
            visibility: question?.visibility ?? "Private",
            content: question?.content ?? "",
            suggestedPoint: question?.suggestedPoint ?? 1,
            explanation: question?.explanation ?? "",
            options: question
                ? question.options.map((o) => ({ content: o.content, isCorrect: o.isCorrect }))
                : defaultOptionsForType("SingleChoice", []),
            tags: question?.tags ?? [],
        },
    });

    const type = form.watch("type");
    const isPending = create.isPending || update.isPending;

    // Subjects to show: active catalog subjects, plus the question's current subject if it has since
    // been deactivated/removed from the active list (so editing doesn't silently drop it).
    const subjectOptions = useMemo(() => {
        const items = (subjects?.items ?? []).map((s) => ({
            publicId: s.publicId,
            name: s.name,
        }));
        if (
            question?.subjectPublicId &&
            !items.some((s) => s.publicId === question.subjectPublicId)
        ) {
            items.unshift({
                publicId: question.subjectPublicId,
                name: question.subjectName ?? question.subjectPublicId,
            });
        }
        return items;
    }, [subjects, question]);

    // Localized labels for the Markdown editor toolbar (shared by the content + explanation editors).
    const toolbarLabels: MarkdownToolbarLabels = useMemo(
        () => ({
            bold: t("form.toolbar.bold"),
            italic: t("form.toolbar.italic"),
            strikethrough: t("form.toolbar.strikethrough"),
            underline: t("form.toolbar.underline"),
            heading: t("form.toolbar.heading"),
            quote: t("form.toolbar.quote"),
            bulletList: t("form.toolbar.bulletList"),
            orderedList: t("form.toolbar.orderedList"),
            code: t("form.toolbar.code"),
            codeBlock: t("form.toolbar.codeBlock"),
            link: t("form.toolbar.link"),
        }),
        [t],
    );

    const onSubmit = (values: QuestionFormValues) => {
        const payload: CreateQuestionRequest = {
            subjectId: values.subjectId === SUBJECT_NONE ? null : values.subjectId,
            type: values.type,
            content: values.content.trim(),
            difficulty: values.difficulty,
            suggestedPoint: values.suggestedPoint,
            visibility: values.visibility,
            explanation: values.explanation?.trim() ? values.explanation.trim() : null,
            options: isChoiceType(values.type)
                ? values.options.map((o) => ({
                      content: o.content.trim(),
                      isCorrect: o.isCorrect,
                  }))
                : [],
            tags: values.tags,
        };

        const onSuccess = () => {
            toast.success(isEdit ? t("toast.updated") : t("toast.created"));
            navigate("/teacher/question-bank");
        };
        const onError = (error: unknown) =>
            toast.error(error instanceof ApiError ? error.message : t("toast.error"));

        if (isEdit) update.mutate({ publicId: question.publicId, payload }, { onSuccess, onError });
        else create.mutate(payload, { onSuccess, onError });
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-5">
                <div className="grid gap-5 sm:grid-cols-2">
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
                        name="type"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{t("form.type")}</FormLabel>
                                <Select
                                    value={field.value}
                                    onValueChange={(v) => {
                                        const next = v as QuestionType;
                                        field.onChange(next);
                                        form.setValue(
                                            "options",
                                            defaultOptionsForType(next, form.getValues("options")),
                                            { shouldValidate: false },
                                        );
                                    }}
                                >
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        {QUESTION_TYPES.map((qt) => (
                                            <SelectItem key={qt} value={qt}>
                                                {t(`types.${qt}`)}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="content"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("form.content")}</FormLabel>
                            <FormControl>
                                <MarkdownEditor
                                    value={field.value}
                                    onChange={field.onChange}
                                    onBlur={field.onBlur}
                                    placeholder={t("form.contentPlaceholder")}
                                    writeLabel={t("form.write")}
                                    previewLabel={t("form.preview")}
                                    emptyLabel={t("form.previewEmpty")}
                                    toolbarLabels={toolbarLabels}
                                />
                            </FormControl>
                            <FormDescription>{t("form.markdownHint")}</FormDescription>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                {isChoiceType(type) ? (
                    <FormField
                        control={form.control}
                        name="options"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{t("form.options")}</FormLabel>
                                <QuestionOptionsField
                                    type={type}
                                    value={field.value}
                                    onChange={field.onChange}
                                />
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                ) : null}

                <div className="grid gap-5 sm:grid-cols-2">
                    <FormField
                        control={form.control}
                        name="difficulty"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{t("form.difficulty")}</FormLabel>
                                <Select value={field.value} onValueChange={field.onChange}>
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        {DIFFICULTIES.map((d) => (
                                            <SelectItem key={d} value={d}>
                                                {t(`difficulty.${d}`)}
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
                        name="suggestedPoint"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{t("form.suggestedPoint")}</FormLabel>
                                <FormControl>
                                    <Input
                                        type="number"
                                        min={0.5}
                                        max={100}
                                        step={0.5}
                                        {...field}
                                    />
                                </FormControl>
                                <FormDescription>{t("form.suggestedPointHint")}</FormDescription>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="explanation"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("form.explanation")}</FormLabel>
                            <FormControl>
                                <MarkdownEditor
                                    value={field.value ?? ""}
                                    onChange={field.onChange}
                                    onBlur={field.onBlur}
                                    rows={3}
                                    placeholder={t("form.explanationPlaceholder")}
                                    writeLabel={t("form.write")}
                                    previewLabel={t("form.preview")}
                                    emptyLabel={t("form.previewEmpty")}
                                    toolbarLabels={toolbarLabels}
                                />
                            </FormControl>
                            <FormDescription>{t("form.explanationHint")}</FormDescription>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="tags"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>{t("form.tags")}</FormLabel>
                            <QuestionTagsField
                                value={field.value}
                                onChange={field.onChange}
                                placeholder={t("form.tagsPlaceholder")}
                            />
                            <FormDescription>{t("form.tagsHint")}</FormDescription>
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
                                <FormDescription>{t("form.makePublicHint")}</FormDescription>
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
                        onClick={() => navigate("/teacher/question-bank")}
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
