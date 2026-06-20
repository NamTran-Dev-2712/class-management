# Postman — Class Management API

Importable collection + environment for manually exercising the API alongside the automated
integration tests (`api/tests/ClassManagement.IntegrationTests`).

## Files
- `ClassManagement.postman_collection.json` — all endpoints (Auth, Admin Subjects, Admin Users).
- `ClassManagement-MVP5.postman_collection.json` — end-to-end Assignment & online-testing flow.
- `ClassManagement-MVP6.postman_collection.json` — end-to-end Manual Grading & Reports flow
  (grade a writing answer → report/CSV export → publish grades → student sees score + feedback).
- `ClassManagement.postman_environment.json` — `baseUrl` + admin/teacher/student credentials.

## Feature flows (MVP-5 / MVP-6)
The MVP collections are **self-contained**: each registers its own teacher/student, builds the exam →
class → assignment chain, and captures ids into collection variables as you go. Run the numbered
folders **top to bottom**. Because auth is cookie-based and the jar holds one session per host, the
folders deliberately re-log-in when they switch actor (teacher ↔ student ↔ admin).

**MVP-6 order**: `1. Setup (Teacher)` → `2. Student — Join` → `3. Teacher — Approve member` →
`4. Student — Take & Submit` → `5. Teacher — Grade & Report` → `6. Student — View released result` →
`7. Admin — Report`. The assignment uses the **Manual** grade-publish policy, so the student's score
stays hidden until step 5 publishes it; the writing answer (4/5) + the auto-graded objective (4/4)
make a released total of 8.

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
