import {
    flexRender,
    getCoreRowModel,
    useReactTable,
    type ColumnDef,
    type OnChangeFn,
    type RowData,
    type SortingState,
} from "@tanstack/react-table";
import { ArrowDown, ArrowUp, ChevronsUpDown } from "lucide-react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";

declare module "@tanstack/react-table" {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    interface ColumnMeta<TData extends RowData, TValue> {
        /** Label shown on the mobile card layout. */
        label?: string;
        /** Hide this column from the mobile card layout (e.g. row actions handled inline). */
        hideOnMobile?: boolean;
    }
}

interface DataTableProps<TData> {
    columns: ColumnDef<TData, unknown>[];
    data: TData[];
    isLoading?: boolean;
    sorting?: SortingState;
    onSortingChange?: OnChangeFn<SortingState>;
}

export function DataTable<TData>({
    columns,
    data,
    isLoading,
    sorting,
    onSortingChange,
}: DataTableProps<TData>) {
    const { t } = useTranslation("common");

    const table = useReactTable({
        data,
        columns,
        getCoreRowModel: getCoreRowModel(),
        manualPagination: true,
        manualSorting: true,
        manualFiltering: true,
        state: { sorting: sorting ?? [] },
        onSortingChange,
    });

    if (isLoading) {
        return (
            <div className="space-y-2">
                {Array.from({ length: 6 }).map((_, i) => (
                    <Skeleton key={i} className="h-12 w-full" />
                ))}
            </div>
        );
    }

    const rows = table.getRowModel().rows;

    if (rows.length === 0) {
        return (
            <div className="text-muted-foreground rounded-md border py-16 text-center text-sm">
                {t("table.noResults")}
            </div>
        );
    }

    return (
        <>
            {/* Desktop: table */}
            <div className="hidden rounded-md border md:block">
                <Table>
                    <TableHeader>
                        {table.getHeaderGroups().map((hg) => (
                            <TableRow key={hg.id}>
                                {hg.headers.map((header) => {
                                    const canSort = header.column.getCanSort();
                                    const sorted = header.column.getIsSorted();
                                    return (
                                        <TableHead key={header.id}>
                                            {header.isPlaceholder ? null : canSort ? (
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    className="-ml-2 h-8 data-[active=true]:text-foreground"
                                                    data-active={sorted !== false}
                                                    onClick={header.column.getToggleSortingHandler()}
                                                >
                                                    {flexRender(
                                                        header.column.columnDef.header,
                                                        header.getContext(),
                                                    )}
                                                    {sorted === "asc" ? (
                                                        <ArrowUp className="size-3.5" />
                                                    ) : sorted === "desc" ? (
                                                        <ArrowDown className="size-3.5" />
                                                    ) : (
                                                        <ChevronsUpDown className="size-3.5 opacity-50" />
                                                    )}
                                                </Button>
                                            ) : (
                                                flexRender(
                                                    header.column.columnDef.header,
                                                    header.getContext(),
                                                )
                                            )}
                                        </TableHead>
                                    );
                                })}
                            </TableRow>
                        ))}
                    </TableHeader>
                    <TableBody>
                        {rows.map((row) => (
                            <TableRow key={row.id}>
                                {row.getVisibleCells().map((cell) => (
                                    <TableCell key={cell.id}>
                                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                    </TableCell>
                                ))}
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </div>

            {/* Mobile: cards */}
            <div className="grid gap-3 md:hidden">
                {rows.map((row) => (
                    <div key={row.id} className="bg-card rounded-lg border p-4">
                        {row.getVisibleCells().map((cell) => {
                            const meta = cell.column.columnDef.meta;
                            if (meta?.hideOnMobile) {
                                return (
                                    <div key={cell.id} className="mt-3 flex justify-end">
                                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                    </div>
                                );
                            }
                            return (
                                <div
                                    key={cell.id}
                                    className={cn(
                                        "flex items-start justify-between gap-3 py-1 text-sm",
                                    )}
                                >
                                    <span className="text-muted-foreground shrink-0">
                                        {meta?.label}
                                    </span>
                                    <span className="text-right">
                                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                    </span>
                                </div>
                            );
                        })}
                    </div>
                ))}
            </div>
        </>
    );
}
