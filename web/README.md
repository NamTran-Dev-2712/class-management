# Class Management — Web

Frontend for the Class Management System. **React Router v7** (framework mode, SSR) +
**Tailwind v4** + **shadcn/ui** + **TanStack Query** + **axios** + **react-hook-form/zod** +
**Zustand** + **i18next** (en/vi). Package manager: **Bun**.

## Prerequisites

- [Bun](https://bun.sh) ≥ 1.1
- The backend API running (see `../api`). Set its base URL in `.env`.

## Setup

```bash
bun install
cp .env.example .env   # then edit VITE_API_URL
```

## Scripts

```bash
bun run dev         # start the dev server (HMR) at http://localhost:5173
bun run build       # production build (client + SSR server bundle)
bun run start       # serve the production build (react-router-serve)
bun run typecheck   # react-router typegen + tsc
```

## Structure

```
app/
  components/   ui/ (shadcn) + shared/ reusable components
  config/       languages, feature flags
  features/     vertical slices: <area>/<feature>/{page,form,hook,schema}.tsx
  guards/       loader-based route guards (require-auth, require-role)
  hooks/        cross-cutting React hooks
  lib/          axios, query client/keys, i18n, utils, api-error
  services/     API clients + request/response DTOs
  stores/       Zustand stores
  types/        shared TypeScript types
  routes.ts     route → feature-page map
  root.tsx      document shell + providers
public/locales/<lng>/<ns>.json   translations (en, vi)
```

## Internationalization

- Languages: English (default/fallback) + Vietnamese. Add more in `app/config/languages.ts`.
- Namespaces live in `public/locales/<lng>/<ns>.json` (see `app/lib/i18n.ts`).
- The active language is sent to the API as the `Accept-Language` header so the
  backend localizes its responses. The choice is persisted in the `i18next` cookie.
