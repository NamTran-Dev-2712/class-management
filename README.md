# Class Management System

> A full-stack platform for online classrooms — question banks, exam building, online testing, automated & manual grading, reporting, moderation, and premium subscriptions with real payment-gateway integration.

<p align="center">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white">
  <img alt="ASP.NET Core" src="https://img.shields.io/badge/ASP.NET%20Core-Clean%20Architecture-512BD4">
  <img alt="React Router" src="https://img.shields.io/badge/React%20Router-v7%20(SSR)-CA4245?logo=reactrouter&logoColor=white">
  <img alt="TypeScript" src="https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white">
  <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white">
  <img alt="Redis" src="https://img.shields.io/badge/Redis-7-DC382D?logo=redis&logoColor=white">
  <img alt="Docker" src="https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white">
  <img alt="License" src="https://img.shields.io/badge/License-MIT-green">
</p>

---

## 📖 Overview

**Class Management System** is an end-to-end online education platform that lets teachers run digital
classrooms and lets students learn, take tests, and track their progress. It automates the full
teaching loop: build a reusable **question bank**, assemble **exams**, hand them out as **assignments**
to a class, let students sit **timed online attempts**, then **auto-grade** objective questions and
**manually grade** writing — with class-level **reports**, CSV exports, audit trails, moderation, and
**premium subscriptions** billed through real payment gateways.

The system is built **feature-by-feature against a product roadmap** (`docs/mvp/`), shipping each MVP
slice end-to-end (backend + frontend + tests + docs) before moving on. It is **bilingual (English /
Vietnamese)** from the database to the UI.

| Role | What they can do |
|---|---|
| 👨‍🏫 **Teacher** | Create classes, build question banks & exams, publish assignments, grade, view reports, subscribe to Pro |
| 👨‍🎓 **Student** | Join classes by invite code, take timed assignments, auto-save answers, review graded results |
| 🛡️ **Admin** | Read-only oversight of all resources, user management, moderation, audit logs, live system settings, dashboards, revenue |

---

## 🧰 Tech Stack

### Backend (`api/`)
| Concern | Technology |
|---|---|
| Runtime | **.NET 10**, ASP.NET Core |
| Architecture | **Clean Architecture** (Domain → Application → Infrastructure → Api) |
| CQRS / Messaging | **MediatR** (commands/queries + pipeline behaviors) |
| Validation | **FluentValidation** |
| Persistence | **EF Core** + **PostgreSQL 16** (snake_case, soft-delete, dual-ID) |
| Caching | **Redis** (domain cache + HTTP output cache) |
| Auth | **ASP.NET Identity** + **JWT** (httpOnly cookie, refresh-token rotation) |
| Background jobs | **Hangfire** (recurring lifecycle sweeps & notifications) |
| Realtime | **SignalR** (push notifications) |
| Email | **Resend** |
| PDF / Export | **QuestPDF** (invoices) + RFC-4180 CSV export |
| API docs | **Scalar** (OpenAPI) at `/scalar/v1` |

### Frontend (`web/`)
| Concern | Technology |
|---|---|
| Framework | **React Router v7** (framework mode, **SSR**) + **TypeScript** |
| Styling | **Tailwind CSS v4** + **shadcn/ui** (new-york) |
| Data fetching | **TanStack Query** + **axios** |
| Forms | **react-hook-form** + **zod** |
| State | **Zustand** |
| i18n | **i18next** (en / vi) |
| Charts | **Recharts** · Drag & drop: **@dnd-kit** |
| Package manager | **Bun** |

### Infrastructure & Tooling
- **Docker Compose** — local PostgreSQL + Redis
- **Testcontainers** + **xUnit** — integration tests against real Postgres + Redis
- **Husky** git hooks — format (CSharpier + Prettier) → typecheck → build → tests
- **CSharpier** (backend) / **Prettier** (frontend) — formatting

---

## 🏛️ Architecture & Design Patterns

The backend follows **Clean Architecture** with a strictly enforced, one-directional dependency rule:

```
Domain  ←  Application  ←  Infrastructure  ←  Api
(no deps)   (→Domain)      (→App, Domain)     (→App, Infra)
```

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, enums, primitives, marker interfaces — zero external dependencies |
| **Application** | CQRS handlers, validators, DTOs, and **all abstractions/interfaces** |
| **Infrastructure** | EF Core, Identity, Redis, JWT, Hangfire, email — **implementations** of Application interfaces |
| **Api** | Controllers, middleware, DI composition root |

