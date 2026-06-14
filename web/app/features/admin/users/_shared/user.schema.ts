import type { TFunction } from "i18next";
import { z } from "zod";

import { Roles } from "@/config/roles";

const roleValues = [Roles.Admin, Roles.Teacher, Roles.Student] as const;

export function createUserSchema(t: TFunction) {
    return z.object({
        displayName: z
            .string()
            .min(2, t("form.validation.displayNameLength"))
            .max(100, t("form.validation.displayNameLength")),
        email: z
            .string()
            .min(1, t("form.validation.emailRequired"))
            .email(t("form.validation.emailInvalid")),
        role: z.enum(roleValues, { message: t("form.validation.roleRequired") }),
        phoneNumber: z
            .string()
            .regex(/^\+?[0-9\s\-()]{6,20}$/, t("form.validation.phoneInvalid"))
            .optional()
            .or(z.literal("")),
    });
}

export type UserFormValues = z.infer<ReturnType<typeof createUserSchema>>;
