import { useTranslation } from "react-i18next";

import { QuestionForm } from "../_shared/question-form";
import { QuestionFormShell } from "../_shared/question-form-shell";
import type { Route } from "./+types/question-create.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Create question · Class Management" }];
}

export default function QuestionCreatePage() {
    const { t } = useTranslation("question");

    return (
        <QuestionFormShell title={t("form.createTitle")} subtitle={t("form.createSubtitle")}>
            <QuestionForm />
        </QuestionFormShell>
    );
}
