# 00 — Database Conventions

> File này là **single source of truth** cho mọi quy ước database trong dự án.
> Tất cả file schema (01–09) đều phải tuân theo conventions ở đây.

---

## 1. Naming Conventions

### Tables
- Dùng `snake_case`, **plural noun**
- Ví dụ: `users`, `classes`, `exam_questions`, `attempt_answers`

### Columns
- Dùng `snake_case`, **singular**
- FK column: `<singular_referenced_table>_id` (vd: `class_id`, `teacher_id`, `question_id`)
- Boolean: prefix `is_` hoặc `has_` (vd: `is_locked`, `is_active`, `is_correct`, `auto_submitted`)
- Timestamp event: suffix `_at` (vd: `created_at`, `submitted_at`, `locked_at`)
- Tránh tên trùng với PostgreSQL keywords: `user` → `users`, `order` → `display_order`

### Indexes
```
-- Regular index
idx_<table>_<col1>[_<col2>...]

-- Unique index
uq_<table>_<col1>[_<col2>...]

-- Partial index
idx_<table>_<col1>_where_<condition_description>

-- GIN / GiST
gin_<table>_<col>
```

### Constraints
```
-- Primary key (auto-named)
pk_<table>

-- Foreign key
fk_<table>_<col>_<referenced_table>

-- Check constraint
chk_<table>_<description>

-- Unique constraint
uq_<table>_<col1>[_<col2>...]
```

### Enum Types
- PostgreSQL native `ENUM` **không dùng** (khó ALTER khi thêm value).
- Thay bằng `TEXT NOT NULL` với `CHECK` constraint.
- Ví dụ:
  ```
  status TEXT NOT NULL CHECK (status IN ('Draft', 'Open', 'Closed', 'Archived'))
  ```
- Document các giá trị hợp lệ trong schema file.

---

## 2. ID Strategy

### Internal Primary Key
```
id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY
```
- **Dùng cho**: internal joins, FK references
- **Không expose ra API**
- Compact (8 bytes), nhanh hơn UUID cho B-tree index

### Public ID (API-facing)
```
public_id UUID NOT NULL DEFAULT gen_random_uuid()
```
- **Dùng cho**: tất cả API request/response
- Unique, non-predictable → chống IDOR enumeration
- `gen_random_uuid()` = UUID v4, built-in PostgreSQL 13+
- Nếu cần sortable: dùng `pg_uuidv7` extension (UUID v7), thêm sau nếu cần
- Add `UNIQUE` constraint hoặc `UNIQUE INDEX` trên `public_id`

### Quy tắc sử dụng
| Context | Dùng |
|---|---|
| FK giữa các bảng (internal) | `id` (BIGINT) |
| API request parameter (path/query) | `public_id` (UUID) |
| API response | `public_id` (UUID) — không bao giờ return `id` |
| Audit log `target_id` | `id` (BIGINT) nội bộ |

---

## 3. Timestamps

### Mọi bảng nghiệp vụ đều có
```
created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
```

### Auto-update `updated_at`
Dùng trigger PostgreSQL:
```sql
-- Trigger function (tạo 1 lần, dùng cho nhiều bảng)
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Áp dụng cho từng bảng
CREATE TRIGGER trg_<table>_updated_at
  BEFORE UPDATE ON <table>
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();
```

### Bảng chỉ có `created_at`
Bảng immutable / append-only (không bao giờ UPDATE):
- `audit_logs`
- `password_reset_tokens` (chỉ set `used_at`, không update)
- `refresh_tokens` (revoke bằng `revoked_at`, không update content)
- `invoices`
- `snapshot_*` tables

### Timezone
- Mọi timestamp store dạng UTC (`TIMESTAMPTZ`)
- Application layer convert sang timezone local khi hiển thị
- Không store timezone trong DB

---

## 4. Soft Delete

### Bảng dùng soft delete
Bảng nghiệp vụ quan trọng, có references từ bảng khác:

| Bảng | Lý do soft delete |
|---|---|
| `users` | Có relationships với classes, attempts, grades... |
| `classes` | Có memberships, assignments |
| `questions` | Có thể đang được dùng trong exams |
| `exams` | Có thể đang được dùng trong assignments |
| `subjects` | Được reference bởi classes, questions |

```
deleted_at TIMESTAMPTZ NULL  -- NULL = active; NOT NULL = soft deleted
```

### Bảng KHÔNG dùng soft delete (hard delete OK)
- `question_options`, `question_tags` — cascade từ parent
- `exam_questions` — junction table
- `class_memberships` — có `Removed`/`Left` status thay thế
- `snapshot_*` — KHÔNG BAO GIỜ DELETE
- `audit_logs` — KHÔNG BAO GIỜ DELETE
- `refresh_tokens` — expire/revoke, không xoá
- `notifications` — archive, không xoá

### Unique constraint với soft delete
Partial unique index để cho phép tái tạo sau khi soft delete:
```sql
-- Email unique chỉ khi chưa bị soft delete
CREATE UNIQUE INDEX uq_users_email_active
  ON users (email)
  WHERE deleted_at IS NULL;

-- Class name unique per teacher chỉ khi active
CREATE UNIQUE INDEX uq_classes_name_owner_active
  ON classes (name, owner_id)
  WHERE deleted_at IS NULL;
```

### Application layer
- Mọi query phải thêm `WHERE deleted_at IS NULL` (hoặc dùng global query filter trong EF Core)
- Không dùng `DELETE FROM` trực tiếp cho các bảng có soft delete (trừ hard delete bởi admin)

---

## 5. Audit Columns

### Bảng nghiệp vụ (không phải log)
```
created_by  BIGINT REFERENCES users(id) ON DELETE SET NULL  -- NULL cho system actions
updated_by  BIGINT REFERENCES users(id) ON DELETE SET NULL  -- NULL cho system actions
```