> **Boundary rule:** a layer crosses a boundary only through an Application interface. Identity types
> (`ApplicationUser`) never leak out of Infrastructure.

**Key patterns in use:**

- **CQRS via MediatR** — every feature is a vertical slice: a `command`/`query` record + one handler +
  one FluentValidation validator, run through a validation/audit/realtime pipeline.
- **Unit of Work + thin repositories** — handlers depend on `IUnitOfWork`; repositories stage changes,
  the handler commits once.
- **Read-model views** — list endpoints read from purpose-built Postgres views via a reusable
  `BaseGetQueryHandler` (search / filter / sort / paginate in SQL).
- **Dual-ID** — internal `long Id` for fast joins (never exposed) + public `Guid PublicId` for the API.
- **Immutable snapshots** — publishing an assignment freezes its exam into append-only snapshot tables,
  so later edits never affect a live test.
- **Idempotent webhooks** — payment processing applies *exactly once* inside a transaction, keyed by an
  idempotency key, even on gateway re-delivery.
- **Uniform responses & errors** — every endpoint returns `ApiResponse<T>`; handlers throw domain
  exceptions that a global handler maps to HTTP status codes.
- **i18n everywhere** — user-facing strings are message keys resolved per request (`Accept-Language`),
  with parallel `en.json` / `vi.json` resources.

The frontend mirrors this with a **feature-slice** structure (`features/<area>/<feature>/`), an
SSR-authoritative auth boundary (root middleware + route guards), and server-driven data tables.

---

## ✨ Core Features

The platform is shipped as a sequence of end-to-end MVPs:

- 🔐 **Authentication & Authorization** — register / login / forgot + reset password, JWT cookie auth
  with SSR token-refresh middleware and browser-side single-flight refresh; account lockout & failed-login
  protection; role-based access (Admin / Teacher / Student).
- 🏫 **Classroom Management** — teachers run classes with crypto-random invite codes; students join by
  code (approval flow), leave, view members; admins get read-only oversight.
- 📚 **Question Bank** — 5 question types (single/multiple choice, true-false, short/long writing),
  **Markdown** content (sanitized), free-form tags, difficulty, public/private sharing & duplication,
  trigram-indexed keyword search.
- 📝 **Exam Builder** — assemble versioned exam templates with **drag-and-drop** question ordering,
  per-question points, public/private sharing, and student preview.
- 🧪 **Assignments & Online Testing** — publish an exam to a class as an immutable snapshot; students take
  **timed attempts** with shuffled questions, debounced auto-save, and idempotent submit; server-authoritative
  deadlines via a lazy check **and** a Hangfire lifecycle sweep.
- ✅ **Automated & Manual Grading** — objective auto-grading (all-or-nothing for multi-choice); teachers
  score writing answers with feedback; configurable grade-publish policies.
- 📊 **Reports & Exports** — per-assignment statistics, score histograms, effective-score policies
  (highest/latest), and server-generated **CSV export**.
- 🛡️ **Admin & Moderation** — append-only **audit log**, **notifications** (with realtime SignalR push),
  user reports & moderation actions (warn/hide/delete/ban), **live system settings**, and an analytics dashboard.
- 💳 **Premium & Payment** — subscription plans with plan-based resource limits, **MoMo / VnPay** gateway
  integration (plus a fake provider for offline dev), idempotent webhooks, immutable invoices with on-the-fly
  **PDF generation**, and a subscription lifecycle job.

