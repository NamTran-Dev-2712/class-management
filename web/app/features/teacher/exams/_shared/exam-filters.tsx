import { Search, Tag } from "lucide-react";
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
import { VISIBILITIES } from "./exam.schema";

export const FILTER_ALL = "all";

export interface ExamFilterState {
    searchTerm: string;
    subjectId: string;
    visibility: string;
    tag: string;
}

interface SubjectOption {
    publicId: string;
    name: string;
}

interface ExamFiltersProps {
    state: ExamFilterState;
    subjects: SubjectOption[];
    /** Show the Public/Private filter (owner & admin views; hidden for the all-public pool). */
    showVisibility?: boolean;
    onSearchChange: (v: string) => void;
    onSubjectChange: (v: string) => void;
    onVisibilityChange: (v: string) => void;
    onTagChange: (v: string) => void;
}

function useDebounced(value: string, commit: (v: string) => void) {
    const [local, setLocal] = useState(value);
    useEffect(() => setLocal(value), [value]);
    useEffect(() => {
        const id = setTimeout(() => {
            if (local !== value) commit(local);
        }, 350);
        return () => clearTimeout(id);
    }, [local, value, commit]);
    return [local, setLocal] as const;
}

export function ExamFilters({
    state,
    subjects,
    showVisibility,
    onSearchChange,
    onSubjectChange,
    onVisibilityChange,
    onTagChange,
}: ExamFiltersProps) {
    const { t } = useTranslation("exam");
    const [search, setSearch] = useDebounced(state.searchTerm, onSearchChange);
    const [tag, setTag] = useDebounced(state.tag, onTagChange);

    return (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            <div className="relative">
                <Search className="text-muted-foreground absolute top-1/2 left-3 size-4 -translate-y-1/2" />
                <Input
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    placeholder={t("filters.search")}
                    className="pl-9"
                />
            </div>

            <Select value={state.subjectId} onValueChange={onSubjectChange}>
                <SelectTrigger>
                    <SelectValue placeholder={t("filters.subject")} />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value={FILTER_ALL}>{t("filters.allSubjects")}</SelectItem>
                    {subjects.map((s) => (
                        <SelectItem key={s.publicId} value={s.publicId}>
                            {s.name}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>

            {showVisibility ? (
                <Select value={state.visibility} onValueChange={onVisibilityChange}>
                    <SelectTrigger>
                        <SelectValue placeholder={t("filters.visibility")} />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value={FILTER_ALL}>{t("filters.allVisibility")}</SelectItem>
                        {VISIBILITIES.map((v) => (
                            <SelectItem key={v} value={v}>
                                {t(`visibility.${v}`)}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
            ) : null}

            <div className="relative">
                <Tag className="text-muted-foreground absolute top-1/2 left-3 size-4 -translate-y-1/2" />
                <Input
                    value={tag}
                    onChange={(e) => setTag(e.target.value)}
                    placeholder={t("filters.tag")}
                    className="pl-9"
                />
            </div>
        </div>
    );
}
