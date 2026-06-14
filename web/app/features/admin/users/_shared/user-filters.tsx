import { Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

import { Roles } from "@/config/roles";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

export type StatusFilter = "all" | "active" | "locked";
export type RoleFilter = "all" | "Admin" | "Teacher" | "Student";

interface UserFiltersProps {
    searchTerm: string;
    statusValue: StatusFilter;
    roleValue: RoleFilter;
    onSearchChange: (term: string) => void;
    onStatusChange: (value: StatusFilter) => void;
    onRoleChange: (value: RoleFilter) => void;
}

export function UserFilters({
    searchTerm,
    statusValue,
    roleValue,
    onSearchChange,
    onStatusChange,
    onRoleChange,
}: UserFiltersProps) {
    const { t } = useTranslation("user");
    const [local, setLocal] = useState(searchTerm);

    useEffect(() => setLocal(searchTerm), [searchTerm]);

    // Debounce search input → URL/query.
    useEffect(() => {
        const id = setTimeout(() => {
            if (local !== searchTerm) onSearchChange(local);
        }, 350);
        return () => clearTimeout(id);
    }, [local, searchTerm, onSearchChange]);

    return (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <div className="relative sm:max-w-xs sm:flex-1">
                <Search className="text-muted-foreground absolute top-1/2 left-3 size-4 -translate-y-1/2" />
                <Input
                    value={local}
                    onChange={(e) => setLocal(e.target.value)}
                    placeholder={t("filters.search")}
                    className="pl-9"
                />
            </div>
            <Select value={roleValue} onValueChange={(v) => onRoleChange(v as RoleFilter)}>
                <SelectTrigger className="sm:w-[160px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="all">{t("filters.allRoles")}</SelectItem>
                    <SelectItem value={Roles.Admin}>{t("roles.Admin")}</SelectItem>
                    <SelectItem value={Roles.Teacher}>{t("roles.Teacher")}</SelectItem>
                    <SelectItem value={Roles.Student}>{t("roles.Student")}</SelectItem>
                </SelectContent>
            </Select>
            <Select value={statusValue} onValueChange={(v) => onStatusChange(v as StatusFilter)}>
                <SelectTrigger className="sm:w-[160px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="all">{t("filters.allStatuses")}</SelectItem>
                    <SelectItem value="active">{t("filters.active")}</SelectItem>
                    <SelectItem value="locked">{t("filters.locked")}</SelectItem>
                </SelectContent>
            </Select>
        </div>
    );
}
