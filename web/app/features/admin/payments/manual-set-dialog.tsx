import { Loader2 } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { ApiError } from "@/lib/api-error";
import { usePlans } from "@/features/teacher/subscription/subscription.hook";
import { useManualSetPro } from "./payments.hook";

interface ManualSetDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    /** Optionally pre-fill the teacher public id (e.g. from a row action). */
    defaultTeacherId?: string;
}

export function ManualSetDialog({ open, onOpenChange, defaultTeacherId }: ManualSetDialogProps) {
    const { t } = useTranslation("payment");
    const plans = usePlans();
    const manualSet = useManualSetPro();
    const [teacherId, setTeacherId] = useState(defaultTeacherId ?? "");
    const [planId, setPlanId] = useState("");
    const [note, setNote] = useState("");

    const proPlans = (plans.data ?? []).filter((p) => p.priceVnd > 0);

    const submit = () => {
        manualSet.mutate(
            {
                teacherPublicId: teacherId.trim(),
                planPublicId: planId,
                adminNote: note || undefined,
            },
            {
                onSuccess: () => {
                    toast.success(t("admin.manualDialog.success"));
                    onOpenChange(false);
                    setTeacherId("");
                    setPlanId("");
                    setNote("");
                },
                onError: (err) => toast.error(err instanceof ApiError ? err.message : "Error"),
            },
        );
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>{t("admin.manualDialog.title")}</DialogTitle>
                    <DialogDescription>{t("admin.manualDialog.description")}</DialogDescription>
                </DialogHeader>

                <div className="space-y-4">
                    <div className="space-y-1.5">
                        <Label>{t("admin.manualDialog.teacherId")}</Label>
                        <Input
                            value={teacherId}
                            onChange={(e) => setTeacherId(e.target.value)}
                            placeholder="00000000-0000-0000-0000-000000000000"
                        />
                    </div>
                    <div className="space-y-1.5">
                        <Label>{t("admin.manualDialog.plan")}</Label>
                        <Select value={planId} onValueChange={setPlanId}>
                            <SelectTrigger>
                                <SelectValue placeholder={t("admin.manualDialog.plan")} />
                            </SelectTrigger>
                            <SelectContent>
                                {proPlans.map((p) => (
                                    <SelectItem key={p.publicId} value={p.publicId}>
                                        {p.name}
                                        {p.billingCycle
                                            ? ` · ${t(`plans.billing.${p.billingCycle}`)}`
                                            : ""}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                    <div className="space-y-1.5">
                        <Label>{t("admin.manualDialog.note")}</Label>
                        <Textarea
                            value={note}
                            maxLength={500}
                            onChange={(e) => setNote(e.target.value)}
                            placeholder={t("admin.manualDialog.notePlaceholder")}
                        />
                    </div>
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        {t("actions.cancel", { ns: "common" })}
                    </Button>
                    <Button
                        onClick={submit}
                        disabled={manualSet.isPending || !teacherId.trim() || !planId}
                    >
                        {manualSet.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                        {t("admin.manualDialog.submit")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
