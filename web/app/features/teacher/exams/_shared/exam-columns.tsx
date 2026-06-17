import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";
import { Copy, Eye, Globe, Lock, MoreHorizontal, Pencil, Trash2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { ExamListItem } from "@/services/exam/dtos/queries/exam-list";

export interface ExamColumnActions {
    onPreview?: (e: ExamListItem) => void;
    onEdit?: (e: ExamListItem) => void;
    onDuplicate?: (e: ExamListItem) => void;
    onToggleVisibility?: (e: ExamListItem) => void;
    onDelete?: (e: ExamListItem) => void;
}

interface ColumnOptions extends ExamColumnActions {
    t: TFunction;
    locale: string;
    /** Show the owner/teacher column (for the public pool + admin views). */
    showOwner?: boolean;
}

export function getExamColumns({
    t,
    locale,
    showOwner,
    onPreview,
    onEdit,
    onDuplicate,
    onToggleVisibility,
    onDelete,
}: ColumnOptions): ColumnDef<ExamListItem>[] {
    const formatDate = (iso: string) =>
        new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(iso));

    const hasActions = onPreview || onEdit || onDuplicate || onToggleVisibility || onDelete;

    const columns: ColumnDef<ExamListItem>[] = [
        {
            accessorKey: "title",
            header: () => t("columns.title"),
            enableSorting: true,
            meta: { label: t("columns.title") },
            cell: ({ row }) =>
                onEdit ? (
                    <button
                        type="button"
                        onClick={() => onEdit(row.original)}
                        className="hover:text-primary line-clamp-2 max-w-md text-left font-medium"
                    >
                        {row.original.title}
                    </button>
                ) : (
                    <span className="line-clamp-2 max-w-md font-medium">{row.original.title}</span>
                ),
        },
        {
            accessorKey: "subjectName",
            header: () => t("columns.subject"),
            enableSorting: false,
            meta: { label: t("columns.subject") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{row.original.subjectName ?? "—"}</span>
            ),
        },
        {
            accessorKey: "totalQuestions",
            header: () => t("columns.questions"),
            enableSorting: true,
            meta: { label: t("columns.questions") },
            cell: ({ row }) => <span className="tabular-nums">{row.original.totalQuestions}</span>,
        },
        {
            accessorKey: "totalPoint",
            header: () => t("columns.totalPoint"),
            enableSorting: true,
            meta: { label: t("columns.totalPoint") },
            cell: ({ row }) => <span className="tabular-nums">{row.original.totalPoint}</span>,
        },
    ];

    if (showOwner) {
        columns.push({
            accessorKey: "teacherName",
            header: () => t("columns.owner"),
            enableSorting: false,
            meta: { label: t("columns.owner") },
            cell: ({ row }) => (
                <span className="text-muted-foreground">{row.original.teacherName}</span>
            ),
        });
    } else {
        columns.push({
            accessorKey: "visibility",
            header: () => t("columns.visibility"),
            enableSorting: false,
            meta: { label: t("columns.visibility") },
            cell: ({ row }) =>
                row.original.visibility === "Public" ? (
                    <Badge variant="default" className="gap-1">
                        <Globe className="size-3" />
                        {t("visibility.Public")}
                    </Badge>
                ) : (
                    <Badge variant="outline" className="gap-1">
                        <Lock className="size-3" />
                        {t("visibility.Private")}
                    </Badge>
                ),
        });
    }

    columns.push({
        accessorKey: "updatedAt",
        header: () => t("columns.updatedAt"),
        enableSorting: true,
        meta: { label: t("columns.updatedAt") },
        cell: ({ row }) => (
            <span className="text-muted-foreground">{formatDate(row.original.updatedAt)}</span>
        ),
    });

    if (hasActions) {
        columns.push({
            id: "actions",
            header: () => <span className="sr-only">{t("columns.actions")}</span>,
            enableSorting: false,
            meta: { hideOnMobile: true },
            cell: ({ row }) => {
                const exam = row.original;
                return (
                    <div className="text-right">
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="icon" className="size-8">
                                    <MoreHorizontal className="size-4" />
                                    <span className="sr-only">{t("columns.actions")}</span>
                                </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                                {onPreview ? (
                                    <DropdownMenuItem onSelect={() => onPreview(exam)}>
                                        <Eye className="size-4" />
                                        {t("actions.preview")}
                                    </DropdownMenuItem>
                                ) : null}
                                {onEdit ? (
                                    <DropdownMenuItem onSelect={() => onEdit(exam)}>
                                        <Pencil className="size-4" />
                                        {t("actions.edit")}
                                    </DropdownMenuItem>
                                ) : null}
                                {onDuplicate ? (
                                    <DropdownMenuItem onSelect={() => onDuplicate(exam)}>
                                        <Copy className="size-4" />
                                        {t("actions.duplicate")}
                                    </DropdownMenuItem>
                                ) : null}
                                {onToggleVisibility ? (
                                    <DropdownMenuItem onSelect={() => onToggleVisibility(exam)}>
                                        {exam.visibility === "Public" ? (
                                            <>
                                                <Lock className="size-4" />
                                                {t("actions.makePrivate")}
                                            </>
                                        ) : (
                                            <>
                                                <Globe className="size-4" />
                                                {t("actions.makePublic")}
                                            </>
                                        )}
                                    </DropdownMenuItem>
                                ) : null}
                                {onDelete ? (
                                    <>
                                        <DropdownMenuSeparator />
                                        <DropdownMenuItem
                                            variant="destructive"
                                            onSelect={() => onDelete(exam)}
                                        >
                                            <Trash2 className="size-4" />
                                            {t("actions.delete")}
                                        </DropdownMenuItem>
                                    </>
                                ) : null}
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </div>
                );
            },
        });
    }

    return columns;
}
