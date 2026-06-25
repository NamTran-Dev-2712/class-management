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

export type TargetTypeFilter = "all" | string;

const TARGET_TYPES = [
    "User",
    "Class",
    "Question",
    "Exam",
    "Assignment",
    "Attempt",
    "ManualGrade",
    "Report",
    "SystemSetting",
] as const;

interface AuditLogFiltersProps {
    searchTerm: string;
    targetType: TargetTypeFilter;
    onSearchChange: (term: string) => void;
    onTargetTypeChange: (value: TargetTypeFilter) => void;
}

export function AuditLogFilters({
    searchTerm,
    targetType,
    onSearchChange,
    onTargetTypeChange,
}: AuditLogFiltersProps) {
    const { t } = useTranslation("audit");
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
            <Select value={targetType} onValueChange={onTargetTypeChange}>
                <SelectTrigger className="sm:w-[190px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="all">{t("filters.allTargets")}</SelectItem>
                    {TARGET_TYPES.map((type) => (
                        <SelectItem key={type} value={type}>
                            {type}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>
        </div>
    );
}
