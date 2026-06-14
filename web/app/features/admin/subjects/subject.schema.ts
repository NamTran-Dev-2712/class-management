import type { TFunction } from "i18next";
import { z } from "zod";

export function createSubjectSchema(t: TFunction) {
    return z.object({
        name: z
            .string()
            .min(2, t("form.validation.nameLength"))
            .max(100, t("form.validation.nameLength")),
        description: z.string().max(500, t("form.validation.descriptionMax")).optional(),
        displayOrder: z.coerce.number().int().min(0, t("form.validation.displayOrderInvalid")),
        isActive: z.boolean(),
    });
}

export type SubjectFormValues = z.infer<ReturnType<typeof createSubjectSchema>>;
