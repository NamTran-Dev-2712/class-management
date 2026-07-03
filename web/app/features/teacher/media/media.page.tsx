import { useQueryClient } from "@tanstack/react-query";
import { FileAudio, FileVideo, Trash2, UploadCloud } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { useMediaUpload } from "@/components/shared/media/use-media-upload";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { ApiError } from "@/lib/api-error";
import { formatBytes } from "@/lib/format";
import { queryKeys } from "@/lib/query-keys";
import type { MediaKind, MediaListItem } from "@/services/media/dtos/media-dtos";
import { StorageUsageBar } from "./storage-usage-bar";
import { useDeleteMedia, useMediaList } from "./media.hook";

export function meta() {
    return [{ title: "Media · Class Management" }];
}

const KINDS: (MediaKind | "all")[] = ["all", "Image", "Audio", "Video"];

export default function MediaPage() {
    const { t, i18n } = useTranslation("media");
    const qc = useQueryClient();
    const { dialog, openUploadDialog } = useMediaUpload();
    const remove = useDeleteMedia();

    const [kind, setKind] = useState<MediaKind | "all">("all");
    const [page, setPage] = useState(1);
    const [deleting, setDeleting] = useState<MediaListItem | null>(null);

    const { data, isLoading } = useMediaList({
        pageNumber: page,
        pageSize: 24,
        kind: kind === "all" ? undefined : kind,
        sortBy: "createdAt",
        sortOrder: "desc",
    });

    const handleUpload = async () => {
        const media = await openUploadDialog();
        if (media) qc.invalidateQueries({ queryKey: queryKeys.media.all });
    };

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

    const items = data?.items ?? [];

    return (
        <div className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight">{t("library.title")}</h1>
                    <p className="text-muted-foreground text-sm">{t("library.subtitle")}</p>
                </div>
                <Button onClick={handleUpload} className="gap-1.5">
                    <UploadCloud className="size-4" />
                    {t("library.uploadButton")}
                </Button>
            </div>

            <StorageUsageBar />

            <div className="flex flex-wrap items-center gap-2">
                <Select
                    value={kind}
                    onValueChange={(v) => {
                        setKind(v as MediaKind | "all");
                        setPage(1);
                    }}
                >
                    <SelectTrigger className="w-44">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        {KINDS.map((k) => (
                            <SelectItem key={k} value={k}>
                                {k === "all" ? t("kind.all") : t(`kind.${k}`)}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
            </div>

            {isLoading ? (
                <p className="text-muted-foreground text-sm">…</p>
            ) : items.length === 0 ? (
                <Card>
                    <CardContent className="text-muted-foreground py-12 text-center text-sm">
                        {t("library.empty")}
                    </CardContent>
                </Card>
            ) : (
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
                    {items.map((m) => (
                        <MediaCard
                            key={m.publicId}
                            item={m}
                            locale={i18n.language}
                            onDelete={() => setDeleting(m)}
                        />
                    ))}
                </div>
            )}

            {data && (data.hasPreviousPage || data.hasNextPage) ? (
                <div className="flex items-center justify-end gap-2">
                    <Button
                        variant="outline"
                        size="sm"
                        disabled={!data.hasPreviousPage}
                        onClick={() => setPage((p) => Math.max(1, p - 1))}
                    >
                        ‹
                    </Button>
                    <span className="text-muted-foreground text-sm">
                        {data.pageNumber} / {data.totalPages}
                    </span>
                    <Button
                        variant="outline"
                        size="sm"
                        disabled={!data.hasNextPage}
                        onClick={() => setPage((p) => p + 1)}
                    >
                        ›
                    </Button>
                </div>
            ) : null}

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
            {dialog}
        </div>
    );
}

function MediaCard({
    item,
    locale,
    onDelete,
}: {
    item: MediaListItem;
    locale: string;
    onDelete: () => void;
}) {
    const { t } = useTranslation("media");
    return (
        <Card className="overflow-hidden">
            <div className="bg-muted flex aspect-video items-center justify-center overflow-hidden">
                {item.kind === "Image" ? (
                    <img src={item.url} alt="" className="h-full w-full object-cover" />
                ) : item.kind === "Video" ? (
                    <FileVideo className="text-muted-foreground size-10" />
                ) : (
                    <FileAudio className="text-muted-foreground size-10" />
                )}
            </div>
            <CardContent className="space-y-2 p-3">
                <div className="flex items-center justify-between gap-2">
                    <Badge variant="secondary">{t(`kind.${item.kind}`)}</Badge>
                    <span className="text-muted-foreground text-xs">
                        {formatBytes(item.byteSize)}
                    </span>
                </div>
                {item.kind === "Audio" ? (
                    <audio controls src={item.url} className="w-full" />
                ) : null}
                <div className="flex items-center justify-between gap-2">
                    <span className="text-muted-foreground truncate text-xs">
                        {new Date(item.createdAt).toLocaleDateString(locale)}
                    </span>
                    <Button
                        variant="ghost"
                        size="icon"
                        className="text-destructive size-7"
                        onClick={onDelete}
                        aria-label={t("actions.delete")}
                    >
                        <Trash2 className="size-4" />
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
}
