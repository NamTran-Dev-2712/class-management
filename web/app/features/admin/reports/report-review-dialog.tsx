import { Loader2 } from "lucide-react";
import { useEffect, useState } from "react";
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
import type {
    ReportAdminAction,
    ReportListItem,
    ReportStatus,
} from "@/services/report/dtos/report-dtos";
import { useReviewReport } from "./reports.hook";

const REVIEW_STATUSES: ReportStatus[] = ["Reviewing", "Resolved", "Rejected"];
const ACTIONS: ReportAdminAction[] = [
    "Dismiss",
    "WarnUser",
    "HideContent",
    "DeleteContent",
    "BanUser",
];

interface ReportReviewDialogProps {
    report: ReportListItem | null;
    onOpenChange: (open: boolean) => void;
}

export function ReportReviewDialog({ report, onOpenChange }: ReportReviewDialogProps) {
    const { t } = useTranslation("report");
    const review = useReviewReport();

    const [status, setStatus] = useState<ReportStatus>("Resolved");
    const [action, setAction] = useState<ReportAdminAction>("Dismiss");
    const [note, setNote] = useState("");

    useEffect(() => {
        if (report) {
            setStatus("Resolved");
            setAction("Dismiss");
            setNote("");
        }
    }, [report]);

    if (!report) return null;

    // Ban is only valid for user reports.
    const availableActions = ACTIONS.filter((a) => a !== "BanUser" || report.targetType === "User");

    const onSubmit = () => {
        review.mutate(
            {
                publicId: report.publicId,
                payload: {
                    status,
                    adminAction: status === "Resolved" ? action : null,
                    adminNote: note.trim() || null,
                },
            },
            {
                onSuccess: () => {
                    toast.success(t("toast.reviewed"));
                    onOpenChange(false);
                },
                onError: (error: unknown) =>
                    toast.error(error instanceof ApiError ? error.message : t("toast.error")),
            },
        );
    };

    return (
        <Dialog open={report !== null} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{t("review.title")}</DialogTitle>
                    <DialogDescription>
                        {t("review.subtitle", {
                            targetType: report.targetType,
                            reason: t(`reasons.${report.reason}`),
                        })}
                    </DialogDescription>
                </DialogHeader>

                {report.description ? (
                    <p className="bg-muted text-muted-foreground rounded-md p-3 text-sm">
                        {report.description}
                    </p>
                ) : null}

                <div className="grid gap-4">
                    <div className="grid gap-2">
                        <Label>{t("review.status")}</Label>
                        <Select value={status} onValueChange={(v) => setStatus(v as ReportStatus)}>
                            <SelectTrigger>
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                {REVIEW_STATUSES.map((s) => (
                                    <SelectItem key={s} value={s}>
                                        {t(`status.${s}`)}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    {status === "Resolved" ? (
                        <div className="grid gap-2">
                            <Label>{t("review.action")}</Label>
                            <Select
                                value={action}
                                onValueChange={(v) => setAction(v as ReportAdminAction)}
                            >
                                <SelectTrigger>
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    {availableActions.map((a) => (
                                        <SelectItem key={a} value={a}>
                                            {t(`actions.${a}`)}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                            {(action === "BanUser" || action === "DeleteContent") && (
                                <p className="text-destructive text-xs">
                                    {t("review.destructiveWarning")}
                                </p>
                            )}
                        </div>
                    ) : null}

                    <div className="grid gap-2">
                        <Label>{t("review.note")}</Label>
                        <Textarea
                            rows={3}
                            value={note}
                            onChange={(e) => setNote(e.target.value)}
                            placeholder={t("review.notePlaceholder")}
                        />
                    </div>
                </div>

                <DialogFooter>
                    <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                        {t("form.cancel")}
                    </Button>
                    <Button type="button" onClick={onSubmit} disabled={review.isPending}>
                        {review.isPending && <Loader2 className="size-4 animate-spin" />}
                        {t("review.submit")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
