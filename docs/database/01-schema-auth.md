# 01 — Schema: Authentication & Foundation

> MVP liên quan: [MVP-1 — Core Foundation](../mvp/MVP-1.md)
> Conventions: [00-conventions.md](./00-conventions.md)

---

## ⚠️ Implementation Note — ASP.NET Core Identity

> Dự án sử dụng **ASP.NET Core Identity** (không phải custom auth từ đầu).
> Quyết định này ảnh hưởng đến cách `users`, `roles`, `user_roles` được quản lý:
>
> | Aspect | Thiết kế gốc (docs) | Thực tế với Identity |
> |---|---|---|
> | PK type | `BIGINT GENERATED ALWAYS AS IDENTITY` | `BIGINT` (Identity dùng `long` key) |
> | Password hash | `{algorithm}:{hash}` format | PBKDF2 do `IPasswordHasher<T>` quản lý |
> | Email uniqueness | Custom partial index | Identity + custom partial index |
> | Lockout | Custom `is_locked`, `locked_at`, `locked_by` | Coexist với Identity's built-in `LockoutEnd`/`LockoutEnabled` |
> | Table name | `users`, `roles`, `user_roles` | Đổi tên từ AspNetUsers/AspNetRoles/AspNetUserRoles qua `ToTable()` |
>
> Ngoài 3 bảng chính, Identity còn tạo thêm 4 bảng phụ trợ (snake_case):
> - `user_claims` — claims của user
> - `user_logins` — external login providers (OAuth future)
> - `user_tokens` — provider tokens (e.g. 2FA)
> - `role_claims` — claims của role
>
> Các bảng phụ trợ này đặt tên snake_case cho đồng bộ nhưng không có trong spec ban đầu.

---

## Tables Overview

| Table | Mô tả | Nguồn |
|---|---|---|
| `users` | Tài khoản người dùng — `ApplicationUser : IdentityUser<long>` | Custom + Identity |
| `roles` | Lookup: Student, Teacher, Admin — `ApplicationRole : IdentityRole<long>` | Custom + Identity |
| `user_roles` | N-N junction — `ApplicationUserRole : IdentityUserRole<long>` | Custom + Identity |
| `refresh_tokens` | Hash của refresh token | Custom entity |
| `password_reset_tokens` | One-time reset token | Custom entity |
| `subjects` | Môn học — Admin quản lý | Custom entity |

---

## Table: `users`

**C# entity:** `ClassManagement.Infrastructure.Identity.ApplicationUser : IdentityUser<long>`
**EF config:** `ApplicationUserConfiguration`
**Purpose:** Core entity đại diện cho mọi người dùng. Kế thừa tất cả cột Identity + bổ sung cột theo docs.

### Columns

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK — internal, không expose API |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | API-facing ID, unique |
| `user_name` | `TEXT` | YES | — | Từ Identity (`UserName`), set = email khi đăng ký |
| `normalized_user_name` | `TEXT` | YES | — | Từ Identity |
| `email` | `CITEXT` | YES | — | Case-insensitive; partial unique index |
| `normalized_email` | `TEXT` | YES | — | Từ Identity |
| `email_confirmed` | `BOOLEAN` | NO | `false` | Từ Identity |
| `password_hash` | `TEXT` | YES | NULL | PBKDF2 do Identity quản lý; NULL cho OAuth (future) |
| `security_stamp` | `TEXT` | YES | — | Từ Identity — invalidate sessions |
| `concurrency_stamp` | `TEXT` | YES | — | Từ Identity — optimistic concurrency |
| `phone_number` | `TEXT` | YES | NULL | Từ Identity |
| `phone_number_confirmed` | `BOOLEAN` | NO | `false` | Từ Identity |
| `two_factor_enabled` | `BOOLEAN` | NO | `false` | Từ Identity |
| `lockout_end` | `TIMESTAMPTZ` | YES | NULL | **Identity built-in lockout** (failed attempts) |
| `lockout_enabled` | `BOOLEAN` | NO | `true` | Từ Identity |
| `access_failed_count` | `INT` | NO | `0` | Từ Identity |
| `display_name` | `TEXT` | NO | — | CHECK length 2–100 |
| `avatar_url` | `TEXT` | YES | NULL | CHECK `^https?://` |
| `bio` | `TEXT` | YES | NULL | CHECK length ≤ 500 |
| `is_locked` | `BOOLEAN` | NO | `false` | **Admin-initiated lockout** (khác Identity lockout) |
| `locked_at` | `TIMESTAMPTZ` | YES | NULL | Thời điểm admin khóa |
| `locked_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL — admin đã khóa |
| `last_login_at` | `TIMESTAMPTZ` | YES | NULL | Cập nhật khi login thành công |
| `created_at` | `TIMESTAMPTZ` | NO | `now()` | Auto-set bởi `AuditableEntityInterceptor` |
| `updated_at` | `TIMESTAMPTZ` | NO | `now()` | Auto-set + DB trigger `trg_users_updated_at` |
| `deleted_at` | `TIMESTAMPTZ` | YES | NULL | Soft delete — NULL = active |
| `created_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL |

