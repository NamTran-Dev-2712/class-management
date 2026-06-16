import { useTranslation } from "react-i18next";
import { useParams } from "react-router";

import { Skeleton } from "@/components/ui/skeleton";
import { QuestionForm } from "../_shared/question-form";
import { QuestionFormShell } from "../_shared/question-form-shell";
import { useTeacherQuestion } from "../_shared/questions.hook";
import type { Route } from "./+types/question-edit.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Edit question · Class Management" }];
}

export default function QuestionEditPage() {
    const { t } = useTranslation("question");
    const { publicId } = useParams();
    const { data: question, isLoading } = useTeacherQuestion(publicId);

    return (
        <QuestionFormShell title={t("form.editTitle")} subtitle={t("form.editSubtitle")}>
            {isLoading || !question ? (
                <div className="grid gap-5">
                    <Skeleton className="h-10 w-full" />
                    <Skeleton className="h-32 w-full" />
                    <Skeleton className="h-10 w-full" />
                </div>
            ) : (
                <QuestionForm question={question} />
            )}
        </QuestionFormShell>
    );
}
