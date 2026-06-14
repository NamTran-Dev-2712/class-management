import { PassThrough } from "node:stream";
import { resolve } from "node:path";

import { createReadableStreamFromReadable } from "@react-router/node";
import { createInstance } from "i18next";
import Backend from "i18next-fs-backend";
import { isbot } from "isbot";
import { renderToPipeableStream } from "react-dom/server";
import { I18nextProvider, initReactI18next } from "react-i18next";
import type { AppLoadContext, EntryContext } from "react-router";
import { ServerRouter } from "react-router";

import { i18nConfig } from "@/lib/i18n";
import { detectLocale } from "@/lib/i18n.server";

const ABORT_DELAY = 5_000;

export default async function handleRequest(
    request: Request,
    responseStatusCode: number,
    responseHeaders: Headers,
    routerContext: EntryContext,
    _loadContext: AppLoadContext,
) {
    const lng = detectLocale(request);

    const i18n = createInstance();
    await i18n
        .use(initReactI18next)
        .use(Backend)
        .init({
            ...i18nConfig,
            lng,
            backend: {
                loadPath: resolve("./public/locales/{{lng}}/{{ns}}.json"),
            },
        });

    return new Promise((resolvePromise, reject) => {
        let shellRendered = false;
        const userAgent = request.headers.get("user-agent");
        const readyEvent = userAgent && isbot(userAgent) ? "onAllReady" : "onShellReady";

        const { pipe, abort } = renderToPipeableStream(
            <I18nextProvider i18n={i18n}>
                <ServerRouter context={routerContext} url={request.url} />
            </I18nextProvider>,
            {
                [readyEvent]() {
                    shellRendered = true;
                    const body = new PassThrough();
                    const stream = createReadableStreamFromReadable(body);

                    responseHeaders.set("Content-Type", "text/html");
                    resolvePromise(
                        new Response(stream, {
                            headers: responseHeaders,
                            status: responseStatusCode,
                        }),
                    );
                    pipe(body);
                },
                onShellError(error: unknown) {
                    reject(error);
                },
                onError(error: unknown) {
                    responseStatusCode = 500;
                    if (shellRendered) console.error(error);
                },
            },
        );

        setTimeout(abort, ABORT_DELAY);
    });
}
