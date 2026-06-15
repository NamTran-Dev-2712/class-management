import type { TFunction } from "i18next";
import { z } from "zod";

/** Sentinel values for the subject picker (a catalog id selects a catalog subject). */
export const SUBJECT_NONE = "none";
export const SUBJECT_CUSTOM = "custom";

export function createClassSchema(t: TFunction) {
    return z
        .object({
            name: z
                .string()
                .min(2, t("form.validation.nameLength"))
                .max(200, t("form.validation.nameLength")),
            description: z.string().max(1000, t("form.validation.descriptionMax")).optional(),
            subjectChoice: z.string(),
            customSubject: z.string().max(200, t("form.validation.subjectMax")).optional(),
        })
        .refine(
            (d) => d.subjectChoice !== SUBJECT_CUSTOM || (d.customSubject?.trim().length ?? 0) >= 1,
            { message: t("form.validation.customSubjectRequired"), path: ["customSubject"] },
        );
}

export type ClassFormValues = z.infer<ReturnType<typeof createClassSchema>>;
