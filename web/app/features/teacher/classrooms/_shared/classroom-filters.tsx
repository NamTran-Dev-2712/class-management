import { Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

export type ClassStatusFilter = "all" | "Active" | "Archived";

interface ClassroomFiltersProps {
    searchTerm: string;
    statusValue: ClassStatusFilter;
    onSearchChange: (term: string) => void;
    onStatusChange: (value: ClassStatusFilter) => void;
}

export function ClassroomFilters({
    searchTerm,
    statusValue,
    onSearchChange,
    onStatusChange,
}: ClassroomFiltersProps) {
    const { t } = useTranslation("classroom");
    const [local, setLocal] = useState(searchTerm);

    useEffect(() => setLocal(searchTerm), [searchTerm]);

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
            <Select
                value={statusValue}
                onValueChange={(v) => onStatusChange(v as ClassStatusFilter)}
            >
                <SelectTrigger className="sm:w-[170px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="all">{t("filters.all")}</SelectItem>
                    <SelectItem value="Active">{t("status.active")}</SelectItem>
                    <SelectItem value="Archived">{t("status.archived")}</SelectItem>
                </SelectContent>
            </Select>
        </div>
    );
}
