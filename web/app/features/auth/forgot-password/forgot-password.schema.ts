import type { TFunction } from "i18next";
import { z } from "zod";

export function createForgotPasswordSchema(t: TFunction) {
    return z.object({
        email: z
            .string()
            .min(1, t("forgotPassword.validation.emailRequired"))
            .email(t("forgotPassword.validation.emailInvalid")),
    });
}

export type ForgotPasswordValues = z.infer<ReturnType<typeof createForgotPasswordSchema>>;
