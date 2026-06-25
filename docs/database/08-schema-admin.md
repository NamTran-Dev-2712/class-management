# 08 — Schema: Admin & Moderation

> MVP liên quan: [MVP-7 — Admin & Moderation](../mvp/MVP-7.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [01-schema-auth.md](./01-schema-auth.md) — cần `users`

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `audit_logs` | Append-only log mọi hành động nhạy cảm |
| `reports` | Báo cáo vi phạm từ user, Admin xử lý |
| `notifications` | Thông báo in-app cho user |
| `system_settings` | Key-value config cho hệ thống |

---

## Table: `audit_logs`

**Purpose:** Append-only history của mọi hành động nhạy cảm. Không bao giờ UPDATE, không bao giờ DELETE. Production-grade accountability.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `action` | `TEXT` | NO | — | CHECK (action IN (see below)) | Loại hành động |
| `actor_id` | `BIGINT` | YES | NULL | — | User thực hiện (NULL = system/background job) |
| `actor_role` | `TEXT` | YES | NULL | CHECK (actor_role IN ('Student', 'Teacher', 'Admin', 'System')) | Role tại thời điểm action |
| `target_type` | `TEXT` | YES | NULL | CHECK (target_type IN (see below)) | Loại object bị tác động |
| `target_id` | `BIGINT` | YES | NULL | — | Internal ID của object (không FK constraint — đa hình) |
| `target_public_id` | `UUID` | YES | NULL | — | Public ID của object (để human-readable) |
| `metadata` | `JSONB` | YES | NULL | — | Chi tiết bổ sung (old_value, new_value, reason...) |
| `ip_address` | `TEXT` | YES | NULL | — | IP của actor |
| `user_agent` | `TEXT` | YES | NULL | — | Browser/device info |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | **Không có updated_at** |

### Allowed `action` Values
```
Auth:         'user.login', 'user.logout', 'user.login_failed', 'user.password_changed',
              'user.password_reset_requested', 'user.password_reset_completed'

Admin User:   'user.role_changed', 'user.locked', 'user.unlocked', 'user.deleted'

Question:     'question.created', 'question.updated', 'question.correct_answer_changed',
              'question.deleted', 'question.visibility_changed'

Exam:         'exam.created', 'exam.updated', 'exam.deleted', 'exam.visibility_changed'

Assignment:   'assignment.published', 'assignment.closed', 'assignment.archived'

Attempt:      'attempt.started', 'attempt.submitted', 'attempt.auto_submitted'

Grade:        'grade.manual_graded', 'grade.manual_grade_updated', 'grade.published'

Report:       'report.created', 'report.reviewed', 'report.resolved', 'report.rejected'

Admin:        'admin.system_settings_changed', 'admin.assignment_force_closed'

Payment:      'subscription.created', 'subscription.cancelled', 'subscription.expired',
              'payment.completed', 'payment.failed', 'subscription.manual_set'
```

### Allowed `target_type` Values
```
'User', 'Class', 'Question', 'Exam', 'Assignment', 'Attempt', 'ManualGrade',
'Report', 'Subscription', 'Payment', 'SystemSetting'
```

### `metadata` JSONB Examples
```json
// grade.manual_grade_updated
{
  "old_score": 7.0,
  "new_score": 8.5,
  "attempt_id_public": "uuid-...",
  "question_content_preview": "Phân tích..."
}

// user.role_changed
{
  "old_role": "Student",
  "new_role": "Teacher"
}

// admin.system_settings_changed
{
  "key": "max_classes_per_teacher",
  "old_value": 5,
  "new_value": 10
}
```

### Indexes
```
pk_audit_logs                       PRIMARY KEY (id)
idx_audit_logs_actor_created        (actor_id, created_at DESC)          -- Admin filter by user
idx_audit_logs_action_created       (action, created_at DESC)            -- Filter by action type
idx_audit_logs_target               (target_type, target_id)             -- "Lịch sử của object X"
idx_audit_logs_created_at           (created_at DESC)                    -- Recent activity
```

### Notes
- **Không** có FK constraint trên `actor_id` và `target_id` — đa hình, append-only. Không cần referential integrity ở đây.
- **Không bao giờ** INSERT `password_hash`, raw token, hay payment credentials vào `metadata`
- Candidate cho **table partitioning** theo `created_at` (monthly) khi volume > 10M rows. Chi tiết ở [10-indexing-and-performance.md](./10-indexing-and-performance.md)
- **Retention**: giữ ít nhất 12 tháng hot; archive cold storage sau đó. Xem [11-security.md](./11-security.md)

---

## Table: `reports`

**Purpose:** Báo cáo vi phạm từ user. Admin xem, xử lý, và thực hiện action.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `reporter_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | User gửi báo cáo |
| `target_type` | `TEXT` | NO | — | CHECK (target_type IN ('Question', 'Exam', 'Assignment', 'Class', 'User')) | |
| `target_id` | `BIGINT` | NO | — | — | Internal ID (không FK constraint — đa hình) |
| `target_public_id` | `UUID` | YES | NULL | — | Public ID cho display |
| `reason` | `TEXT` | NO | — | CHECK (reason IN ('inappropriate_content', 'spam', 'copyright', 'incorrect_answer', 'other')) | Loại vi phạm |
| `description` | `TEXT` | YES | NULL | CHECK (length <= 2000) | Chi tiết mô tả |
| `status` | `TEXT` | NO | `'Pending'` | CHECK (status IN ('Pending', 'Reviewing', 'Resolved', 'Rejected')) | |
| `admin_id` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | Admin xử lý |
| `admin_action` | `TEXT` | YES | NULL | CHECK (admin_action IN ('Dismiss', 'WarnUser', 'HideContent', 'DeleteContent', 'BanUser')) | |
| `admin_note` | `TEXT` | YES | NULL | CHECK (length <= 1000) | |
| `resolved_at` | `TIMESTAMPTZ` | YES | NULL | — | |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |

### Unique Constraints
```sql
-- Idempotent: 1 user chỉ report 1 target 1 lần (khi pending hoặc reviewing)
CREATE UNIQUE INDEX uq_reports_idempotent
  ON reports (reporter_id, target_type, target_id)
  WHERE status IN ('Pending', 'Reviewing');
```

### Foreign Keys
| Column | References | On Delete |
|---|---|---|
| `reporter_id` | `users(id)` | RESTRICT |
| `admin_id` | `users(id)` | SET NULL |

### Indexes
```
pk_reports                      PRIMARY KEY (id)
uq_reports_public_id            UNIQUE (public_id)
uq_reports_idempotent           UNIQUE (reporter_id, target_type, target_id) WHERE status IN ('Pending', 'Reviewing')
idx_reports_status              (status, created_at DESC)        -- Admin view pending reports
idx_reports_target              (target_type, target_id)         -- "Reports về object X"
idx_reports_reporter            (reporter_id)                    -- "Reports của user X"
idx_reports_admin               (admin_id) WHERE admin_id IS NOT NULL
```

---

## Table: `notifications`

**Purpose:** Thông báo in-app. Tạo khi có sự kiện hệ thống; user đọc và archive.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `user_id` | `BIGINT` | NO | — | FK → users(id) CASCADE | Người nhận |
| `event_type` | `TEXT` | NO | — | CHECK (event_type IN (see below)) | Loại sự kiện |
| `title` | `TEXT` | NO | — | CHECK (length <= 200) | Tiêu đề thông báo |
| `body` | `TEXT` | YES | NULL | CHECK (length <= 500) | Nội dung ngắn |
| `link` | `TEXT` | YES | NULL | — | URL để navigate (relative path) |
| `payload` | `JSONB` | YES | NULL | — | Extra data liên quan |
| `status` | `TEXT` | NO | `'Unread'` | CHECK (status IN ('Unread', 'Read', 'Archived')) | |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `read_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi user đọc |

### Allowed `event_type` Values
```
'ClassJoinApproved', 'ClassJoinRejected',
'AssignmentCreated', 'AssignmentDueSoon', 'AssignmentClosed',
'GradePublished', 'PendingGradingReminder',
'ReportResolved', 'ReportReceived',
'SystemAnnouncement', 'SubscriptionExpiringSoon', 'SubscriptionExpired',
'PaymentSucceeded', 'PaymentFailed'
```

### Foreign Keys
| Column | References | On Delete |
|---|---|---|
| `user_id` | `users(id)` | CASCADE |

### Indexes
```
pk_notifications                    PRIMARY KEY (id)
uq_notifications_public_id          UNIQUE (public_id)
idx_notifications_user_status       (user_id, status, created_at DESC)   -- HOT: "Thông báo chưa đọc của user X"
idx_notifications_user_unread       (user_id) WHERE status = 'Unread'    -- Count unread badge
idx_notifications_event_type        (event_type, created_at DESC)        -- Debug/analytics
idx_notifications_created_at        (created_at) WHERE status != 'Archived'  -- Cleanup old
```

### Idempotency cho Notification
```sql
-- Tránh gửi 2 lần cùng event cho cùng user (background job retry)
CREATE UNIQUE INDEX uq_notifications_idempotent
  ON notifications (user_id, event_type, (payload->>'reference_id'))
  WHERE created_at > NOW() - INTERVAL '24 hours';
-- Chú ý: partial index theo thời gian, không cần giữ uniqueness vĩnh viễn
```

### Notes
- ON DELETE CASCADE: khi user bị hard delete, notifications cũng xóa
- Candidate cho **table partitioning** theo `created_at` khi > 50M rows
- Cleanup job: archive notifications `Read` hơn 30 ngày; xóa `Archived` hơn 90 ngày
- **Bulk insert** khi Assignment được tạo: INSERT nhiều rows cho tất cả students của class trong 1 transaction

---

## Table: `system_settings`

**Purpose:** Key-value store cho config hệ thống. Admin thay đổi, ảnh hưởng behavior toàn hệ thống.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `key` | `TEXT` | NO | — | UNIQUE | Tên setting |
| `value` | `JSONB` | NO | — | — | Giá trị (số, string, bool, object) |
| `description` | `TEXT` | YES | NULL | — | Mô tả setting cho Admin |
| `value_type` | `TEXT` | NO | `'string'` | CHECK (value_type IN ('string', 'integer', 'boolean', 'json')) | Kiểu dữ liệu để validate |
| `is_public` | `BOOLEAN` | NO | `false` | — | TRUE = frontend có thể đọc qua public API |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | Admin đã cập nhật |

### Indexes
```
pk_system_settings              PRIMARY KEY (id)
uq_system_settings_key          UNIQUE (key)
idx_system_settings_public      (key) WHERE is_public = true
```

### Seed Data (Required Keys)

| Key | Default Value | Description |
|---|---|---|
| `max_classes_per_teacher` | `10` | Free plan: số lớp tối đa per Teacher |
| `max_questions_per_teacher` | `500` | Free plan: số câu hỏi tối đa per Teacher |
| `max_exams_per_teacher` | `50` | Free plan: số đề tối đa per Teacher |
| `max_students_per_class` | `100` | Số học sinh tối đa per Class |
| `max_attempts_per_assignment` | `10` | Hard limit attempts (above assignment config) |
| `invite_code_length` | `8` | Độ dài mã invite (6 hoặc 8) |
| `refresh_token_ttl_days` | `7` | TTL refresh token (ngày) |
| `reset_password_token_ttl_minutes` | `15` | TTL reset password link |
| `assignment_due_soon_hours` | `24` | Giờ trước deadline gửi nhắc nhở |
| `notification_cleanup_days` | `90` | Xóa notification archived sau N ngày |
| `app_name` | `"Class Management"` | Tên app (public) |
| `maintenance_mode` | `false` | Bật maintenance mode |

---

## Relationships Diagram

```
users (*) ─────────────────── (*) audit_logs    [via actor_id — no FK]
users (*) ─────────────────── (*) reports       [via reporter_id]
users (1) ─────────────────── (*) notifications [CASCADE]

audit_logs   (standalone — references qua target_id/target_type without FK)
reports      (standalone — references qua target_id/target_type without FK)
system_settings (standalone)
```

---

## Migration Dependencies

Phụ thuộc:
- `users` (từ 01)
- Không phụ thuộc schema domain nào khác (reports/audit dùng generic target_id)

### Migration order
1. `CREATE TABLE audit_logs`
2. `CREATE TABLE reports`
3. Apply `set_updated_at` trigger cho `reports`
4. `CREATE TABLE notifications`
5. `CREATE TABLE system_settings`
6. Apply `set_updated_at` trigger cho `system_settings`
7. INSERT seed data vào `system_settings`

---

## Open Questions

- [x] **Row-Level Security (RLS)**: **Không bật** ở MVP-7. Quyền đọc notification được enforce ở app layer
  (mọi truy vấn scope theo `ICurrentUserService.UserId`); RLS để dành nếu cần defense-in-depth sau.
- [x] **Notification delivery**: **Polling** (client pull, TanStack Query `refetchInterval`). Schema để mở
  cho real-time sau (có thể thêm `delivered_at`… không phá schema).
- [ ] **Audit log rotation**: pg_partman hay manual partition management? Cần quyết định trước khi volume lớn.
- [x] **system_settings caching**: Cache ở app layer qua `ISystemSettingsService` (Redis `ICacheService`),
  invalidate khi Admin cập nhật (Phase 4 MVP-7).

---

## Implementation notes (MVP-7, as built)

Migration: `20260620063139_create_admin_moderation_tables` (gộp 4 bảng — xem
[12-migrations-strategy.md](./12-migrations-strategy.md)). Entities: `Domain/Modules/Admin/Entities`;
EF config: `Infrastructure/Persistence/Configurations/Admin`; repos (thin, qua `IUnitOfWork`):
`AuditLogs`/`Reports`/`Notifications`/`SystemSettings`.

Khác với thiết kế ở trên (cập nhật bảng cho khớp code):

- **`audit_logs`**: append-only ở **app layer** qua `IAuditLogger` (chưa `REVOKE UPDATE/DELETE` ở DB).
  `metadata` map qua value-converter `Dictionary<string,object?>` ↔ jsonb. `action`/`actor_role`/
  `target_type` là hằng trong `Domain/Modules/Admin/Constants` (CHECK constraint sinh từ đó).
- **`reports.reason`**: lưu **PascalCase** (`InappropriateContent`, `Spam`, `Copyright`,
  `IncorrectAnswer`, `Other`) cho nhất quán enum-as-string toàn hệ thống (doc gốc ghi snake_case).
  Không có `created_by/updated_by` (đúng doc); `updated_at` qua trigger `set_updated_at`.
- **`notifications`**: thêm cột **`reference_id`** (text, ≤100) cho idempotency thay vì biểu thức
  `payload->>'reference_id'`; partial unique `uq_notifications_idempotent (user_id, event_type,
  reference_id) WHERE reference_id IS NOT NULL` (bỏ cửa sổ 24h vì `NOW()` không IMMUTABLE — job tự
  dedup theo thời gian). `payload` vẫn là jsonb cho dữ liệu giàu.
- **`system_settings`**: có thêm `created_at` (từ `BaseEntity`, vô hại). `value` là jsonb (raw JSON
  text). Seed 12 key mặc định qua `SystemSettingSeeder` (idempotent, additive — không ghi đè chỉnh sửa
  của Admin). `value_type` ∈ {string,integer,boolean,json}.
- **Triggers** `trg_reports_updated_at`, `trg_system_settings_updated_at` thêm vào
  `DatabaseSeeder.ApplyTriggersAsync` (idempotent mỗi lần khởi động).
- **Read views**: `vw_audit_logs` (audit ⨝ actor) + `vw_reports` (report ⨝ reporter ⨝ admin) tạo bằng
  raw SQL trong migration; notifications đọc trực tiếp bảng (scope theo user). Xem
  [12-migrations-strategy.md](./12-migrations-strategy.md).
- **`system_settings` là source-of-truth runtime cho resource limits** (MVP-7): `ISystemSettingsService`
  (cache Redis 5 phút, invalidate khi admin sửa) cấp giá trị cho `IClassroomPolicy`/`IQuestionPolicy`/
  `IExamPolicy`; fallback là giá trị appsettings. Default seed cho `max_classes/questions/exams_per_teacher`
  = **0 (unlimited)** để giữ hành vi hiện tại (MVP-8 / admin sẽ đặt cap thực). Thêm key
  `max_reports_per_day` (mặc định 10) cho chống spam report.
- **Moderation actions** (review report): `BanUser` → khoá account + revoke refresh token (BR-7-03, dùng
  `IUserAdminRepository.SetLockAsync`); `HideContent`/`DeleteContent` → soft-delete nội dung (Question/Exam/
  Class/Assignment), `DeleteContent` chặn nếu Question còn trong exam (BR-7-05) → buộc dùng Hide. Không
  thêm cột `is_hidden` (hide = soft-delete, phân biệt qua `admin_action` + audit).

### MVP-7.5 — mọi system setting đều LIVE (consumer cụ thể)
Tất cả 13 key đã được nối dây để có tác dụng thật (qua `ISystemSettingsService`, cache + fallback appsettings):

| Key | Consumer |
|---|---|
| `max_classes_per_teacher` | `IClassroomPolicy` → `CreateClass` |
| `max_questions_per_teacher` | `IQuestionPolicy` → `CreateQuestion`/`DuplicateQuestion` |
| `max_exams_per_teacher` | `IExamPolicy` → `CreateExam`/`DuplicateExam` |
| `max_students_per_class` | `IClassroomPolicy` → `ApproveMember` (chặn khi đủ chỗ) |
| `max_attempts_per_assignment` | `IAssignmentPolicy` → `StartAttempt` (trần cứng = min với `assignment.MaxAttempts`) |
| `invite_code_length` | `IClassroomPolicy` → `CreateClass`/`RegenerateInviteCode` (clamp 6–12) |
| `refresh_token_ttl_days` | `JwtTokenService` (đọc lúc phát token) |
| `reset_password_token_ttl_minutes` | `AuthRepository.ForgotPassword` |
| `assignment_due_soon_hours` | job `assignment-due-soon` |
| `notification_cleanup_days` | job `notification-cleanup` |
| `max_reports_per_day` | `SubmitReport` (daily cap) |
| `app_name` | `GET /api/public/app-config` (anonymous) → FE brand |
| `maintenance_mode` | `GET /api/public/app-config` + **maintenance-gate middleware** (503 cho write của non-admin) |

`app_name`/`maintenance_mode` là `is_public=true`; FE đọc qua `PublicConfigController` (output-cache
`PublicConfigRead`, tag `system-settings` nên evict khi admin sửa). Default cho 3 cap per-teacher vẫn **0
(unlimited)**; admin hạ xuống để áp dụng.
