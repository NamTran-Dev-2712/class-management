import { Eye, EyeOff } from "lucide-react";
import * as React from "react";
import { useTranslation } from "react-i18next";

import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

/** Password field with a show/hide toggle. Drop-in for `Input type="password"`. */
function PasswordInput({ className, ...props }: React.ComponentProps<"input">) {
    const { t } = useTranslation("common");
    const [visible, setVisible] = React.useState(false);

    return (
        <div className="relative">
            <Input
                type={visible ? "text" : "password"}
                className={cn("pr-10", className)}
                {...props}
            />
            <button
                type="button"
                tabIndex={-1}
                onClick={() => setVisible((v) => !v)}
                aria-label={visible ? t("a11y.hidePassword") : t("a11y.showPassword")}
                className="text-muted-foreground hover:text-foreground absolute inset-y-0 right-0 flex w-10 items-center justify-center"
            >
                {visible ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
            </button>
        </div>
    );
}

export { PasswordInput };
