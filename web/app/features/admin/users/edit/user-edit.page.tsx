import { useTranslation } from "react-i18next";
import { useParams } from "react-router";

import { Skeleton } from "@/components/ui/skeleton";
import { UserForm } from "../_shared/user-form";
import { UserFormShell } from "../_shared/user-form-shell";
import { useUser } from "../_shared/users.hook";
import type { Route } from "./+types/user-edit.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Edit user · Class Management" }];
}

export default function UserEditPage() {
    const { t } = useTranslation("user");
    const { publicId } = useParams();
    const { data: user, isLoading } = useUser(publicId);

    return (
        <UserFormShell title={t("form.editTitle")} subtitle={t("form.editSubtitle")}>
            {isLoading || !user ? (
                <div className="grid gap-5">
                    <Skeleton className="h-10 w-full" />
                    <Skeleton className="h-10 w-full" />
                    <Skeleton className="h-10 w-full" />
                </div>
            ) : (
                <UserForm user={user} />
            )}
        </UserFormShell>
    );
}
