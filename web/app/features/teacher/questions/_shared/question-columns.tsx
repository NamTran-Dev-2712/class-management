import type { ColumnDef } from "@tanstack/react-table";
import type { TFunction } from "i18next";
import { Copy, Eye, Flag, Globe, Lock, MoreHorizontal, Pencil, Trash2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type {
    QuestionDifficulty,
    QuestionListItem,
} from "@/services/question/dtos/queries/question-list";

export interface QuestionColumnActions {
    onPreview: (q: QuestionListItem) => void;
    onEdit?: (q: QuestionListItem) => void;
    onDuplicate?: (q: QuestionListItem) => void;
    onToggleVisibility?: (q: QuestionListItem) => void;
    onDelete?: (q: QuestionListItem) => void;
    onReport?: (q: QuestionListItem) => void;
}

const difficultyVariant: Record<QuestionDifficulty, "secondary" | "outline" | "destructive"> = {
    Easy: "secondary",
    Medium: "outline",
    Hard: "destructive",
};

interface ColumnOptions extends QuestionColumnActions {
    t: TFunction;
    locale: string;
    /** Show the owner/teacher column (for the public pool + admin views). */
    showOwner?: boolean;
}

export function getQuestionColumns({
    t,
    locale,
    showOwner,
    onPreview,
    onEdit,
    onDuplicate,
    onToggleVisibility,
    onDelete,
    onReport,
}: ColumnOptions): ColumnDef<QuestionListItem>[] {
    const formatDate = (iso: string) =>
        new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(iso));

    const columns: ColumnDef<QuestionListItem>[] = [
        {
            accessorKey: "content",
            header: () => t("columns.content"),
            enableSorting: false,
            meta: { label: t("columns.content") },
            cell: ({ row }) => (
                <button
                    type="button"
                    onClick={() => onPreview(row.original)}
                    className="hover:text-primary line-clamp-2 max-w-md text-left font-medium"
                >
                    {row.original.content}
                </button>
            ),
        },
        {
            accessorKey: "type",
            header: () => t("columns.type"),
            enableSorting: false,
            meta: { label: t("columns.type") },
            cell: ({ row }) => <Badge variant="outline">{t(`types.${row.original.type}`)}</Badge>,
        },
        {
            accessorKey: "difficulty",
            header: () => t("columns.difficulty"),
            enableSorting: true,
            meta: { label: t("columns.difficulty") },
            cell: ({ row }) => (
                <Badge variant={difficultyVariant[row.original.difficulty]}>
                    {t(`difficulty.${row.original.difficulty}`)}
                </Badge>
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
            id: "tags",
            header: () => t("columns.tags"),
            enableSorting: false,
            meta: { label: t("columns.tags") },
            cell: ({ row }) =>
                row.original.tags.length > 0 ? (
                    <div className="flex flex-wrap gap-1">
                        {row.original.tags.slice(0, 3).map((tag) => (
                            <Badge key={tag} variant="secondary" className="text-xs">
                                #{tag}
                            </Badge>
                        ))}
                        {row.original.tags.length > 3 ? (
                            <span className="text-muted-foreground text-xs">
                                +{row.original.tags.length - 3}
                            </span>
                        ) : null}
                    </div>
                ) : (
                    <span className="text-muted-foreground">—</span>
                ),
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

    columns.push({
        id: "actions",
        header: () => <span className="sr-only">{t("columns.actions")}</span>,
        enableSorting: false,
        meta: { hideOnMobile: true },
        cell: ({ row }) => {
            const q = row.original;
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
                            <DropdownMenuItem onSelect={() => onPreview(q)}>
                                <Eye className="size-4" />
                                {t("actions.preview")}
                            </DropdownMenuItem>
                            {onEdit ? (
                                <DropdownMenuItem onSelect={() => onEdit(q)}>
                                    <Pencil className="size-4" />
                                    {t("actions.edit")}
                                </DropdownMenuItem>
                            ) : null}
                            {onDuplicate ? (
                                <DropdownMenuItem onSelect={() => onDuplicate(q)}>
                                    <Copy className="size-4" />
                                    {t("actions.duplicate")}
                                </DropdownMenuItem>
                            ) : null}
                            {onToggleVisibility ? (
                                <DropdownMenuItem onSelect={() => onToggleVisibility(q)}>
                                    {q.visibility === "Public" ? (
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
                            {onReport ? (
                                <DropdownMenuItem onSelect={() => onReport(q)}>
                                    <Flag className="size-4" />
                                    {t("actions.report")}
                                </DropdownMenuItem>
                            ) : null}
                            {onDelete ? (
                                <>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem
                                        variant="destructive"
                                        onSelect={() => onDelete(q)}
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

    return columns;
}
