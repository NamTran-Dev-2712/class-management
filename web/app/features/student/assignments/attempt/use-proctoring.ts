import { useCallback, useEffect, useRef, useState } from "react";

import type { ProctorEventInput } from "@/services/assignment/dtos/commands/assignment-commands";
import type { ProctoringConfig } from "@/services/assignment/dtos/queries/assignment-detail";
import { useRecordProctorEvents } from "../_shared/assignments.hook";

// MVP-10 event-type constants (mirror backend AttemptEventTypes).
const EVENT = {
    TabSwitch: "TabSwitch",
    FocusLoss: "FocusLoss",
    FullscreenExit: "FullscreenExit",
    CopyAttempt: "CopyAttempt",
    PasteAttempt: "PasteAttempt",
    ContextMenu: "ContextMenu",
    DevToolsOpen: "DevToolsOpen",
} as const;

interface UseProctoringArgs {
    attemptId: string;
    config: ProctoringConfig;
    initialViolationCount: number;
    active: boolean;
    onAutoSubmit: () => void;
    onLocked: () => void;
    // Fired after each server-confirmed new violation, with the running count + latest type.
    onViolation: (count: number, eventType: string) => void;
}

interface ProctoringState {
    violationCount: number;
    isFullscreen: boolean;
    /** Enter fullscreen (needs a user gesture; wire to a button + attempt once on mount). */
    requestFullscreen: () => void;
}

const FLUSH_DELAY = 1500;
const MAX_BATCH = 25;
// Collapse the blur + visibilitychange pair a single tab-switch fires into one violation.
const DEDUP_WINDOW = 800;

export function useProctoring({
    attemptId,
    config,
    initialViolationCount,
    active,
    onAutoSubmit,
    onLocked,
    onViolation,
}: UseProctoringArgs): ProctoringState {
    const record = useRecordProctorEvents();
    const [violationCount, setViolationCount] = useState(initialViolationCount);
    const [isFullscreen, setIsFullscreen] = useState(false);

    const queueRef = useRef<ProctorEventInput[]>([]);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const lastFocusEventRef = useRef(0);
    const flushingRef = useRef(false);

    // Keep the latest callbacks/flags in refs so listeners bound once stay current.
    const activeRef = useRef(active);
    activeRef.current = active;
    const cbRef = useRef({ onAutoSubmit, onLocked, onViolation });
    cbRef.current = { onAutoSubmit, onLocked, onViolation };

    const flush = useCallback(async () => {
        if (flushingRef.current || queueRef.current.length === 0) return;
        const batch = queueRef.current.splice(0, MAX_BATCH);
        flushingRef.current = true;
        try {
            const result = await record.mutateAsync({ attemptId, payload: { events: batch } });
            setViolationCount(result.violationCount);
            if (result.submitted) cbRef.current.onAutoSubmit();
            else if (result.locked) cbRef.current.onLocked();
        } catch {
            // Best-effort: re-queue for the next flush so a transient failure doesn't lose events.
            queueRef.current.unshift(...batch);
        } finally {
            flushingRef.current = false;
        }
    }, [attemptId, record]);

    const scheduleFlush = useCallback(() => {
        if (timerRef.current) clearTimeout(timerRef.current);
        if (queueRef.current.length >= MAX_BATCH) {
            void flush();
            return;
        }
        timerRef.current = setTimeout(() => void flush(), FLUSH_DELAY);
    }, [flush]);

    const push = useCallback(
        (eventType: string) => {
            if (!activeRef.current) return;
            queueRef.current.push({ eventType, occurredAt: new Date().toISOString() });
            cbRef.current.onViolation(violationCount + queueRef.current.length, eventType);
            scheduleFlush();
        },
        [scheduleFlush, violationCount],
    );

    const pushRef = useRef(push);
    pushRef.current = push;

    const requestFullscreen = useCallback(() => {
        const el = document.documentElement;
        el.requestFullscreen?.().catch(() => {
            // Blocked (no gesture / unsupported) — degrade gracefully; the exit is still logged.
        });
    }, []);

    // Attach the lockdown listeners once while the attempt is active.
    useEffect(() => {
        if (!active) return;
        const { detectTabSwitch, blockCopyPaste, requireFullscreen } = config;

        const focusEvent = (type: string) => {
            const now = Date.now();
            if (now - lastFocusEventRef.current < DEDUP_WINDOW) return;
            lastFocusEventRef.current = now;
            pushRef.current(type);
        };

        const onVisibility = () => {
            if (document.hidden && detectTabSwitch) focusEvent(EVENT.TabSwitch);
            // A hidden tab (reload/close/switch) is the last reliable moment to persist the queue.
            if (document.hidden) void flush();
        };
        const onBlur = () => {
            if (detectTabSwitch && !document.hidden) focusEvent(EVENT.FocusLoss);
        };
        const onFullscreenChange = () => {
            const fs = !!document.fullscreenElement;
            setIsFullscreen(fs);
            if (!fs && requireFullscreen) pushRef.current(EVENT.FullscreenExit);
        };
        const onCopy = (e: Event) => {
            if (!blockCopyPaste) return;
            e.preventDefault();
            pushRef.current(EVENT.CopyAttempt);
        };
        const onPaste = (e: Event) => {
            if (!blockCopyPaste) return;
            e.preventDefault();
            pushRef.current(EVENT.PasteAttempt);
        };
        const onContextMenu = (e: Event) => {
            if (!blockCopyPaste) return;
            e.preventDefault();
            pushRef.current(EVENT.ContextMenu);
        };
        const onKeyDown = (e: KeyboardEvent) => {
            const devtools =
                e.key === "F12" ||
                ((e.ctrlKey || e.metaKey) &&
                    e.shiftKey &&
                    ["I", "J", "C"].includes(e.key.toUpperCase())) ||
                ((e.ctrlKey || e.metaKey) && e.key.toUpperCase() === "U");
            if (devtools) {
                e.preventDefault();
                pushRef.current(EVENT.DevToolsOpen);
            }
        };
        const onPageHide = () => void flush();

        document.addEventListener("visibilitychange", onVisibility);
        window.addEventListener("blur", onBlur);
        document.addEventListener("fullscreenchange", onFullscreenChange);
        document.addEventListener("copy", onCopy);
        document.addEventListener("cut", onCopy);
        document.addEventListener("paste", onPaste);
        document.addEventListener("contextmenu", onContextMenu);
        document.addEventListener("keydown", onKeyDown);
        window.addEventListener("pagehide", onPageHide);

        setIsFullscreen(!!document.fullscreenElement);

        return () => {
            document.removeEventListener("visibilitychange", onVisibility);
            window.removeEventListener("blur", onBlur);
            document.removeEventListener("fullscreenchange", onFullscreenChange);
            document.removeEventListener("copy", onCopy);
            document.removeEventListener("cut", onCopy);
            document.removeEventListener("paste", onPaste);
            document.removeEventListener("contextmenu", onContextMenu);
            document.removeEventListener("keydown", onKeyDown);
            window.removeEventListener("pagehide", onPageHide);
            if (timerRef.current) clearTimeout(timerRef.current);
        };
    }, [active, config, flush]);

    // Warn the student before they navigate away / reload while the attempt is active.
    useEffect(() => {
        if (!active) return;
        const onBeforeUnload = (e: BeforeUnloadEvent) => {
            e.preventDefault();
            e.returnValue = "";
        };
        window.addEventListener("beforeunload", onBeforeUnload);
        return () => window.removeEventListener("beforeunload", onBeforeUnload);
    }, [active]);

    return { violationCount, isFullscreen, requestFullscreen };
}
