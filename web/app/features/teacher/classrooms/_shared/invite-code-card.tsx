import { Copy, RefreshCw } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ApiError } from "@/lib/api-error";
import { useRegenerateInviteCode } from "./classrooms.hook";

interface InviteCodeCardProps {
    publicId: string;
    inviteCode: string;
}

export function InviteCodeCard({ publicId, inviteCode }: InviteCodeCardProps) {
    const { t } = useTranslation("classroom");
    const regenerate = useRegenerateInviteCode();
    const [confirmOpen, setConfirmOpen] = useState(false);

    const copy = async () => {
        try {
            await navigator.clipboard.writeText(inviteCode);
            toast.success(t("invite.copied"));
        } catch {
            toast.error(t("toast.error"));
        }
    };

    const confirmRegenerate = () => {
        regenerate.mutate(publicId, {
            onSuccess: () => {
                toast.success(t("invite.regenerated"));
                setConfirmOpen(false);
            },
            onError: (error: unknown) =>
                toast.error(error instanceof ApiError ? error.message : t("toast.error")),
        });
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>{t("invite.title")}</CardTitle>
                <CardDescription>{t("invite.subtitle")}</CardDescription>
            </CardHeader>
            <CardContent className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <code className="bg-muted rounded-md px-4 py-2 text-lg font-semibold tracking-[0.3em]">
                    {inviteCode}
                </code>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={copy}>
                        <Copy className="size-4" />
                        {t("invite.copy")}
                    </Button>
                    <Button variant="outline" onClick={() => setConfirmOpen(true)}>
                        <RefreshCw className="size-4" />
                        {t("invite.regenerate")}
                    </Button>
                </div>
            </CardContent>

            <ConfirmDialog
                open={confirmOpen}
                onOpenChange={setConfirmOpen}
                title={t("invite.regenerateConfirm.title")}
                description={t("invite.regenerateConfirm.description")}
                confirmLabel={t("invite.regenerate")}
                onConfirm={confirmRegenerate}
                isPending={regenerate.isPending}
                destructive
            />
        </Card>
    );
}
