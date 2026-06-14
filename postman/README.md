# Postman — Class Management API

Importable collection + environment for manually exercising the API alongside the automated
integration tests (`api/tests/ClassManagement.IntegrationTests`).

## Files
- `ClassManagement.postman_collection.json` — all endpoints (Auth, Admin Subjects, Admin Users).
- `ClassManagement.postman_environment.json` — `baseUrl` + admin credentials.

## Import
1. Postman → **Import** → select both JSON files.
2. Select the **Class Management — Local** environment (top-right).
3. Adjust `baseUrl` if your API runs elsewhere. Defaults to `http://localhost:5007/api`
   (the HTTP profile in `api/src/ClassManagement.Api/Properties/launchSettings.json`).

## Auth model (cookies)
The API issues **HttpOnly cookies** (`access_token`, `refresh_token`) on login — there is no bearer
token to copy. Postman's **cookie jar** stores them per-host automatically, so:

1. Run **Auth / Login (Admin)** once. Cookies are now stored for `{{baseUrl}}`'s host.
2. Every following request reuses them. When the access token expires, run **Auth / Refresh Token**
   (the backend rotates both cookies).

> If you change `baseUrl` host, log in again (cookies are scoped per host). Cookies require the
> requests to share the same host as the login call.

## Suggested run order (folder = scenario)
**Admin Subjects**: Login (Admin) → Create Subject → Get By Id → Update → List → Delete.
`Create Subject` saves the new `publicId` into the `subjectId` collection variable for the rest.

**Admin Users**: Login (Admin) → Create User → Get By Id → Update → Lock → Unlock → Delete.
`Create User` generates a unique `newUserEmail` and saves the new `publicId` into `userId`.
The temporary password is **emailed** (never returned), so a real login as the created user requires
reading that email — the automated tests assert this via a fake email queue instead.

**Auth**: Register (Student) creates `selfEmail`; Forgot/Reset Password operate on it (the OTP is
emailed, so Reset will 400 with the placeholder `000000` unless you supply the real code).

## Relationship to automated tests
- Run tests: `cd api && dotnet test tests/ClassManagement.IntegrationTests/ClassManagement.IntegrationTests.csproj`
  (needs Docker — Testcontainers spins up Postgres + Redis). These cover create/list/update/
  lock-unlock/delete + auth lock enforcement with assertions, including the emailed temporary
  password via `FakeEmailQueueService`.
- Use Postman for manual/exploratory checks and latency spot-checks against a running instance
  (`dotnet run --project src/ClassManagement.Api`).
