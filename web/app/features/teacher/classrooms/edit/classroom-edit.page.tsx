import { useTranslation } from "react-i18next";
import { useParams } from "react-router";

import { Skeleton } from "@/components/ui/skeleton";
import { ClassroomForm } from "../_shared/classroom-form";
import { ClassroomFormShell } from "../_shared/classroom-form-shell";
import { useTeacherClass } from "../_shared/classrooms.hook";
import type { Route } from "./+types/classroom-edit.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Edit class · Class Management" }];
}

export default function ClassroomEditPage() {
    const { t } = useTranslation("classroom");
    const { publicId } = useParams();
    const { data: cls, isLoading } = useTeacherClass(publicId);

    return (
        <ClassroomFormShell title={t("form.editTitle")} subtitle={t("form.editSubtitle")}>
            {isLoading || !cls ? (
                <div className="grid gap-5">
                    <Skeleton className="h-10 w-full" />
                    <Skeleton className="h-24 w-full" />
                    <Skeleton className="h-10 w-full" />
                </div>
            ) : (
                <ClassroomForm cls={cls} />
            )}
        </ClassroomFormShell>
    );
}
