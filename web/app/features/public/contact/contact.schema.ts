import type { TFunction } from "i18next";
import { z } from "zod";

/** Build the contact schema with localized messages (pass the `public` t function). */
export function createContactSchema(t: TFunction) {
    return z.object({
        name: z.string().min(2, t("contact.validation.nameRequired")),
        email: z
            .string()
            .min(1, t("contact.validation.emailRequired"))
            .email(t("contact.validation.emailInvalid")),
        subject: z.string().min(3, t("contact.validation.subjectRequired")),
        message: z.string().min(10, t("contact.validation.messageMin")),
    });
}

export type ContactFormValues = z.infer<ReturnType<typeof createContactSchema>>;
