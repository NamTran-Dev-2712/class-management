import { useCallback, useRef, useState } from "react";

import { MediaUploadDialog, type UploadedMedia } from "./media-upload-dialog";

/**
 * Promise-based media upload dialog (MVP-9). `openUploadDialog()` opens the dialog and resolves with the
 * uploaded media (or null if cancelled) — letting a caller `await` an upload inline (e.g. the Markdown
 * editor's Upload button). Render `dialog` once in the component tree.
 */
export function useMediaUpload() {
    const [open, setOpen] = useState(false);
    const resolverRef = useRef<((media: UploadedMedia | null) => void) | null>(null);

    const settle = useCallback((media: UploadedMedia | null) => {
        setOpen(false);
        resolverRef.current?.(media);
        resolverRef.current = null;
    }, []);

    const openUploadDialog = useCallback(
        () =>
            new Promise<UploadedMedia | null>((resolve) => {
                resolverRef.current = resolve;
                setOpen(true);
            }),
        [],
    );

    const dialog = (
        <MediaUploadDialog
            open={open}
            onClose={() => settle(null)}
            onUploaded={(media) => settle(media)}
        />
    );

    return { dialog, openUploadDialog };
}