**Không thêm** `created_by`/`updated_by` vào:
- `audit_logs` (có `actor_id` thay thế)
- `refresh_tokens`, `password_reset_tokens` (system-managed)
- `snapshot_*` tables (system-generated)
- `attempt_answers` (system write)

### Lưu ý
- `created_by` thường = user đang đăng nhập; có thể NULL khi hệ thống tự tạo (background job)
- `updated_by` cập nhật cùng lúc với `updated_at`
- Với `manual_grades`, `graded_by` thay thế `created_by`/`updated_by`

---

## 6. Foreign Key Behaviors

### Quy tắc chung

| Tình huống | ON DELETE | Lý do |
|---|---|---|
| Parent là business data quan trọng | RESTRICT | Không cho xoá khi còn children |
| Parent là lookup/reference | RESTRICT | Bảo toàn data integrity |
| Parent bị soft delete | Giữ FK; app filter | Không cần cascade |
| Junction table entries | CASCADE | OK xoá junction khi parent xoá |
| Nullable FK (optional reference) | SET NULL | Giữ row, clear reference |
| Token/session → user | CASCADE hoặc SET NULL | Xoá user → xoá sessions |

### Mọi FK column đều cần index
PostgreSQL không tự tạo index cho FK. Phải thêm thủ công:
```sql
CREATE INDEX idx_<table>_<fk_column> ON <table> (<fk_column>);
```

---

## 7. Enum Values Reference

Tổng hợp tất cả CHECK constraint values:

### User / Auth
```
-- Không có enum, roles là lookup table
```

### Class
```
classes.status: 'Active', 'Archived'
class_memberships.status: 'Pending', 'Approved', 'Rejected', 'Removed', 'Left'
```

### Question
```
questions.type: 'SingleChoice', 'MultipleChoice', 'TrueFalse', 'ShortWriting', 'LongWriting'
questions.difficulty: 'Easy', 'Medium', 'Hard'
questions.visibility: 'Private', 'Public'
```

### Exam
```
exams.visibility: 'Private', 'Public'
```

### Assignment & Attempt
```
assignments.status: 'Draft', 'Scheduled', 'Open', 'Closed', 'Archived'
assignments.score_policy: 'highest', 'latest'
assignments.grade_publish_policy: 'immediate', 'after_deadline', 'manual'
attempts.status: 'InProgress', 'Submitted', 'AutoGraded', 'NeedManualGrading', 'Graded'
```

### Admin
```
reports.status: 'Pending', 'Reviewing', 'Resolved', 'Rejected'
reports.target_type: 'Question', 'Exam', 'Assignment', 'Class', 'User'
reports.admin_action: 'Dismiss', 'WarnUser', 'HideContent', 'DeleteContent', 'BanUser'
notifications.status: 'Unread', 'Read', 'Archived'
```

### Payment
```
subscriptions.status: 'Active', 'PastDue', 'Cancelled', 'Expired'
subscriptions.payment_type: 'auto', 'manual'
payments.provider: 'momo', 'vnpay'
payments.status: 'Pending', 'Completed', 'Failed', 'Expired'
plans.billing_cycle: 'monthly', 'annual'  -- NULL cho Free plan
```

---

## 8. JSONB Usage Policy

### Khi nên dùng `JSONB`
- Metadata flexible, không cần query theo từng field (vd: `audit_logs.metadata`)
- Payload có cấu trúc thay đổi theo loại event (vd: `notifications.payload`)
- Provider response có thể thay đổi (vd: `payments.provider_metadata`)
- Array IDs cần lưu together (vd: `attempt_answers.selected_option_ids`)

### Khi KHÔNG dùng JSONB
- Data cần query/filter/sort thường xuyên → dùng cột riêng
- Data cần JOIN → dùng cột riêng + FK
- Nullable scalar values → dùng nullable column

### JSONB Index
```sql
-- GIN index cho search trong JSONB
CREATE INDEX gin_<table>_<col> ON <table> USING GIN (<col>);

-- Path index cho truy cập field cụ thể
CREATE INDEX idx_<table>_<col>_<field> ON <table> ((<col>->>'field_name'));
```

---

## 9. citext Extension

Email addresses nên dùng type `CITEXT` (case-insensitive text) thay vì `TEXT`:
```sql
CREATE EXTENSION IF NOT EXISTS citext;

-- Trong bảng users
email CITEXT NOT NULL
```
`CITEXT` tự động so sánh case-insensitive, không cần `LOWER()` trong query.

---

## 10. Multi-tenancy Preparation

Hiện tại: **single-tenant SaaS** (không có `organization_id`).

Tương lai B2B: cần thêm `organization_id BIGINT NOT NULL REFERENCES organizations(id)` vào:
- `users`
- `classes`
- `questions`
- `exams`

**Thiết kế hiện tại phải đảm bảo:**
- Unique constraints là per-natural-key (không global), dễ đổi thành composite với `organization_id`
- Ví dụ: `UNIQUE (email)` → sẽ đổi thành `UNIQUE (organization_id, email)` khi multi-tenant
- Không hard-code global uniqueness assumptions trong business logic

> Chi tiết xem [13-extensibility.md](./13-extensibility.md)

---

## 11. Numeric / Money

- Điểm số: `NUMERIC(8,2)` — đủ cho điểm 0–9999.99 với 2 chữ số thập phân
- Tiền VND: `BIGINT` (VND không có thập phân, lưu đơn vị đồng) hoặc `NUMERIC(15,0)`
- Không dùng `FLOAT` hay `DOUBLE PRECISION` cho tiền/điểm (floating point imprecision)
