import ReactMarkdown from "react-markdown";
import rehypeRaw from "rehype-raw";
import rehypeSanitize, { defaultSchema } from "rehype-sanitize";
import remarkGfm from "remark-gfm";

import { cn } from "@/lib/utils";

/**
 * Sanitize schema: extends rehype-sanitize's safe defaults to additionally allow a few inline
 * formatting tags (underline / insert / highlight / sub / sup) that have no native Markdown syntax.
 * Everything else dangerous — `<script>`, `<style>`, `on*` handlers, `javascript:` URLs — is stripped
 * by the default schema, so embedded raw HTML stays XSS-safe.
 */
const sanitizeSchema = {
    ...defaultSchema,
    tagNames: [
        ...(defaultSchema.tagNames ?? []),
        "u",
        "ins",
        "mark",
        "sub",
        "sup",
        // Media (MVP-9) — HTML5 players for uploaded audio/video.
        "audio",
        "video",
        "source",
    ],
    attributes: {
        ...defaultSchema.attributes,
        // Only safe, presentational attributes — no `on*` handlers, no `javascript:` URLs (the default
        // schema's URL sanitization still applies to src/poster).
        audio: ["controls", "src", "preload", "loop", "muted"],
        video: ["controls", "src", "poster", "width", "height", "preload", "loop", "muted"],
        source: ["src", "type"],
    },
};

interface MarkdownContentProps {
    children: string;
    className?: string;
}

/**
 * Renders Markdown. Raw HTML in the source is parsed (`rehype-raw`) then passed through a strict
 * sanitizer (`rehype-sanitize` with {@link sanitizeSchema}) before rendering — so the GitHub-style
 * editor's `<u>` underline works while the output remains XSS-safe. GitHub-flavored Markdown (tables,
 * strikethrough, task lists) is supported via `remark-gfm`.
 */
export function MarkdownContent({ children, className }: MarkdownContentProps) {
    return (
        <div
            className={cn(
                "prose prose-sm dark:prose-invert max-w-none break-words",
                "[&_pre]:bg-muted [&_pre]:overflow-x-auto [&_pre]:rounded-md [&_pre]:p-3",
                "[&_code]:bg-muted [&_code]:rounded [&_code]:px-1 [&_code]:py-0.5 [&_pre_code]:bg-transparent [&_pre_code]:p-0",
                "[&_table]:w-full [&_th]:text-left [&_td]:border [&_th]:border [&_td]:px-2 [&_th]:px-2",
                "[&_a]:text-primary [&_a]:underline [&_img]:max-w-full [&_img]:rounded-md",
                "[&_video]:max-w-full [&_video]:rounded-md [&_audio]:w-full",
                className,
            )}
        >
            <ReactMarkdown
                remarkPlugins={[remarkGfm]}
                rehypePlugins={[rehypeRaw, [rehypeSanitize, sanitizeSchema]]}
            >
                {children}
            </ReactMarkdown>
        </div>
    );
}
