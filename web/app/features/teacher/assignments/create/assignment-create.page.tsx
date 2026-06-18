import { ArrowLeft } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";

import { Button } from "@/components/ui/button";
import { AssignmentForm } from "../_shared/assignment-form";
import type { Route } from "./+types/assignment-create.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "New Assignment · Class Management" }];
}

export default function AssignmentCreatePage() {
    const { t } = useTranslation("assignment");
    const navigate = useNavigate();

    return (
        <div className="mx-auto max-w-3xl space-y-6">
            <div className="flex items-center gap-3">
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigate("/teacher/assignments")}
                >
                    <ArrowLeft className="size-4" />
                </Button>
                <div>
                    <h2 className="text-xl font-semibold">{t("create.title")}</h2>
                    <p className="text-muted-foreground text-sm">{t("create.subtitle")}</p>
                </div>
            </div>
            <AssignmentForm />
        </div>
    );
}