### Indexes
```
uq_users_public_id          UNIQUE (public_id)
uq_users_email_active       UNIQUE (email) WHERE deleted_at IS NULL    ← partial unique
EmailIndex                  (normalized_email)                          ← từ Identity
UserNameIndex               UNIQUE (normalized_user_name)              ← từ Identity
idx_users_deleted_at        (deleted_at)
idx_users_is_locked         (is_locked) WHERE is_locked = true
```

### Lockout Strategy (2 cơ chế coexist)
| Cơ chế | Column | Kích hoạt bởi |
|---|---|---|
| Identity auto-lockout | `lockout_end`, `lockout_enabled`, `access_failed_count` | Login thất bại nhiều lần (config: 5 attempts, 15 min) |
| Admin manual-lock | `is_locked`, `locked_at`, `locked_by` | Admin action qua API |

Khi kiểm tra đăng nhập: verify CẢ HAI — `is_locked = true` hoặc `lockout_end > NOW()` đều chặn login.

---

## Table: `roles`

**C# entity:** `ClassManagement.Infrastructure.Identity.ApplicationRole : IdentityRole<long>`
**EF config:** `ApplicationRoleConfiguration`

### Columns

| Column | Type | Notes |
|---|---|---|
| `id` | `BIGINT` | PK |
| `name` | `TEXT` | CHECK IN ('Student', 'Teacher', 'Admin') |
| `normalized_name` | `TEXT` | Từ Identity |
| `concurrency_stamp` | `TEXT` | Từ Identity |
| `description` | `TEXT` | Nullable — mô tả role |
| `created_at` | `TIMESTAMPTZ` | DEFAULT now() |

### Seed Data
```
id=1, name='Student'
id=2, name='Teacher'
id=3, name='Admin'
```

---

## Table: `user_roles`

**C# entity:** `ClassManagement.Infrastructure.Identity.ApplicationUserRole : IdentityUserRole<long>`
**EF config:** `ApplicationUserRoleConfiguration`

### Columns

| Column | Type | Notes |
|---|---|---|
| `user_id` | `BIGINT` | PK (composite), FK → users(id) RESTRICT |
| `role_id` | `BIGINT` | PK (composite), FK → roles(id) RESTRICT |
| `assigned_at` | `TIMESTAMPTZ` | DEFAULT now() |
| `assigned_by` | `BIGINT` | FK → users(id) SET NULL — admin gán role |

### Indexes
```
pk_user_roles              PRIMARY KEY (user_id, role_id)
idx_user_roles_role_id     (role_id)
```

---

## Table: `refresh_tokens`

**C# entity:** `ClassManagement.Domain.Modules.Auth.Entities.RefreshToken : BaseEntity`
**EF config:** `RefreshTokenConfiguration`
**Pattern:** Append-only — không có `updated_at`, không có `created_by`/`updated_by`

### Columns

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK |
| `user_id` | `BIGINT` | NO | — | FK → users(id) CASCADE |
| `token_hash` | `TEXT` | NO | — | SHA-256(raw_token). **KHÔNG bao giờ lưu raw token** |
| `expires_at` | `TIMESTAMPTZ` | NO | — | CHECK > created_at |
| `revoked_at` | `TIMESTAMPTZ` | YES | NULL | NULL = active |
| `replaced_by_token_id` | `BIGINT` | YES | NULL | FK → refresh_tokens(id) SET NULL — rotation chain |
| `ip_address` | `TEXT` | YES | NULL | Audit |
| `user_agent` | `TEXT` | YES | NULL | Device info |
| `created_at` | `TIMESTAMPTZ` | NO | `now()` | |

