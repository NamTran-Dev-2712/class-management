import { Eye } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { ExamForm } from "../_shared/exam-form";
import { ExamFormShell } from "../_shared/exam-form-shell";
import { ExamPreviewDialog } from "../_shared/exam-preview";
import { ExamQuestionsEditor } from "../_shared/exam-questions-editor";
import { useExamPreview, useTeacherExam } from "../_shared/exams.hook";
import type { Route } from "./+types/exam-edit.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Edit exam · Class Management" }];
}

export default function ExamEditPage() {
    const { t } = useTranslation("exam");
    const { publicId } = useParams();
    const { data: exam, isLoading } = useTeacherExam(publicId);
    const [previewOpen, setPreviewOpen] = useState(false);
    const { data: preview, isLoading: previewLoading } = useExamPreview(
        previewOpen ? publicId : undefined,
    );

    return (
        <ExamFormShell
            title={t("form.editTitle")}
            subtitle={t("form.editSubtitle")}
            actions={
                exam ? (
                    <Button type="button" variant="outline" onClick={() => setPreviewOpen(true)}>
                        <Eye className="size-4" />
                        <span className="hidden sm:inline">{t("actions.preview")}</span>
                    </Button>
                ) : null
            }
        >
            {isLoading || !exam ? (
                <div className="grid gap-5">
                    <Skeleton className="h-40 w-full" />
                    <Skeleton className="h-64 w-full" />
                </div>
            ) : (
                <div className="space-y-6">
                    <ExamForm exam={exam} />
                    <ExamQuestionsEditor exam={exam} />
                </div>
            )}

            <ExamPreviewDialog
                open={previewOpen}
                onOpenChange={setPreviewOpen}
                exam={preview}
                isLoading={previewLoading}
            />
        </ExamFormShell>
    );
}
