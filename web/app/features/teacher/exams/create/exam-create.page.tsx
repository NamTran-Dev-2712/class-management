import { useTranslation } from "react-i18next";

import { ExamForm } from "../_shared/exam-form";
import { ExamFormShell } from "../_shared/exam-form-shell";
import type { Route } from "./+types/exam-create.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Create exam · Class Management" }];
}

export default function ExamCreatePage() {
    const { t } = useTranslation("exam");

    return (
        <ExamFormShell title={t("form.createTitle")} subtitle={t("form.createSubtitle")}>
            <ExamForm />
        </ExamFormShell>
    );
}
