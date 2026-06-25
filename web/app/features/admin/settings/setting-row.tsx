import { Loader2 } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { ApiError } from "@/lib/api-error";
import type { SystemSetting } from "@/services/admin/admin.service";
import { useUpdateSetting } from "./settings.hook";

// Parse the stored JSON value into the editable form for a given value type.
function toEditable(setting: SystemSetting): string {
    if (setting.valueType === "string") {
        try {
            const parsed = JSON.parse(setting.value);
            return typeof parsed === "string" ? parsed : setting.value;
        } catch {
            return setting.value;
        }
    }
    return setting.value;
}

export function SettingRow({ setting }: { setting: SystemSetting }) {
    const { t } = useTranslation("settings");
    const update = useUpdateSetting();
    const [value, setValue] = useState(() => toEditable(setting));

    const dirty = value !== toEditable(setting);

    const save = () => {
        update.mutate(
            { key: setting.key, value },
            {
                onSuccess: () => toast.success(t("toast.saved")),
                onError: (error: unknown) =>
                    toast.error(error instanceof ApiError ? error.message : t("toast.error")),
            },
        );
    };

    return (
        <div className="flex flex-col gap-3 border-b py-4 last:border-b-0 sm:flex-row sm:items-center">
            <div className="sm:flex-1">
                <p className="text-sm font-medium">
                    {t(`keys.${setting.key}.label`, { defaultValue: setting.key })}
                </p>
                <p className="text-muted-foreground text-xs">
                    {t(`keys.${setting.key}.description`, {
                        defaultValue: setting.description ?? "",
                    })}
                </p>
                <code className="text-muted-foreground/70 text-[10px]">{setting.key}</code>
            </div>
            <div className="flex items-center gap-2 sm:w-80">
                {setting.valueType === "boolean" ? (
                    <Select value={value} onValueChange={setValue}>
                        <SelectTrigger className="flex-1">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="true">{t("boolean.true")}</SelectItem>
                            <SelectItem value="false">{t("boolean.false")}</SelectItem>
                        </SelectContent>
                    </Select>
                ) : (
                    <Input
                        value={value}
                        inputMode={setting.valueType === "integer" ? "numeric" : "text"}
                        onChange={(e) => setValue(e.target.value)}
                        className="flex-1"
                    />
                )}
                <Button size="sm" onClick={save} disabled={!dirty || update.isPending}>
                    {update.isPending && <Loader2 className="size-4 animate-spin" />}
                    {t("save")}
                </Button>
            </div>
        </div>
    );
}
