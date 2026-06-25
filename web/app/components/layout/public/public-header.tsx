import { Menu } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, NavLink } from "react-router";

import { LanguageSwitcher } from "@/components/shared/language-switcher";
import { UserMenu } from "@/components/shared/user-menu";
import { Button } from "@/components/ui/button";
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from "@/components/ui/sheet";
import { brandIcon as BrandIcon, marketingNav } from "@/config/nav";
import { useAppName } from "@/hooks/use-app-name";
import { roleHome } from "@/lib/auth";
import { cn } from "@/lib/utils";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

function navLinkClass({ isActive }: { isActive: boolean }) {
    return cn(
        "text-sm font-medium transition-colors",
        isActive ? "text-foreground" : "text-muted-foreground hover:text-foreground",
    );
}

export function PublicHeader({ user }: { user: ProfileResponse | null }) {
    const { t } = useTranslation(["public", "common"]);
    const appName = useAppName();
    const [open, setOpen] = useState(false);

    return (
        <header className="bg-background/95 supports-[backdrop-filter]:bg-background/60 sticky top-0 z-40 border-b backdrop-blur">
            <div className="mx-auto flex h-16 max-w-6xl items-center gap-6 px-4 md:px-6">
                <Link to="/" className="flex items-center gap-2 font-semibold">
                    <BrandIcon className="text-primary size-6" />
                    <span className="truncate">{appName}</span>
                </Link>

                {/* Desktop nav */}
                <nav className="hidden items-center gap-6 md:flex">
                    {marketingNav.map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            end={item.to === "/"}
                            className={navLinkClass}
                        >
                            {t(item.labelKey)}
                        </NavLink>
                    ))}
                </nav>

                <div className="ml-auto flex items-center gap-2">
                    <LanguageSwitcher />

                    {/* Desktop auth cluster */}
                    <div className="hidden items-center gap-2 md:flex">
                        {user ? (
                            <>
                                <Button asChild variant="ghost" size="sm">
                                    <Link to={roleHome(user.roles)}>
                                        {t("userMenu.dashboard", { ns: "common" })}
                                    </Link>
                                </Button>
                                <UserMenu user={user} />
                            </>
                        ) : (
                            <>
                                <Button asChild variant="ghost" size="sm">
                                    <Link to="/login">{t("nav.login", { ns: "common" })}</Link>
                                </Button>
                                <Button asChild size="sm">
                                    <Link to="/register">
                                        {t("nav.register", { ns: "common" })}
                                    </Link>
                                </Button>
                            </>
                        )}
                    </div>

                    {/* Mobile menu */}
                    <Sheet open={open} onOpenChange={setOpen}>
                        <SheetTrigger asChild>
                            <Button
                                variant="ghost"
                                size="icon"
                                className="md:hidden"
                                aria-label={t("nav.home")}
                            >
                                <Menu className="size-5" />
                            </Button>
                        </SheetTrigger>
                        <SheetContent side="right" className="w-72">
                            <SheetHeader>
                                <SheetTitle className="flex items-center gap-2">
                                    <BrandIcon className="text-primary size-5" />
                                    {appName}
                                </SheetTitle>
                                <SheetDescription className="sr-only">{appName}</SheetDescription>
                            </SheetHeader>
                            <nav className="grid gap-1 px-4">
                                {marketingNav.map((item) => (
                                    <NavLink
                                        key={item.to}
                                        to={item.to}
                                        end={item.to === "/"}
                                        onClick={() => setOpen(false)}
                                        className={({ isActive }) =>
                                            cn(
                                                "rounded-md px-3 py-2 text-sm font-medium",
                                                isActive
                                                    ? "bg-accent text-accent-foreground"
                                                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground",
                                            )
                                        }
                                    >
                                        {t(item.labelKey)}
                                    </NavLink>
                                ))}
                            </nav>
                            <div className="mt-auto grid gap-2 p-4">
                                {user ? (
                                    <Button asChild onClick={() => setOpen(false)}>
                                        <Link to={roleHome(user.roles)}>
                                            {t("userMenu.dashboard", { ns: "common" })}
                                        </Link>
                                    </Button>
                                ) : (
                                    <>
                                        <Button
                                            asChild
                                            variant="outline"
                                            onClick={() => setOpen(false)}
                                        >
                                            <Link to="/login">
                                                {t("nav.login", { ns: "common" })}
                                            </Link>
                                        </Button>
                                        <Button asChild onClick={() => setOpen(false)}>
                                            <Link to="/register">
                                                {t("nav.register", { ns: "common" })}
                                            </Link>
                                        </Button>
                                    </>
                                )}
                            </div>
                        </SheetContent>
                    </Sheet>
                </div>
            </div>
        </header>
    );
}
