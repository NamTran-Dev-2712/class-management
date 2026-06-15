import { useTranslation } from "react-i18next";

import { ClassroomForm } from "../_shared/classroom-form";
import { ClassroomFormShell } from "../_shared/classroom-form-shell";
import type { Route } from "./+types/classroom-create.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Create class · Class Management" }];
}

export default function ClassroomCreatePage() {
    const { t } = useTranslation("classroom");

    return (
        <ClassroomFormShell title={t("form.createTitle")} subtitle={t("form.createSubtitle")}>
            <ClassroomForm />
        </ClassroomFormShell>
    );
}
