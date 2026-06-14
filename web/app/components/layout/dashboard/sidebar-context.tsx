import * as React from "react";

interface SidebarContextValue {
    /** Desktop: collapsed to icon rail. */
    collapsed: boolean;
    toggleCollapsed: () => void;
    /** Mobile: slide-in sheet open state. */
    mobileOpen: boolean;
    setMobileOpen: (open: boolean) => void;
}

const SidebarContext = React.createContext<SidebarContextValue | null>(null);

export function useSidebar(): SidebarContextValue {
    const ctx = React.useContext(SidebarContext);
    if (!ctx) throw new Error("useSidebar must be used within <SidebarProvider>");
    return ctx;
}

export function SidebarProvider({ children }: { children: React.ReactNode }) {
    const [collapsed, setCollapsed] = React.useState(false);
    const [mobileOpen, setMobileOpen] = React.useState(false);

    const value = React.useMemo<SidebarContextValue>(
        () => ({
            collapsed,
            toggleCollapsed: () => setCollapsed((c) => !c),
            mobileOpen,
            setMobileOpen,
        }),
        [collapsed, mobileOpen],
    );

    return <SidebarContext.Provider value={value}>{children}</SidebarContext.Provider>;
}
