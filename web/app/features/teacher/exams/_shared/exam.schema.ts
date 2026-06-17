import type { TFunction } from "i18next";
import { z } from "zod";

export const VISIBILITIES = ["Private", "Public"] as const;

/** Sentinel for the subject picker — subject is optional, so "none" maps to a null subjectId. */
export const SUBJECT_NONE = "none";

export function createExamSchema(t: TFunction) {
    return z.object({
        subjectId: z.string(),
        visibility: z.enum(VISIBILITIES),
        title: z
            .string()
            .trim()
            .min(3, t("form.validation.titleLength"))
            .max(300, t("form.validation.titleLength")),
        description: z.string().max(1000, t("form.validation.descriptionMax")).optional(),
    });
}

export type ExamFormValues = z.infer<ReturnType<typeof createExamSchema>>;
