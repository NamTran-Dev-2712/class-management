import type { TFunction } from "i18next";
import { z } from "zod";

export function createResetPasswordSchema(t: TFunction) {
    return z
        .object({
            email: z
                .string()
                .min(1, t("resetPassword.validation.emailRequired"))
                .email(t("resetPassword.validation.emailInvalid")),
            otp: z.string().regex(/^[0-9]{6}$/, t("resetPassword.validation.otp")),
            newPassword: z
                .string()
                .min(8, t("resetPassword.validation.passwordMin"))
                .regex(/[A-Z]/, t("resetPassword.validation.passwordUpper"))
                .regex(/[a-z]/, t("resetPassword.validation.passwordLower"))
                .regex(/[0-9]/, t("resetPassword.validation.passwordNumber"))
                .regex(/[^a-zA-Z0-9]/, t("resetPassword.validation.passwordSpecial")),
            confirmNewPassword: z.string().min(1, t("resetPassword.validation.confirmRequired")),
        })
        .refine((data) => data.newPassword === data.confirmNewPassword, {
            message: t("resetPassword.validation.passwordMismatch"),
            path: ["confirmNewPassword"],
        });
}

export type ResetPasswordValues = z.infer<ReturnType<typeof createResetPasswordSchema>>;
