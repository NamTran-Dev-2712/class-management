import i18next from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import Backend from "i18next-http-backend";
import { startTransition, StrictMode } from "react";
import { hydrateRoot } from "react-dom/client";
import { I18nextProvider, initReactI18next } from "react-i18next";
import { HydratedRouter } from "react-router/dom";

import { LANGUAGE_COOKIE } from "@/config/languages";
import { i18nConfig } from "@/lib/i18n";

async function hydrate() {
    await i18next
        .use(initReactI18next)
        .use(Backend)
        .use(LanguageDetector)
        .init({
            ...i18nConfig,
            detection: {
                // htmlTag matches the language the server already rendered with,
                // avoiding a hydration mismatch; cookie persists the user's choice.
                order: ["cookie", "htmlTag", "navigator"],
                caches: ["cookie"],
                lookupCookie: LANGUAGE_COOKIE,
            },
            backend: { loadPath: "/locales/{{lng}}/{{ns}}.json" },
        });

    startTransition(() => {
        hydrateRoot(
            document,
            <StrictMode>
                <I18nextProvider i18n={i18next}>
                    <HydratedRouter />
                </I18nextProvider>
            </StrictMode>,
        );
    });
}

void hydrate();
