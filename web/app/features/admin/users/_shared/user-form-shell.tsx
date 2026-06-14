import { ArrowLeft } from "lucide-react";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

interface UserFormShellProps {
    title: string;
    subtitle: string;
    children: ReactNode;
}

/** Shared full-page layout for the create/edit user forms (consistent header + card). */
export function UserFormShell({ title, subtitle, children }: UserFormShellProps) {
    const { t } = useTranslation("user");

    return (
        <div className="mx-auto w-full max-w-2xl space-y-6">
            <Link
                to="/admin/users"
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
