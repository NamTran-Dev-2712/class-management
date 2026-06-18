import type { TFunction } from "i18next";
import { z } from "zod";

export function createAssignmentSchema(t: TFunction) {
    return z
        .object({
            examId: z.string().min(1, t("form.validation.examRequired")),
            classId: z.string().min(1, t("form.validation.classRequired")),
            title: z
                .string()
                .trim()
                .min(3, t("form.validation.titleLength"))
                .max(300, t("form.validation.titleLength")),
            description: z.string().max(1000, t("form.validation.descriptionMax")).optional(),
            opensAt: z.string().optional(),
            closesAt: z.string().optional(),
            timeLimitMinutes: z.coerce.number().int().min(1).max(1440).optional(),
            maxAttempts: z.coerce
                .number()
                .int()
                .min(1, t("form.validation.maxAttempts"))
                .max(100, t("form.validation.maxAttempts")),
            scorePolicy: z.enum(["Highest", "Latest"]),
            allowLate: z.boolean(),
            gradePublishPolicy: z.enum(["Immediate", "AfterDeadline", "Manual"]),
            shuffleQuestions: z.boolean(),
            shuffleOptions: z.boolean(),
            showAnswersAfterGrade: z.boolean(),
        })
        .refine((v) => !v.opensAt || !v.closesAt || new Date(v.closesAt) > new Date(v.opensAt), {
            message: t("form.validation.timeWindow"),
            path: ["closesAt"],
        });
}

export type AssignmentFormValues = z.infer<ReturnType<typeof createAssignmentSchema>>;
