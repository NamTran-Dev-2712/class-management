import { ArrowLeft } from "lucide-react";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

interface ClassroomFormShellProps {
    title: string;
    subtitle: string;
    children: ReactNode;
}

/** Shared full-page layout for the create/edit class forms. */
export function ClassroomFormShell({ title, subtitle, children }: ClassroomFormShellProps) {
    const { t } = useTranslation("classroom");

    return (
        <div className="mx-auto w-full max-w-2xl space-y-6">
            <Link
                to="/teacher/classrooms"
                className="text-muted-foreground hover:text-foreground inline-flex items-center gap-1.5 text-sm"
            >
                <ArrowLeft className="size-4" />
                {t("actions.back")}
            </Link>

            <Card>
                <CardHeader>
                    <CardTitle>{title}</CardTitle>
                    <CardDescription>{subtitle}</CardDescription>
                </CardHeader>
                <CardContent>{children}</CardContent>
            </Card>
        </div>
    );
}
