/// <reference types="vite/client" />

interface ImportMetaEnv {
    /** Base URL of the backend API, e.g. https://localhost:5001/api */
    readonly VITE_API_URL: string;
    readonly VITE_FEATURE_AI_CHAT?: string;
    readonly VITE_FEATURE_PAYMENTS?: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv;
}
