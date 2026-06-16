import {
    Bold,
    Braces,
    Code,
    Eye,
    Heading2,
    Italic,
    Link as LinkIcon,
    List,
    ListOrdered,
    Pencil,
    Quote,
    Strikethrough,
    Underline,
    type LucideIcon,
} from "lucide-react";
import { useEffect, useRef, useState } from "react";

import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { MarkdownContent } from "./markdown-content";

/** Toolbar button identifiers — the consumer supplies a localized label per key. */
export type MarkdownToolbarKey =
    | "bold"
    | "italic"
    | "strikethrough"
    | "underline"
    | "heading"
    | "quote"
    | "bulletList"
    | "orderedList"
    | "code"
    | "codeBlock"
    | "link";

export type MarkdownToolbarLabels = Record<MarkdownToolbarKey, string>;

interface MarkdownEditorProps {
    value: string;
    onChange: (value: string) => void;
    onBlur?: () => void;
    placeholder?: string;
    rows?: number;
    writeLabel: string;
    previewLabel: string;
    emptyLabel: string;
    toolbarLabels: MarkdownToolbarLabels;
}

type Command =
    | { kind: "wrap"; before: string; after: string }
    | { kind: "prefix"; prefix: string }
    | { kind: "link" };

const ICONS: Record<MarkdownToolbarKey, LucideIcon> = {
    bold: Bold,
    italic: Italic,
    strikethrough: Strikethrough,
    underline: Underline,
    heading: Heading2,
    quote: Quote,
    bulletList: List,
    orderedList: ListOrdered,
    code: Code,
    codeBlock: Braces,
    link: LinkIcon,
};

// Toolbar groups (rendered with a Separator between groups), GitHub-PR style.
const GROUPS: MarkdownToolbarKey[][] = [
    ["bold", "italic", "strikethrough", "underline"],
    ["heading", "quote", "bulletList", "orderedList"],
    ["code", "codeBlock", "link"],
];

const COMMANDS: Record<MarkdownToolbarKey, Command> = {
    bold: { kind: "wrap", before: "**", after: "**" },
    italic: { kind: "wrap", before: "_", after: "_" },
    strikethrough: { kind: "wrap", before: "~~", after: "~~" },
    underline: { kind: "wrap", before: "<u>", after: "</u>" },
    code: { kind: "wrap", before: "`", after: "`" },
    codeBlock: { kind: "wrap", before: "```\n", after: "\n```" },
    heading: { kind: "prefix", prefix: "### " },
    quote: { kind: "prefix", prefix: "> " },
    bulletList: { kind: "prefix", prefix: "- " },
    orderedList: { kind: "prefix", prefix: "1. " },
    link: { kind: "link" },
};

/**
 * GitHub-PR-style Markdown editor: a Write textarea with a formatting toolbar (bold/italic/strike/
 * underline/heading/quote/lists/code/link) and a Preview toggle (rendered via the sanitized
 * MarkdownContent). Content stays plain Markdown text; the toolbar just injects syntax around the
 * selection. Extensible — add a button by extending the key/icon/command maps.
 */
export function MarkdownEditor({
    value,
    onChange,
    onBlur,
    placeholder,
    rows = 6,
    writeLabel,
    previewLabel,
    emptyLabel,
    toolbarLabels,
}: MarkdownEditorProps) {
    const [mode, setMode] = useState<"write" | "preview">("write");
    const ref = useRef<HTMLTextAreaElement>(null);
    const pendingSelection = useRef<[number, number] | null>(null);

    // Restore the caret/selection after a toolbar edit re-renders the controlled textarea.
    useEffect(() => {
        if (pendingSelection.current && ref.current) {
            const [start, end] = pendingSelection.current;
            ref.current.focus();
            ref.current.setSelectionRange(start, end);
            pendingSelection.current = null;
        }
    }, [value]);

    const apply = (key: MarkdownToolbarKey) => {
        const ta = ref.current;
        if (!ta) return;
        const start = ta.selectionStart;
        const end = ta.selectionEnd;
        const selected = value.slice(start, end);
        const command = COMMANDS[key];

        if (command.kind === "wrap") {
            const next =
                value.slice(0, start) +
                command.before +
                selected +
                command.after +
                value.slice(end);
            pendingSelection.current = [start + command.before.length, end + command.before.length];
            onChange(next);
            return;
        }

        if (command.kind === "link") {
            const text = selected || "text";
            const inserted = `[${text}](url)`;
            const next = value.slice(0, start) + inserted + value.slice(end);
            // Select the "url" placeholder so the user can type over it.
            const urlStart = start + inserted.length - 4;
            pendingSelection.current = [urlStart, urlStart + 3];
            onChange(next);
            return;
        }

        // prefix: prepend to every line touched by the selection.
        const lineStart = value.lastIndexOf("\n", start - 1) + 1;
        const block = value.slice(lineStart, end);
        const lineCount = block.split("\n").length;
        const prefixed = block
            .split("\n")
            .map((line) => command.prefix + line)
            .join("\n");
        const next = value.slice(0, lineStart) + prefixed + value.slice(end);
        pendingSelection.current = [lineStart, end + command.prefix.length * lineCount];
        onChange(next);
    };

    return (
        <div className="rounded-md border">
            <div className="bg-muted/40 flex flex-wrap items-center gap-1 border-b p-1">
                <Button
                    type="button"
                    size="sm"
                    variant={mode === "write" ? "secondary" : "ghost"}
                    className="h-7 gap-1.5 px-2"
                    onClick={() => setMode("write")}
                >
                    <Pencil className="size-3.5" />
                    {writeLabel}
                </Button>
                <Button
                    type="button"
                    size="sm"
                    variant={mode === "preview" ? "secondary" : "ghost"}
                    className="h-7 gap-1.5 px-2"
                    onClick={() => setMode("preview")}
                >
                    <Eye className="size-3.5" />
                    {previewLabel}
                </Button>

                {mode === "write" ? (
                    <>
                        <Separator orientation="vertical" className="mx-1 h-5" />
                        {GROUPS.map((group, gi) => (
                            <div key={gi} className="flex items-center gap-0.5">
                                {gi > 0 ? (
                                    <Separator orientation="vertical" className="mx-1 h-5" />
                                ) : null}
                                {group.map((key) => {
                                    const Icon = ICONS[key];
                                    return (
                                        <Button
                                            key={key}
                                            type="button"
                                            size="icon"
                                            variant="ghost"
                                            className="size-7"
                                            title={toolbarLabels[key]}
                                            aria-label={toolbarLabels[key]}
                                            // Prevent the textarea from losing its selection on click.
                                            onMouseDown={(e) => e.preventDefault()}
                                            onClick={() => apply(key)}
                                        >
                                            <Icon className="size-3.5" />
                                        </Button>
                                    );
                                })}
                            </div>
                        ))}
                    </>
                ) : null}
            </div>

            {mode === "write" ? (
                <Textarea
                    ref={ref}
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                    onBlur={onBlur}
                    placeholder={placeholder}
                    rows={rows}
                    className={cn("rounded-none border-0 focus-visible:ring-0")}
                />
            ) : (
                <div className="min-h-[8rem] p-3">
                    {value.trim() ? (
                        <MarkdownContent>{value}</MarkdownContent>
                    ) : (
                        <p className="text-muted-foreground text-sm">{emptyLabel}</p>
                    )}
                </div>
            )}
        </div>
    );
}