### Indexes
```
uq_refresh_tokens_hash          UNIQUE (token_hash)
idx_refresh_tokens_user_active  (user_id, revoked_at) WHERE revoked_at IS NULL
idx_refresh_tokens_expires      (expires_at)
```

---

## Table: `password_reset_tokens`

**C# entity:** `ClassManagement.Domain.Modules.Auth.Entities.PasswordResetToken : BaseEntity`
**EF config:** `PasswordResetTokenConfiguration`
**Pattern:** Append-only — TTL 15 phút

### Columns

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `id` | `BIGINT` | NO | PK |
| `user_id` | `BIGINT` | NO | FK → users(id) CASCADE |
| `token_hash` | `TEXT` | NO | SHA-256(raw_token) |
| `expires_at` | `TIMESTAMPTZ` | NO | CHECK > created_at, NOW() + 15 min |
| `used_at` | `TIMESTAMPTZ` | YES | NULL = chưa dùng |
| `created_at` | `TIMESTAMPTZ` | NO | DEFAULT now() |

### Indexes
```
uq_password_reset_hash    UNIQUE (token_hash)
idx_prt_user_active       (user_id) WHERE used_at IS NULL AND expires_at > NOW()
```

---

## Table: `subjects`

**C# entity:** `ClassManagement.Domain.Modules.Catalog.Entities.Subject : AuditableEntity, IHasPublicId, ISoftDeletable`
**EF config:** `SubjectConfiguration`

### Columns

| Column | Type | Nullable | Default | Constraints |
|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE |
| `name` | `TEXT` | NO | — | CHECK length 2–100; partial unique where deleted_at IS NULL |
| `description` | `TEXT` | YES | NULL | CHECK length ≤ 500 |
| `is_active` | `BOOLEAN` | NO | `true` | Inactive = ẩn khỏi dropdown |
| `display_order` | `INT` | NO | `0` | Thứ tự hiển thị |
| `created_at` | `TIMESTAMPTZ` | NO | `now()` | |
| `updated_at` | `TIMESTAMPTZ` | NO | `now()` | Auto via trigger `trg_subjects_updated_at` |
| `deleted_at` | `TIMESTAMPTZ` | YES | NULL | Soft delete |
| `created_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL |

### Indexes
```
uq_subjects_public_id       UNIQUE (public_id)
uq_subjects_name_active     UNIQUE (name) WHERE deleted_at IS NULL
idx_subjects_is_active      (is_active) WHERE is_active = true
idx_subjects_display_order  (display_order)
```

---

## Migration Dependencies

1. `CREATE EXTENSION IF NOT EXISTS citext` — chạy trong `DatabaseSeeder.ApplyDatabaseExtensionsAsync()`
2. `CREATE TABLE roles` (không FK ngoài Identity)
3. `CREATE TABLE users` (self-FK `locked_by` — nullable)
4. `CREATE TABLE user_roles`
5. `CREATE TABLE user_claims`, `user_logins`, `user_tokens`, `role_claims` (Identity phụ trợ)
6. `CREATE TABLE refresh_tokens`
7. `CREATE TABLE password_reset_tokens`
8. `CREATE TABLE subjects`
9. `CREATE FUNCTION set_updated_at()` + triggers cho `users`, `subjects` — trong `DatabaseSeeder`

### Commands để generate migration
```bash
# Từ thư mục api/
dotnet ef migrations add InitDatabase \
  --project src/ClassManagement.Infrastructure \
  --startup-project src/ClassManagement.Api

# Apply migration (cần DB chạy)
dotnet ef database update \
  --project src/ClassManagement.Infrastructure \
  --startup-project src/ClassManagement.Api
```

> **Lưu ý:** `DatabaseSeeder.SeedAsync()` tự động gọi `context.Database.MigrateAsync()` khi app khởi động.
> Triggers và citext extension được tạo/cập nhật idempotent trong `ApplyDatabaseExtensionsAsync()`.

---

## Open Questions (còn lại từ spec gốc)

- [ ] TTL refresh token: 7 ngày (conservative) hay 30 ngày (mobile UX)?
- [ ] Multiple active sessions: có cho phép login nhiều thiết bị cùng lúc không?
- [ ] Email verification flow: add `email_verified_at` column ngay (nullable)?
- [ ] `avatar_url`: external URL hay upload về server (ảnh hưởng validation)?
