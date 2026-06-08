import { Languages } from "lucide-react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { SUPPORTED_LANGUAGES } from "@/config/languages";

export function LanguageSwitcher() {
    const { t, i18n } = useTranslation("common");
    const current =
        SUPPORTED_LANGUAGES.find((l) => l.code === i18n.resolvedLanguage) ??
        SUPPORTED_LANGUAGES[0];

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="outline" size="sm" aria-label={t("language")}>
                    <Languages className="size-4" />
                    <span>{current.flag}</span>
                    <span className="hidden sm:inline">{current.label}</span>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
                {SUPPORTED_LANGUAGES.map((lang) => (
                    <DropdownMenuItem
                        key={lang.code}
                        onClick={() => void i18n.changeLanguage(lang.code)}
                        data-active={lang.code === current.code}
                    >
                        <span>{lang.flag}</span>
                        <span>{lang.label}</span>
                    </DropdownMenuItem>
                ))}
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
