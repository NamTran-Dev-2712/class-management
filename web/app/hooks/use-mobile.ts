import * as React from "react";

const MOBILE_BREAKPOINT = 768;

/** Tracks whether the viewport is below the `md` breakpoint (client-only). */
export function useIsMobile(): boolean {
    const [isMobile, setIsMobile] = React.useState<boolean>(false);

    React.useEffect(() => {
        const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`);
        const onChange = () => setIsMobile(window.innerWidth < MOBILE_BREAKPOINT);
        onChange();
        mql.addEventListener("change", onChange);
        return () => mql.removeEventListener("change", onChange);
    }, []);

    return isMobile;
}
