import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
} from "@/components/ui/sheet";
import { brandIcon as BrandIcon, type NavItem } from "@/config/nav";
import { useAppName } from "@/hooks/use-app-name";
import { SidebarNav } from "./sidebar-nav";
import { useSidebar } from "./sidebar-context";

/** Mobile navigation — a slide-in sheet that replaces the desktop sidebar. */
export function MobileSidebar({ items }: { items: NavItem[] }) {
    const { mobileOpen, setMobileOpen } = useSidebar();
    const appName = useAppName();

    return (
        <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
            <SheetContent side="left" className="w-64 p-0">
                <SheetHeader className="h-14 flex-row items-center gap-2 space-y-0 border-b px-4">
                    <BrandIcon className="text-primary size-6" />
                    <SheetTitle>{appName}</SheetTitle>
                    <SheetDescription className="sr-only">{appName}</SheetDescription>
                </SheetHeader>
                <div className="overflow-y-auto py-2">
                    <SidebarNav items={items} onNavigate={() => setMobileOpen(false)} />
                </div>
            </SheetContent>
        </Sheet>
    );
}
