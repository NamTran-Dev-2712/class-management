import { Trash2 } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { ApiError } from "@/lib/api-error";
import { formatBytes } from "@/lib/format";
import type { MediaListItem } from "@/services/media/dtos/media-dtos";
import {
    useAdminDeleteMedia,
    useAdminMediaList,
    useAdminStorageOverview,
} from "./admin-media.hook";

export function meta() {
    return [{ title: "Storage · Class Management" }];
}

export default function AdminMediaPage() {
    const { t, i18n } = useTranslation("media");
    const [ovPage, setOvPage] = useState(1);
    const [listPage, setListPage] = useState(1);
    const [deleting, setDeleting] = useState<MediaListItem | null>(null);

    const overview = useAdminStorageOverview(ovPage);
    const list = useAdminMediaList({
        pageNumber: listPage,
        pageSize: 20,
        sortBy: "createdAt",
        sortOrder: "desc",
    });
    const remove = useAdminDeleteMedia();

    const confirmDelete = () => {
        if (!deleting) return;
        remove.mutate(deleting.publicId, {
            onSuccess: () => {
                toast.success(t("actions.deleted"));
                setDeleting(null);
            },
            onError: (e) =>
                toast.error(e instanceof ApiError ? e.message : t("errors.uploadFailed")),
        });
    };

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-semibold tracking-tight">{t("admin.title")}</h1>
                <p className="text-muted-foreground text-sm">{t("admin.subtitle")}</p>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle className="text-base">{t("admin.overviewTab")}</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="overflow-x-auto">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>{t("columns.owner")}</TableHead>
                                    <TableHead className="text-right">
                                        {t("columns.assetCount")}
                                    </TableHead>
                                    <TableHead className="text-right">
                                        {t("columns.used")}
                                    </TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {(overview.data?.items ?? []).map((row) => (
                                    <TableRow key={row.ownerPublicId}>
                                        <TableCell className="font-medium">
                                            {row.ownerName}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            {row.assetCount}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            {formatBytes(row.totalBytes)}
                                        </TableCell>
                                    </TableRow>
                                ))}
                                {overview.data && overview.data.items.length === 0 ? (
                                    <TableRow>
                                        <TableCell
                                            colSpan={3}
                                            className="text-muted-foreground text-center"
                                        >
                                            {t("admin.empty")}
                                        </TableCell>
                                    </TableRow>
                                ) : null}
                            </TableBody>
                        </Table>
                    </div>
                    <Pager
                        page={ovPage}
                        totalPages={overview.data?.totalPages ?? 1}
                        hasPrev={overview.data?.hasPreviousPage ?? false}
                        hasNext={overview.data?.hasNextPage ?? false}
                        onChange={setOvPage}
                    />
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle className="text-base">{t("admin.listTab")}</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="overflow-x-auto">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>{t("columns.owner")}</TableHead>
                                    <TableHead>{t("columns.kind")}</TableHead>
                                    <TableHead>{t("columns.contentType")}</TableHead>
                                    <TableHead className="text-right">
                                        {t("columns.size")}
                                    </TableHead>
                                    <TableHead>{t("columns.createdAt")}</TableHead>
                                    <TableHead className="text-right">
                                        {t("columns.actions")}
                                    </TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {(list.data?.items ?? []).map((m) => (
                                    <TableRow key={m.publicId}>
                                        <TableCell className="font-medium">{m.ownerName}</TableCell>
                                        <TableCell>
                                            <Badge variant="secondary">{t(`kind.${m.kind}`)}</Badge>
                                        </TableCell>
                                        <TableCell className="text-muted-foreground text-xs">
                                            {m.contentType}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            {formatBytes(m.byteSize)}
                                        </TableCell>
                                        <TableCell className="text-muted-foreground text-xs">
                                            {new Date(m.createdAt).toLocaleDateString(
                                                i18n.language,
                                            )}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="text-destructive size-7"
                                                onClick={() => setDeleting(m)}
                                                aria-label={t("actions.delete")}
                                            >
                                                <Trash2 className="size-4" />
                                            </Button>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </div>
                    <Pager
                        page={listPage}
                        totalPages={list.data?.totalPages ?? 1}
                        hasPrev={list.data?.hasPreviousPage ?? false}
                        hasNext={list.data?.hasNextPage ?? false}
                        onChange={setListPage}
                    />
                </CardContent>
            </Card>

            <ConfirmDialog
                open={deleting !== null}
                onOpenChange={(o) => (!o ? setDeleting(null) : undefined)}
                title={t("actions.deleteTitle")}
                description={t("actions.deleteConfirm")}
                confirmLabel={t("actions.delete")}
                onConfirm={confirmDelete}
                isPending={remove.isPending}
                destructive
            />
        </div>
    );
}

function Pager({
    page,
    totalPages,
    hasPrev,
    hasNext,
    onChange,
}: {
    page: number;
    totalPages: number;
    hasPrev: boolean;
    hasNext: boolean;
    onChange: (p: number) => void;
}) {
    if (!hasPrev && !hasNext) return null;
    return (
        <div className="mt-3 flex items-center justify-end gap-2">
            <Button
                variant="outline"
                size="sm"
                disabled={!hasPrev}
                onClick={() => onChange(Math.max(1, page - 1))}
            >
                ‹
            </Button>
            <span className="text-muted-foreground text-sm">
                {page} / {totalPages}
            </span>
            <Button
                variant="outline"
                size="sm"
                disabled={!hasNext}
                onClick={() => onChange(page + 1)}
            >
                ›
            </Button>
        </div>
    );
}
