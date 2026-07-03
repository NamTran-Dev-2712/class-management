import { Loader2, UploadCloud } from "lucide-react";
import { useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-error";
import { formatBytes } from "@/lib/format";
import { putToPresignedUrl, teacherMediaService } from "@/services/media/media.service";
import type { MediaAssetDto, MediaKind } from "@/services/media/dtos/media-dtos";

export interface UploadedMedia {
    url: string;
    kind: MediaKind;
    publicId: string;
}

interface MediaUploadDialogProps {
    open: boolean;
    onClose: () => void;
    onUploaded: (media: UploadedMedia) => void;
}

function kindOf(contentType: string): MediaKind | null {
    if (contentType.startsWith("image/")) return "Image";
    if (contentType.startsWith("audio/")) return "Audio";
    if (contentType.startsWith("video/")) return "Video";
    return null;
}

/**
 * Shared media upload dialog (MVP-9): presign → PUT bytes straight to storage → confirm. Reused by the
 * Markdown editor's Upload button, the question attachment manager, and per-option images.
 */
export function MediaUploadDialog({ open, onClose, onUploaded }: MediaUploadDialogProps) {
    const { t } = useTranslation("media");
    const [uploading, setUploading] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);

    const handleFile = async (file: File) => {
        const kind = kindOf(file.type);
        if (!kind) {
            toast.error(t("errors.unsupportedType"));
            return;
        }

        setUploading(true);
        try {
            const presign = await teacherMediaService.presign({
                fileName: file.name,
                contentType: file.type,
                byteSize: file.size,
            });
            await putToPresignedUrl(presign.uploadUrl, file, presign.headers);
            const asset: MediaAssetDto = await teacherMediaService.confirm(presign.mediaPublicId);
            onUploaded({ url: asset.url, kind: asset.kind, publicId: asset.publicId });
            onClose();
        } catch (err) {
            toast.error(err instanceof ApiError ? err.message : t("errors.uploadFailed"));
        } finally {
            setUploading(false);
            if (inputRef.current) inputRef.current.value = "";
        }
    };

    return (
        <Dialog open={open} onOpenChange={(v) => (!v && !uploading ? onClose() : undefined)}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>{t("upload.title")}</DialogTitle>
                    <DialogDescription>{t("upload.description")}</DialogDescription>
                </DialogHeader>

                <label
                    className="border-muted-foreground/30 hover:bg-muted/40 flex cursor-pointer flex-col items-center justify-center gap-2 rounded-md border border-dashed p-8 text-center transition-colors"
                    aria-disabled={uploading}
                >
                    {uploading ? (
                        <Loader2 className="text-muted-foreground size-8 animate-spin" />
                    ) : (
                        <UploadCloud className="text-muted-foreground size-8" />
                    )}
                    <span className="text-sm font-medium">
                        {uploading ? t("upload.uploading") : t("upload.pick")}
                    </span>
                    <span className="text-muted-foreground text-xs">{t("upload.hint")}</span>
                    <input
                        ref={inputRef}
                        type="file"
                        accept="image/*,audio/*,video/*"
                        className="hidden"
                        disabled={uploading}
                        onChange={(e) => {
                            const file = e.target.files?.[0];
                            if (file) void handleFile(file);
                        }}
                    />
                </label>

                <p className="text-muted-foreground text-xs">
                    {t("upload.maxHint", {
                        image: formatBytes(5 * 1024 * 1024),
                        audio: formatBytes(20 * 1024 * 1024),
                        video: formatBytes(100 * 1024 * 1024),
                    })}
                </p>
            </DialogContent>
        </Dialog>
    );
}
