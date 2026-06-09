import type { TFunction } from "i18next";
import { z } from "zod";

import { Roles } from "@/config/roles";

/** Build the register schema with localized messages (pass the `auth` t function). */
export function createRegisterSchema(t: TFunction) {
    return z
        .object({
            displayName: z
                .string()
                .min(2, t("register.validation.displayNameMin"))
                .max(100, t("register.validation.displayNameMax")),
            email: z
                .string()
                .min(1, t("register.validation.emailRequired"))
                .email(t("register.validation.emailInvalid")),
            password: z
                .string()
                .min(8, t("register.validation.passwordMin"))
                .regex(/[A-Z]/, t("register.validation.passwordUpper"))
                .regex(/[a-z]/, t("register.validation.passwordLower"))
                .regex(/[0-9]/, t("register.validation.passwordNumber"))
                .regex(/[^a-zA-Z0-9]/, t("register.validation.passwordSpecial")),
            confirmPassword: z.string().min(1, t("register.validation.confirmRequired")),
            role: z.enum([Roles.Student, Roles.Teacher], {
                message: t("register.validation.roleRequired"),
            }),
        })
        .refine((data) => data.password === data.confirmPassword, {
            message: t("register.validation.passwordMismatch"),
            path: ["confirmPassword"],
        });
}

export type RegisterFormValues = z.infer<ReturnType<typeof createRegisterSchema>>;
