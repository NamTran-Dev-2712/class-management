import { useTranslation } from "react-i18next";

import { UserForm } from "../_shared/user-form";
import { UserFormShell } from "../_shared/user-form-shell";
import type { Route } from "./+types/user-create.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Add user · Class Management" }];
}

export default function UserCreatePage() {
    const { t } = useTranslation("user");

    return (
        <UserFormShell title={t("form.createTitle")} subtitle={t("form.createSubtitle")}>
            <UserForm />
        </UserFormShell>
    );
}
