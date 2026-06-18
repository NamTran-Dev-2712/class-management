import { useEffect, useRef, useState } from "react";

/**
 * Countdown driven by a server-provided remaining-seconds value. The server is authoritative for the
 * deadline; this only renders a ticking display and fires `onExpire` once when it reaches zero.
 */
export function useCountdown(initialSeconds: number | null | undefined, onExpire?: () => void) {
    const [remaining, setRemaining] = useState<number | null>(initialSeconds ?? null);
    const firedRef = useRef(false);
    const onExpireRef = useRef(onExpire);
    onExpireRef.current = onExpire;

    // Reset when a fresh server value arrives (e.g. after reload).
    useEffect(() => {
        setRemaining(initialSeconds ?? null);
        firedRef.current = (initialSeconds ?? null) === 0;
    }, [initialSeconds]);

    useEffect(() => {
        if (remaining === null) return;
        if (remaining <= 0) {
            if (!firedRef.current) {
                firedRef.current = true;
                onExpireRef.current?.();
            }
            return;
        }
        const id = setInterval(() => setRemaining((r) => (r === null ? null : r - 1)), 1000);
        return () => clearInterval(id);
    }, [remaining]);

    const minutes = remaining === null ? null : Math.floor(remaining / 60);
    const seconds = remaining === null ? null : remaining % 60;
    return { remaining, minutes, seconds };
}
