import { ChevronLeft, ChevronRight } from "lucide-react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

interface DataTablePaginationProps {
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
    pageSizeOptions?: number[];
}

export function DataTablePagination({
    pageNumber,
    pageSize,
    totalPages,
    totalCount,
    hasPreviousPage,
    hasNextPage,
    onPageChange,
    onPageSizeChange,
    pageSizeOptions = [10, 20, 50],
}: DataTablePaginationProps) {
    const { t } = useTranslation("common");

    return (
        <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
            <p className="text-muted-foreground text-sm">
                {t("table.total", { count: totalCount })}
            </p>

            <div className="flex items-center gap-4">
                <div className="flex items-center gap-2">
                    <span className="text-muted-foreground hidden text-sm sm:inline">
                        {t("table.rowsPerPage")}
                    </span>
                    <Select
                        value={String(pageSize)}
                        onValueChange={(v) => onPageSizeChange(Number(v))}
                    >
                        <SelectTrigger size="sm" className="w-[4.5rem]">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            {pageSizeOptions.map((size) => (
                                <SelectItem key={size} value={String(size)}>
                                    {size}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                <span className="text-sm font-medium">
                    {t("table.pageOf", { page: pageNumber, total: Math.max(totalPages, 1) })}
                </span>

                <div className="flex items-center gap-1">
                    <Button
                        variant="outline"
                        size="icon"
                        className="size-8"
                        disabled={!hasPreviousPage}
                        onClick={() => onPageChange(pageNumber - 1)}
                        aria-label={t("table.previous")}
                    >
                        <ChevronLeft className="size-4" />
                    </Button>
                    <Button
                        variant="outline"
                        size="icon"
                        className="size-8"
                        disabled={!hasNextPage}
                        onClick={() => onPageChange(pageNumber + 1)}
                        aria-label={t("table.next")}
                    >
                        <ChevronRight className="size-4" />
                    </Button>
                </div>
            </div>
        </div>
    );
}
