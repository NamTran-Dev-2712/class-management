import { Plus, Trash2 } from "lucide-react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Switch } from "@/components/ui/switch";
import type { QuestionOptionInput } from "@/services/question/dtos/commands/question-commands";
import type { QuestionType } from "@/services/question/dtos/queries/question-list";

interface QuestionOptionsFieldProps {
    type: QuestionType;
    value: QuestionOptionInput[];
    onChange: (options: QuestionOptionInput[]) => void;
}

const MAX_OPTIONS = 10;

/**
 * Type-aware answer-options editor:
 * - SingleChoice / TrueFalse → exactly one correct (radio). TrueFalse labels are server-seeded and
 *   not editable here.
 * - MultipleChoice → any number correct (switch per row).
 * - Writing types render nothing (no options).
 */
export function QuestionOptionsField({ type, value, onChange }: QuestionOptionsFieldProps) {
    const { t } = useTranslation("question");

    if (type === "ShortWriting" || type === "LongWriting") return null;

    const isTrueFalse = type === "TrueFalse";
    const isSingle = type === "SingleChoice" || isTrueFalse;

    const setContent = (index: number, content: string) =>
        onChange(value.map((o, i) => (i === index ? { ...o, content } : o)));

    const setCorrectSingle = (index: number) =>
        onChange(value.map((o, i) => ({ ...o, isCorrect: i === index })));

    const toggleCorrectMulti = (index: number, checked: boolean) =>
        onChange(value.map((o, i) => (i === index ? { ...o, isCorrect: checked } : o)));

    const addOption = () => {
        if (value.length >= MAX_OPTIONS) return;
        onChange([...value, { content: "", isCorrect: false }]);
    };

    const removeOption = (index: number) => onChange(value.filter((_, i) => i !== index));

    const correctIndex = value.findIndex((o) => o.isCorrect);

    const rows = value.map((option, index) => (
        <div key={index} className="flex items-center gap-2">
            {isSingle ? (
                <RadioGroup
                    value={correctIndex === index ? String(index) : ""}
                    onValueChange={() => setCorrectSingle(index)}
                    className="flex"
                >
                    <RadioGroupItem value={String(index)} aria-label={t("form.markCorrect")} />
                </RadioGroup>
            ) : (
                <Switch
                    checked={option.isCorrect}
                    onCheckedChange={(c) => toggleCorrectMulti(index, c)}
                    aria-label={t("form.markCorrect")}
                />
            )}

            <Input
                value={option.content}
                onChange={(e) => setContent(index, e.target.value)}
                placeholder={t("form.optionPlaceholder", { index: index + 1 })}
                disabled={isTrueFalse}
                className="flex-1"
            />

            {!isTrueFalse ? (
                <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="size-9 shrink-0"
                    onClick={() => removeOption(index)}
                    disabled={value.length <= 2}
                    aria-label={t("form.removeOption")}
                >
                    <Trash2 className="size-4" />
                </Button>
            ) : null}
        </div>
    ));

    return (
        <div className="space-y-3">
            <div className="space-y-2">{rows}</div>
            <p className="text-muted-foreground text-xs">
                {isSingle ? t("form.markOneCorrect") : t("form.markAnyCorrect")}
            </p>
            {!isTrueFalse ? (
                <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={addOption}
                    disabled={value.length >= MAX_OPTIONS}
                >
                    <Plus className="size-4" />
                    {t("form.addOption")}
                </Button>
            ) : null}
        </div>
    );
}
