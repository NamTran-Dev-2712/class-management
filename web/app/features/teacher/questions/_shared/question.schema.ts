import type { TFunction } from "i18next";
import { z } from "zod";

export const QUESTION_TYPES = [
    "SingleChoice",
    "MultipleChoice",
    "TrueFalse",
    "ShortWriting",
    "LongWriting",
] as const;

export const DIFFICULTIES = ["Easy", "Medium", "Hard"] as const;
export const VISIBILITIES = ["Private", "Public"] as const;

export const CHOICE_TYPES = ["SingleChoice", "MultipleChoice", "TrueFalse"] as const;

/** Sentinel for the subject picker — subject is optional, so "none" maps to a null subjectId. */
export const SUBJECT_NONE = "none";

export function isChoiceType(type: string): boolean {
    return (CHOICE_TYPES as readonly string[]).includes(type);
}

export const MAX_TAGS = 10;

export function createQuestionSchema(t: TFunction) {
    return z
        .object({
            subjectId: z.string(),
            type: z.enum(QUESTION_TYPES),
            difficulty: z.enum(DIFFICULTIES),
            visibility: z.enum(VISIBILITIES),
            content: z
                .string()
                .trim()
                .min(10, t("form.validation.contentLength"))
                .max(10000, t("form.validation.contentLength")),
            suggestedPoint: z.coerce
                .number({ invalid_type_error: t("form.validation.pointRange") })
                .gt(0, t("form.validation.pointRange"))
                .lte(100, t("form.validation.pointRange")),
            explanation: z.string().max(2000, t("form.validation.explanationMax")).optional(),
            options: z
                .array(
                    z.object({
                        content: z.string(),
                        isCorrect: z.boolean(),
                    }),
                )
                .max(10),
            tags: z.array(z.string()).max(MAX_TAGS, t("form.validation.tooManyTags")),
        })
        .superRefine((data, ctx) => {
            if (!isChoiceType(data.type)) return;

            const options = data.options;
            const correct = options.filter((o) => o.isCorrect).length;

            if (data.type === "TrueFalse") {
                if (correct !== 1)
                    ctx.addIssue({
                        code: z.ZodIssueCode.custom,
                        message: t("form.validation.trueFalseOneCorrect"),
                        path: ["options"],
                    });
                return;
            }

            // SingleChoice / MultipleChoice
            if (options.length < 2)
                ctx.addIssue({
                    code: z.ZodIssueCode.custom,
                    message: t("form.validation.optionsMin"),
                    path: ["options"],
                });

            options.forEach((o, i) => {
                if (!o.content.trim())
                    ctx.addIssue({
                        code: z.ZodIssueCode.custom,
                        message: t("form.validation.optionContentRequired"),
                        path: ["options", i, "content"],
                    });
            });

            if (data.type === "SingleChoice" && correct !== 1)
                ctx.addIssue({
                    code: z.ZodIssueCode.custom,
                    message: t("form.validation.singleChoiceOneCorrect"),
                    path: ["options"],
                });

            if (data.type === "MultipleChoice" && correct < 1)
                ctx.addIssue({
                    code: z.ZodIssueCode.custom,
                    message: t("form.validation.multipleChoiceOneCorrect"),
                    path: ["options"],
                });
        });
}

export type QuestionFormValues = z.infer<ReturnType<typeof createQuestionSchema>>;
