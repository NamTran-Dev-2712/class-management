import type { TFunction } from "i18next";
import { z } from "zod";

/** Build the login schema with localized messages (pass the `auth` t function). */
export function createLoginSchema(t: TFunction) {
    return z.object({
        email: z
            .string()
            .min(1, t("login.validation.emailRequired"))
            .email(t("login.validation.emailInvalid")),
        password: z.string().min(1, t("login.validation.passwordRequired")),
    });
}

export type LoginFormValues = z.infer<ReturnType<typeof createLoginSchema>>;
