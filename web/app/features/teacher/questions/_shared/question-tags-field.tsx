import { X } from "lucide-react";
import { useState, type KeyboardEvent } from "react";

import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { MAX_TAGS } from "./question.schema";

interface QuestionTagsFieldProps {
    value: string[];
    onChange: (tags: string[]) => void;
    placeholder?: string;
}

/** Normalizes a raw tag to the backend's slug shape (lowercase, [a-z0-9-], max 50). */
function normalizeTag(raw: string): string {
    return raw
        .trim()
        .toLowerCase()
        .replace(/\s+/g, "-")
        .replace(/[^a-z0-9-]/g, "")
        .replace(/-{2,}/g, "-")
        .replace(/^-+|-+$/g, "")
        .slice(0, 50);
}

/** Free-form tag chips input — type and press Enter (or comma) to add, click ✕ to remove. */
export function QuestionTagsField({ value, onChange, placeholder }: QuestionTagsFieldProps) {
    const [draft, setDraft] = useState("");

    const addTag = () => {
        const slug = normalizeTag(draft);
        setDraft("");
        if (!slug || value.includes(slug) || value.length >= MAX_TAGS) return;
        onChange([...value, slug]);
    };

    const removeTag = (tag: string) => onChange(value.filter((t) => t !== tag));

    const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter" || e.key === ",") {
            e.preventDefault();
            addTag();
        } else if (e.key === "Backspace" && !draft && value.length > 0) {
            removeTag(value[value.length - 1]);
        }
    };

    return (
        <div className="space-y-2">
            <Input
                value={draft}
                onChange={(e) => setDraft(e.target.value)}
                onKeyDown={onKeyDown}
                onBlur={addTag}
                placeholder={placeholder}
                disabled={value.length >= MAX_TAGS}
            />
            {value.length > 0 ? (
                <div className="flex flex-wrap gap-1.5">
                    {value.map((tag) => (
                        <Badge key={tag} variant="secondary" className="gap-1 pr-1">
                            {tag}
                            <button
                                type="button"
                                onClick={() => removeTag(tag)}
                                className="hover:bg-muted-foreground/20 rounded-full p-0.5"
                                aria-label={`Remove ${tag}`}
                            >
                                <X className="size-3" />
                            </button>
                        </Badge>
                    ))}
                </div>
            ) : null}
        </div>
    );
}
