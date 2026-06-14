import { ShieldX } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { Button } from "@/components/ui/button";
import type { Route } from "./+types/unauthorized.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Access denied · Class Management" }];
}

export default function UnauthorizedPage() {
    const { t } = useTranslation("common");

    return (
        <div className="mx-auto flex max-w-md flex-1 flex-col items-center justify-center gap-4 px-4 py-16 text-center">
            <ShieldX className="text-destructive size-12" />
            <h1 className="text-2xl font-semibold">{t("errors.unauthorized.title")}</h1>
            <p className="text-muted-foreground">{t("errors.unauthorized.description")}</p>
            <Button asChild>
                <Link to="/">{t("errors.unauthorized.back")}</Link>
            </Button>
        </div>
    );
}
