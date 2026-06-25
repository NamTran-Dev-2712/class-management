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
import type { ReportStatus, ReportTargetType } from "@/services/report/dtos/report-dtos";

const STATUSES: ReportStatus[] = ["Pending", "Reviewing", "Resolved", "Rejected"];
const TARGETS: ReportTargetType[] = ["Question", "Exam", "Assignment", "Class", "User"];

interface ReportFiltersProps {
    searchTerm: string;
    status: string;
    targetType: string;
    onSearchChange: (term: string) => void;
    onStatusChange: (value: string) => void;
    onTargetTypeChange: (value: string) => void;
}

export function ReportFilters({
    searchTerm,
    status,
    targetType,
    onSearchChange,
    onStatusChange,
    onTargetTypeChange,
}: ReportFiltersProps) {
    const { t } = useTranslation("report");
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
            <Select value={status} onValueChange={onStatusChange}>
                <SelectTrigger className="sm:w-[160px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="all">{t("filters.allStatuses")}</SelectItem>
                    {STATUSES.map((s) => (
                        <SelectItem key={s} value={s}>
                            {t(`status.${s}`)}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>
            <Select value={targetType} onValueChange={onTargetTypeChange}>
                <SelectTrigger className="sm:w-[160px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="all">{t("filters.allTargets")}</SelectItem>
                    {TARGETS.map((tg) => (
                        <SelectItem key={tg} value={tg}>
                            {tg}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>
        </div>
    );
}