> Each MVP is specified in [`docs/mvp/`](docs/mvp/) and documented schema-by-schema in
> [`docs/database/`](docs/database/).

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0+** | Build & run the API |
| [Bun](https://bun.sh) | latest | Frontend package manager & dev server |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | latest | PostgreSQL + Redis (and integration tests) |

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd fullstack_class_management
```

### 2. Configure environment

Create a `.env` file at the repo root for Docker Compose (PostgreSQL & Redis credentials):

```dotenv
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=class_management
POSTGRES_PORT=5432

REDIS_PASSWORD=redis
REDIS_PORT=6379
```

Then set backend secrets in `api/src/ClassManagement.Api/appsettings.Development.json` (or user-secrets):
connection strings (Postgres/Redis), the JWT signing key, the Resend API key, and—optionally—payment
gateway credentials (`Payment:UseFakeProvider=true` runs the full payment flow offline, no keys needed).

### 3. Start backing services

```bash
docker-compose up -d        # PostgreSQL 16 + Redis 7
```

### 4. Run the backend (`api/`)

```bash
cd api
dotnet run --project src/ClassManagement.Api
```

> Migrations apply automatically on startup via `DatabaseSeeder` (advisory-locked) — it also seeds roles,
> the admin user, and default subjects. No manual `database update` is required.
> API docs are available at **`/scalar/v1`** in Development.

### 5. Run the frontend (`web/`)

```bash
cd web
bun install
bun run dev                 # HMR dev server at http://localhost:5173
```

### Useful commands

**Backend** (run from `api/`):

```bash
dotnet build                                                              # build the solution
dotnet test tests/ClassManagement.IntegrationTests/ClassManagement.IntegrationTests.csproj   # integration tests (needs Docker)
dotnet ef migrations add <verb_noun> \
  --project src/ClassManagement.Infrastructure --startup-project src/ClassManagement.Api      # new migration
dotnet csharpier check .                                                  # formatting gate (run from repo root)
```

**Frontend** (run from `web/`):

```bash
bun run dev            # dev server (HMR)
bun run build          # production build (client + SSR server bundle)
bun run start          # serve the production build
bun run typecheck      # typegen + tsc — the "build green" gate
```

**Git hooks** (one-time, from repo root): `bun install` installs Husky hooks —
*pre-commit* formats + typechecks + builds; *pre-push* additionally runs the integration test suite.

---

## 📂 Project Structure

```
fullstack_class_management/
├── api/                                  # Backend — .NET 10, Clean Architecture
│   ├── src/
│   │   ├── ClassManagement.Domain/        # Entities, enums, primitives (no deps)
│   │   ├── ClassManagement.Application/   # CQRS modules, interfaces, validators, DTOs
│   │   │   └── Modules/                    # Auth, Catalog, Users, Classroom, Questions,
│   │   │                                   #   Exams, Assignments, Admin, Payment
│   │   ├── ClassManagement.Infrastructure/ # EF Core, Identity, Redis, JWT, Hangfire, email
│   │   └── ClassManagement.Api/            # Controllers, middleware, DI composition root
│   │       └── Controllers/{Teacher,Student,Admin}/   # role-scoped surfaces
│   └── tests/
│       └── ClassManagement.IntegrationTests/   # xUnit + Testcontainers
│
├── web/                                   # Frontend — React Router v7 (SSR)
│   └── app/
│       ├── components/{ui,shared,layout}/  # shadcn primitives + reusable components
│       ├── features/{admin,teacher,student,auth,public}/   # vertical feature slices
│       ├── services/<area>/                # API clients + DTOs
│       ├── guards/                         # loader-based route guards
│       ├── lib/                            # axios, query-client, i18n, auth.server
│       └── routes.ts                       # route table
│
├── docs/
│   ├── mvp/                               # product roadmap & per-MVP specs
│   └── database/                          # schema, conventions, migration strategy
│
├── postman/                              # Postman collections per MVP
├── .claude/rules/                        # engineering conventions (AI-tuned reference)
├── docker-compose.yml                    # local PostgreSQL + Redis
└── CLAUDE.md                             # repo guardrails & module status
```

---

## 🗺️ Roadmap

The project is built MVP-by-MVP; **MVP-1 through MVP-8 are complete end-to-end**:

- ✅ MVP-1 — Authentication
- ✅ MVP-2 — Classroom Management
- ✅ MVP-3 — Question Bank
- ✅ MVP-4 — Exam Builder
- ✅ MVP-5 — Assignment & Online Testing
- ✅ MVP-6 — Manual Grading & Reports
- ✅ MVP-7 — Admin, Moderation & Realtime
- ✅ MVP-8 — Premium & Payment

Future directions are tracked in [`docs/mvp/ROADMAP-FUTURE.md`](docs/mvp/ROADMAP-FUTURE.md).

---

## 🧪 Quality Gates

Every change passes the same gates the git hooks enforce:

- `dotnet build` → **0 warnings/errors**
- `dotnet csharpier check .` → clean
- `dotnet test` → all integration tests green (real Postgres + Redis via Testcontainers)
- `bun run typecheck` + `bun run build` → green for the frontend

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

© 2026 Trần Nam
