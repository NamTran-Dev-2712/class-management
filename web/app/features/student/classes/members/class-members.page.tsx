import { ArrowLeft } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useStudentClassMembers } from "../_shared/classes.hook";
import type { Route } from "./+types/class-members.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Classmates · Class Management" }];
}

export default function StudentClassMembersPage() {
    const { t } = useTranslation("classroom");
    const { publicId } = useParams();
    const { data, isLoading } = useStudentClassMembers(publicId, {
        pageNumber: 1,
        pageSize: 100,
    });
    const items = data?.items ?? [];

    return (
        <div className="mx-auto w-full max-w-2xl space-y-6">
            <Link
                to="/student/classes"
                className="text-muted-foreground hover:text-foreground inline-flex items-center gap-1.5 text-sm"
            >
                <ArrowLeft className="size-4" />
                {t("student.title")}
            </Link>

            <Card>
                <CardHeader>
                    <CardTitle>{t("student.membersTitle")}</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                    {isLoading ? (
                        <Skeleton className="h-12 w-full" />
                    ) : items.length === 0 ? (
                        <p className="text-muted-foreground py-4 text-center text-sm">
                            {t("student.noMembers")}
                        </p>
                    ) : (
                        items.map((m) => (
                            <div key={m.publicId} className="rounded-lg border p-3">
                                <p className="font-medium">{m.studentName}</p>
                                <p className="text-muted-foreground text-sm">{m.studentEmail}</p>
                            </div>
                        ))
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
