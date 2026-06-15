import { ArrowLeft, Check, UserMinus, X } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ApiError } from "@/lib/api-error";
import type { ClassMember } from "@/services/classroom/dtos/queries/class-member";
import { InviteCodeCard } from "../_shared/invite-code-card";
import {
    useApproveMember,
    useClassMembers,
    useKickMember,
    useRejectMember,
    useTeacherClass,
} from "../_shared/classrooms.hook";
import type { Route } from "./+types/class-members.page";

export function meta(_: Route.MetaArgs) {
    return [{ title: "Class members · Class Management" }];
}

const PAGE = { pageNumber: 1, pageSize: 50 };

export default function ClassMembersPage() {
    const { t, i18n } = useTranslation("classroom");
    const { publicId } = useParams();

    const { data: cls, isLoading: clsLoading } = useTeacherClass(publicId);
    const pending = useClassMembers(publicId, { ...PAGE, status: "Pending" });
    const approved = useClassMembers(publicId, { ...PAGE, status: "Approved" });

    const approve = useApproveMember();
    const reject = useRejectMember();
    const kick = useKickMember();

    const [rejecting, setRejecting] = useState<ClassMember | null>(null);
    const [kicking, setKicking] = useState<ClassMember | null>(null);

    const formatDate = (iso: string) =>
        new Intl.DateTimeFormat(i18n.language, { dateStyle: "medium" }).format(new Date(iso));

    const onApprove = (m: ClassMember) =>
        approve.mutate(
            { publicId: publicId!, membershipId: m.publicId },
            {
                onSuccess: () => toast.success(t("members.toast.approved")),
                onError: (e) => toast.error(e instanceof ApiError ? e.message : t("toast.error")),
            },
        );

    const confirmReject = () => {
        if (!rejecting) return;
        reject.mutate(
            {
                publicId: publicId!,
                membershipId: rejecting.publicId,
                payload: { rejectionReason: null },
            },
            {
                onSuccess: () => {
                    toast.success(t("members.toast.rejected"));
                    setRejecting(null);
                },
                onError: (e) => toast.error(e instanceof ApiError ? e.message : t("toast.error")),
            },
        );
    };

    const confirmKick = () => {
        if (!kicking) return;
        kick.mutate(
            { publicId: publicId!, membershipId: kicking.publicId },
            {
                onSuccess: () => {
                    toast.success(t("members.toast.removed"));
                    setKicking(null);
                },
                onError: (e) => toast.error(e instanceof ApiError ? e.message : t("toast.error")),
            },
        );
    };

    const pendingItems = pending.data?.items ?? [];
    const approvedItems = approved.data?.items ?? [];

    return (
        <div className="space-y-6">
            <Link
                to="/teacher/classrooms"
                className="text-muted-foreground hover:text-foreground inline-flex items-center gap-1.5 text-sm"
            >
                <ArrowLeft className="size-4" />
                {t("actions.back")}
            </Link>

            <div>
                <h2 className="text-xl font-semibold">
                    {clsLoading ? <Skeleton className="h-7 w-48" /> : cls?.name}
                </h2>
                <p className="text-muted-foreground text-sm">{t("members.subtitle")}</p>
            </div>

            {cls ? <InviteCodeCard publicId={cls.publicId} inviteCode={cls.inviteCode} /> : null}

            {/* Pending requests */}
            <Card>
                <CardHeader>
                    <CardTitle>{t("members.pendingTitle")}</CardTitle>
                    <CardDescription>{t("members.pendingSubtitle")}</CardDescription>
                </CardHeader>
                <CardContent className="space-y-2">
                    {pending.isLoading ? (
                        <Skeleton className="h-12 w-full" />
                    ) : pendingItems.length === 0 ? (
                        <p className="text-muted-foreground py-4 text-center text-sm">
                            {t("members.noPending")}
                        </p>
                    ) : (
                        pendingItems.map((m) => (
                            <div
                                key={m.publicId}
                                className="flex flex-col gap-3 rounded-lg border p-3 sm:flex-row sm:items-center sm:justify-between"
                            >
                                <div>
                                    <p className="font-medium">{m.studentName}</p>
                                    <p className="text-muted-foreground text-sm">
                                        {m.studentEmail} · {formatDate(m.joinedAt)}
                                    </p>
                                </div>
                                <div className="flex gap-2">
                                    <Button
                                        size="sm"
                                        onClick={() => onApprove(m)}
                                        disabled={approve.isPending}
                                    >
                                        <Check className="size-4" />
                                        {t("members.actions.approve")}
                                    </Button>
                                    <Button
                                        size="sm"
                                        variant="outline"
                                        onClick={() => setRejecting(m)}
                                    >
                                        <X className="size-4" />
                                        {t("members.actions.reject")}
                                    </Button>
                                </div>
                            </div>
                        ))
                    )}
                </CardContent>
            </Card>

            {/* Approved roster */}
            <Card>
                <CardHeader>
                    <CardTitle>{t("members.approvedTitle")}</CardTitle>
                    <CardDescription>{t("members.approvedSubtitle")}</CardDescription>
                </CardHeader>
                <CardContent className="space-y-2">
                    {approved.isLoading ? (
                        <Skeleton className="h-12 w-full" />
                    ) : approvedItems.length === 0 ? (
                        <p className="text-muted-foreground py-4 text-center text-sm">
                            {t("members.noApproved")}
                        </p>
                    ) : (
                        approvedItems.map((m) => (
                            <div
                                key={m.publicId}
                                className="flex flex-col gap-3 rounded-lg border p-3 sm:flex-row sm:items-center sm:justify-between"
                            >
                                <div>
                                    <p className="font-medium">{m.studentName}</p>
                                    <p className="text-muted-foreground text-sm">
                                        {m.studentEmail}
                                    </p>
                                </div>
                                <Button size="sm" variant="outline" onClick={() => setKicking(m)}>
                                    <UserMinus className="size-4" />
                                    {t("members.actions.remove")}
                                </Button>
                            </div>
                        ))
                    )}
                </CardContent>
            </Card>

            <ConfirmDialog
                open={rejecting !== null}
                onOpenChange={(open) => !open && setRejecting(null)}
                title={t("members.rejectConfirm.title")}
                description={t("members.rejectConfirm.description", {
                    name: rejecting?.studentName ?? "",
                })}
                confirmLabel={t("members.actions.reject")}
                onConfirm={confirmReject}
                isPending={reject.isPending}
                destructive
            />

            <ConfirmDialog
                open={kicking !== null}
                onOpenChange={(open) => !open && setKicking(null)}
                title={t("members.kickConfirm.title")}
                description={t("members.kickConfirm.description", {
                    name: kicking?.studentName ?? "",
                })}
                confirmLabel={t("members.actions.remove")}
                onConfirm={confirmKick}
                isPending={kick.isPending}
                destructive
            />
        </div>
    );
}
