# 02 — Schema: Classroom Management

> MVP liên quan: [MVP-2 — Classroom Management](../mvp/MVP-2.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [01-schema-auth.md](./01-schema-auth.md) — cần `users`; `subjects` là tùy chọn (nullable FK)

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `classes` | Lớp học do Teacher tạo. Subject là nhãn tùy chọn. |
| `class_memberships` | Trạng thái tham gia của Student trong Class |

---

## Table: `classes`

**Purpose:** Đơn vị tổ chức trung tâm. Mọi Assignment và Attempt đều gắn với một Class. Teacher tạo và quản lý.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | Internal ID |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | API-facing ID |
| `name` | `TEXT` | NO | — | CHECK (length >= 2 AND length <= 200) | Tên lớp học |
| `description` | `TEXT` | YES | NULL | CHECK (length <= 1000) | Mô tả lớp |
| `subject_id` | `BIGINT` | YES | NULL | FK → subjects(id) SET NULL | Nhãn môn học (tùy chọn). NULL = lớp không gắn môn học cụ thể |
| `owner_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | Teacher sở hữu lớp |
| `invite_code` | `TEXT` | NO | — | UNIQUE, CHECK (invite_code ~ '^[A-Z0-9]{6,8}$') | Mã mời join lớp |
| `status` | `TEXT` | NO | `'Active'` | CHECK (status IN ('Active', 'Archived')) | Trạng thái lớp |
| `cover_image_url` | `TEXT` | YES | NULL | CHECK (cover_image_url ~ '^https?://') | Ảnh bìa lớp |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |
| `deleted_at` | `TIMESTAMPTZ` | YES | NULL | — | Soft delete (chỉ Admin cực kỳ hiếm) |
| `created_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | = owner_id trong hầu hết cases |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | |

### Unique Constraints
```sql
-- Tên lớp unique per teacher (chỉ active)
CREATE UNIQUE INDEX uq_classes_name_owner_active
  ON classes (name, owner_id)
  WHERE deleted_at IS NULL;

-- Invite code globally unique (luôn unique, kể cả deleted)
CREATE UNIQUE INDEX uq_classes_invite_code
  ON classes (invite_code);
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `subject_id` | `subjects(id)` | SET NULL | Subject bị xóa → `subject_id` thành NULL, Class vẫn tồn tại bình thường |
| `owner_id` | `users(id)` | RESTRICT | Không xóa Teacher khi còn lớp |
| `created_by` | `users(id)` | SET NULL | Audit column, không phải business |
| `updated_by` | `users(id)` | SET NULL | Audit column |

### Indexes
```
pk_classes                      PRIMARY KEY (id)
uq_classes_public_id            UNIQUE (public_id)
uq_classes_invite_code          UNIQUE (invite_code)
uq_classes_name_owner_active    UNIQUE (name, owner_id) WHERE deleted_at IS NULL
idx_classes_owner               (owner_id)                          -- "Classes của Teacher X"
idx_classes_subject             (subject_id) WHERE subject_id IS NOT NULL   -- Classes có gắn subject (nullable)
idx_classes_status              (status, owner_id)                  -- Filter by status per teacher
idx_classes_deleted             (deleted_at) WHERE deleted_at IS NULL
```

### Notes
- `invite_code`: 6–8 ký tự [A-Z0-9]. Generate ở application layer (không dùng DEFAULT trong DB vì cần ensure unique)
- Regenerate invite code: UPDATE invite_code, ghi audit_log; mã cũ ngay lập tức không còn hiệu lực (no history)
- Soft delete classes: **không cascade** xuống class_memberships hay assignments. App phải query theo `classes.deleted_at IS NULL`
- `subject_id` nullable: Teacher có thể tạo lớp không thuộc môn học nào (lớp tự do, câu lạc bộ, lớp ngoại khoá...). Subject chỉ là nhãn phân loại tùy chọn.
- RESTRICT trên `owner_id`: bắt buộc transfer ownership hoặc archive trước khi xóa Teacher
- Future: `co_teacher_ids BIGINT[]` hoặc bảng `class_teachers` — chưa thêm MVP-2

---

## Table: `class_memberships`

**Purpose:** Theo dõi trạng thái tham gia của mỗi Student trong mỗi Class. State machine: Pending → Approved/Rejected → Removed/Left.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `class_id` | `BIGINT` | NO | — | FK → classes(id) RESTRICT | |
| `student_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | |
| `status` | `TEXT` | NO | `'Pending'` | CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Removed', 'Left')) | Trạng thái tham gia |
| `joined_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Thời điểm Student gửi request |
| `processed_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi Teacher approve/reject/kick |
| `processed_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | Teacher đã xử lý |
| `rejection_reason` | `TEXT` | YES | NULL | CHECK (length <= 500) | Lý do reject (optional) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | = joined_at |

### Unique Constraints & Rules
```sql
-- Chỉ 1 membership ACTIVE per (class, student)
-- Cho phép: Rejected → tạo record mới khi student thử lại
-- Không cho phép: duplicate Pending/Approved
CREATE UNIQUE INDEX uq_memberships_class_student_active
  ON class_memberships (class_id, student_id)
  WHERE status IN ('Pending', 'Approved');

-- NOTE: Khi student bị Rejected và thử lại, tạo ROW MỚI
-- (không update row cũ — lịch sử đầy đủ)
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `class_id` | `classes(id)` | RESTRICT | Không xóa Class khi còn membership records |
| `student_id` | `users(id)` | RESTRICT | Không xóa User khi còn membership |
| `processed_by` | `users(id)` | SET NULL | Audit, không quan trọng nếu teacher bị xóa |

### Indexes
```
pk_class_memberships                PRIMARY KEY (id)
uq_memberships_public_id            UNIQUE (public_id)
uq_memberships_class_student_active UNIQUE (class_id, student_id) WHERE status IN ('Pending', 'Approved')
idx_memberships_class_status        (class_id, status)           -- Teacher xem pending list
idx_memberships_student_approved    (student_id, status) WHERE status = 'Approved'  -- Student xem lớp của mình
idx_memberships_class_approved      (class_id) WHERE status = 'Approved'            -- Count members
```

### State Machine
```
Pending  ──[Teacher approves]──► Approved
Pending  ──[Teacher rejects]───► Rejected

Approved ──[Teacher kicks]─────► Removed     (data preserved)
Approved ──[Student leaves]────► Left        (data preserved)

Rejected ──[Student retries]───► [NEW Pending record]  (NOT update same row)
```

### Notes
- **Không dùng UPDATE** để change status từ Rejected → Pending. Tạo row mới để giữ đầy đủ history.
- `Removed` và `Left` semantic khác nhau nhưng effect giống nhau (Student không còn active trong lớp)
- Khi Student có status `Approved` trong Class, họ có thể xem Assignment của Class đó
- Student bị `Removed`/`Left`: data cũ (Attempt, Grade) vẫn tồn tại, chỉ không nhận Assignment mới
- Partial unique index: chỉ enforce unique trên trạng thái active (Pending + Approved). Rejected/Removed/Left có thể có nhiều rows cho cùng (class_id, student_id)

---

## Relationships Diagram

```
subjects (0..1) ───────────── (*) classes        [nullable FK, SET NULL]
users/Teacher (1) ──────────── (*) classes        [owner_id]
users/Student (1) ──────────── (*) class_memberships
classes (1) ────────────────── (*) class_memberships

classes ──────────────────────────► assignments  [MVP-5 — foreign key từ assignments.class_id]
```

---

## Migration Dependencies

Phụ thuộc từ file `01-schema-auth.md`:
- `users` (cho `owner_id`, `student_id`, `created_by`, `updated_by`, `processed_by`)
- `subjects` (cho `subject_id` — nullable FK, không bắt buộc tồn tại trước)

### Migration order
1. `CREATE TABLE classes` (phụ thuộc `users`; `subjects` FK nullable nên có thể tạo trước hoặc sau)
2. Apply `set_updated_at` trigger cho `classes`
3. `CREATE TABLE class_memberships` (phụ thuộc `classes`, `users`)

---

## Open Questions

- [ ] **Co-teacher**: MVP-2 spec nói "dời sang sau". Khi implement, cần bảng `class_teachers (class_id, teacher_id, role TEXT, invited_at, accepted_at)`. Các index và FK đã được thiết kế để thêm bảng này không phá gì.
- [ ] **Invite code expiry**: Hiện tại invite code không expire theo thời gian (chỉ expire khi regenerate). Có muốn thêm `invite_code_expires_at` không?
- [ ] **Max members per class**: Admin config `max_students_per_class` trong `system_settings`? Enforce ở app layer hay DB constraint?
- [ ] **Class visibility**: Có muốn "Public class" (student tự join không cần duyệt) không? Nếu có, thêm `auto_approve_members BOOLEAN DEFAULT false`
