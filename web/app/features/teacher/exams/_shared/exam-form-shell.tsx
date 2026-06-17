import { ArrowLeft } from "lucide-react";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

interface ExamFormShellProps {
    title: string;
    subtitle: string;
    children: ReactNode;
    /** Optional extra content rendered in the header (e.g. action buttons). */
    actions?: ReactNode;
}

/** Shared full-page layout for the create/edit exam screens. */
export function ExamFormShell({ title, subtitle, children, actions }: ExamFormShellProps) {
    const { t } = useTranslation("exam");

    return (
        <div className="mx-auto w-full max-w-3xl space-y-6">
            <Link
                to="/teacher/exams"
                className="text-muted-foreground hover:text-foreground inline-flex items-center gap-1.5 text-sm"
            >
                <ArrowLeft className="size-4" />
                {t("actions.back")}
            </Link>

            <div className="flex items-start justify-between gap-4">
                <div>
                    <h2 className="text-xl font-semibold">{title}</h2>
                    <p className="text-muted-foreground text-sm">{subtitle}</p>
                </div>
                {actions}
            </div>

            {children}
        </div>
    );
}
