import { LayoutDashboard, LogOut } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useLogout } from "@/hooks/auth/use-logout";
import { roleHome } from "@/lib/auth";
import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

function initialsOf(name: string): string {
    return (
        name
            .split(" ")
            .map((p) => p[0])
            .filter(Boolean)
            .slice(0, 2)
            .join("")
            .toUpperCase() || "U"
    );
}

export function UserMenu({ user }: { user: ProfileResponse }) {
    const { t } = useTranslation("common");
    const logout = useLogout();

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="size-9 rounded-full p-0">
                    <Avatar className="size-9">
                        {user.avatarUrl ? (
                            <AvatarImage src={user.avatarUrl} alt={user.displayName} />
                        ) : null}
                        <AvatarFallback>{initialsOf(user.displayName)}</AvatarFallback>
                    </Avatar>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel className="flex flex-col gap-0.5">
                    <span className="truncate text-sm font-medium">{user.displayName}</span>
                    <span className="text-muted-foreground truncate text-xs font-normal">
                        {user.email}
                    </span>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                    <Link to={roleHome(user.roles)}>
                        <LayoutDashboard className="size-4" />
                        {t("userMenu.dashboard")}
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                    variant="destructive"
                    onSelect={() => logout.mutate()}
                    disabled={logout.isPending}
                >
                    <LogOut className="size-4" />
                    {t("userMenu.logout")}
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
