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

export type ActiveFilter = "all" | "true" | "false";

interface SubjectFiltersProps {
    searchTerm: string;
    activeValue: ActiveFilter;
    onSearchChange: (term: string) => void;
    onActiveChange: (value: ActiveFilter) => void;
}

export function SubjectFilters({
    searchTerm,
    activeValue,
    onSearchChange,
    onActiveChange,
}: SubjectFiltersProps) {
    const { t } = useTranslation("subject");
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
            <Select value={activeValue} onValueChange={(v) => onActiveChange(v as ActiveFilter)}>
                <SelectTrigger className="sm:w-[160px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="all">{t("filters.all")}</SelectItem>
                    <SelectItem value="true">{t("filters.active")}</SelectItem>
                    <SelectItem value="false">{t("filters.inactive")}</SelectItem>
                </SelectContent>
            </Select>
        </div>
    );
}
