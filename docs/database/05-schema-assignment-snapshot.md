# 05 — Schema: Assignment & Snapshot

> MVP liên quan: [MVP-5 — Assignment & Online Testing](../mvp/MVP-5.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [02-schema-classroom.md](./02-schema-classroom.md), [04-schema-exam.md](./04-schema-exam.md)

---

## Tại sao cần Snapshot?

Đây là **design decision quan trọng nhất** của toàn bộ hệ thống.

**Vấn đề:** Nếu Attempt đọc trực tiếp từ `exams`/`questions`, khi Teacher sửa câu hỏi sau khi học sinh đã làm bài → điểm cũ có thể sai, lịch sử không còn chính xác.

**Giải pháp — Snapshot pattern:**
```
Assignment publish
    │
    ▼
Hệ thống copy toàn bộ nội dung Exam + Questions vào:
  - assignment_snapshots    (container)
  - snapshot_questions      (copy câu hỏi)
  - snapshot_options        (copy đáp án)
    │
    ▼
Attempt chỉ đọc từ snapshot tables
Exam/Question gốc có thể tự do thay đổi → không ảnh hưởng
```

**Đặc điểm snapshot tables:**
- **Append-only**: chỉ INSERT, không bao giờ UPDATE hay DELETE
- Không có `updated_at`
- Không có `deleted_at` (không soft delete)
- FK `original_question_id` → không có cascading (SET NULL) — snapshot tồn tại dù question gốc bị xóa

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `assignments` | Bài được giao — Exam + Class + cấu hình thời gian/chính sách |
| `assignment_snapshots` | Container cho snapshot (1-1 với assignment) |
| `snapshot_questions` | Bản copy câu hỏi tại thời điểm publish |
| `snapshot_options` | Bản copy options của câu hỏi |
| `snapshot_media` | (MVP-9) Bản đóng băng media tham chiếu + guard chống cleanup |

---

## Table: `assignments`

**Purpose:** Kết nối Exam + Class + cấu hình (thời gian, policy). Một Exam có thể được giao nhiều lần (nhiều Assignment) cho các Class khác nhau.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `exam_id` | `BIGINT` | NO | — | FK → exams(id) RESTRICT | Exam template dùng cho bài này |
| `exam_version_at_publish` | `INT` | YES | NULL | — | Version của Exam khi publish (snapshot created from this version) |
| `class_id` | `BIGINT` | NO | — | FK → classes(id) RESTRICT | Lớp được giao |
| `teacher_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | Teacher tạo Assignment |
| `title` | `TEXT` | NO | — | CHECK (length >= 3 AND length <= 300) | Tên bài (có thể khác tên Exam) |
| `description` | `TEXT` | YES | NULL | CHECK (length <= 1000) | |
| `opens_at` | `TIMESTAMPTZ` | YES | NULL | — | NULL = mở ngay khi publish |
| `closes_at` | `TIMESTAMPTZ` | YES | NULL | CHECK (closes_at > opens_at OR opens_at IS NULL) | NULL = không deadline |
| `time_limit_minutes` | `INT` | YES | NULL | CHECK (time_limit_minutes > 0 AND time_limit_minutes <= 1440) | NULL = không giới hạn thời gian. Max 24h |
| `max_attempts` | `INT` | NO | `1` | CHECK (max_attempts >= 1 AND max_attempts <= 100) | Số lần thi tối đa |
| `score_policy` | `TEXT` | NO | `'Highest'` | CHECK (score_policy IN ('Highest', 'Latest')) | Với max_attempts > 1: dùng điểm nào (enum lưu PascalCase, xem "as built") |
| `allow_late` | `BOOLEAN` | NO | `false` | — | Cho phép nộp sau closes_at |
| `grade_publish_policy` | `TEXT` | NO | `'AfterDeadline'` | CHECK (grade_publish_policy IN ('Immediate', 'AfterDeadline', 'Manual')) | Khi nào student xem được điểm (enum PascalCase) |
| `shuffle_questions` | `BOOLEAN` | NO | `false` | — | Random thứ tự câu hỏi per-attempt |
| `shuffle_options` | `BOOLEAN` | NO | `false` | — | Random thứ tự options per-attempt |
| `show_answers_after_grade` | `BOOLEAN` | NO | `false` | — | Hiện đáp án đúng sau khi chấm |
| `status` | `TEXT` | NO | `'Draft'` | CHECK (status IN ('Draft', 'Scheduled', 'Open', 'Closed', 'Archived')) | State machine |
| `published_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi nào Assignment được publish |
| `closed_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi nào Assignment đóng (manual hoặc auto) |
| `grades_released_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi nào điểm được công bố (manual policy) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |
| `created_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | = teacher_id |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | |

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `exam_id` | `exams(id)` | RESTRICT | Không xóa Exam khi còn Assignment |
| `class_id` | `classes(id)` | RESTRICT | Không xóa Class khi còn Assignment |
| `teacher_id` | `users(id)` | RESTRICT | |

### Indexes
```
pk_assignments                          PRIMARY KEY (id)
uq_assignments_public_id               UNIQUE (public_id)
idx_assignments_class_status_opens     (class_id, status, opens_at)       -- HOT: "assignments của lớp đang Open"
idx_assignments_teacher                (teacher_id, status)               -- Teacher quản lý assignments
idx_assignments_exam                   (exam_id)                          -- Check trước khi xóa Exam
idx_assignments_class_open             (class_id) WHERE status = 'Open'  -- Partial: chỉ Open
idx_assignments_scheduled              (opens_at) WHERE status = 'Scheduled'  -- Background job: Scheduled → Open
idx_assignments_closes_at              (closes_at) WHERE status = 'Open'      -- Background job: tự đóng
```

### State Machine
```
Draft ──[publish: opens_at > NOW()]──► Scheduled ──[opens_at reached]──► Open
      ──[publish: opens_at ≤ NOW()]──────────────────────────────────► Open

Open  ──[closes_at reached OR teacher manual close]──► Closed
      ──[teacher manual close]──────────────────────► Closed

Closed ──[teacher archive]──► Archived

Draft / Scheduled / Open ──[teacher archive early]──► Archived (với confirm)
```

### Notes
- **Background jobs** cần thiết:
  1. `Scheduled → Open`: khi `NOW() >= opens_at` (run mỗi phút)
  2. `Open → Closed`: khi `NOW() >= closes_at` (run mỗi phút)
  3. Trigger auto-submit tất cả `InProgress` attempts khi assignment Closed
- `exam_version_at_publish`: để biết snapshot được tạo từ version nào của Exam (audit/debug)
- `grades_released_at` SET khi Teacher bấm "Công bố điểm" (`grade_publish_policy = manual`)

---

## Table: `assignment_snapshots`

**Purpose:** Container snapshot — 1-1 với Assignment. Tạo ngay khi Assignment được publish.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `assignment_id` | `BIGINT` | NO | — | FK → assignments(id) RESTRICT, UNIQUE | 1-1 với assignment |
| `total_point` | `NUMERIC(8,2)` | NO | — | CHECK (total_point > 0) | Tổng điểm tại thời điểm snapshot |
| `total_questions` | `INT` | NO | — | CHECK (total_questions > 0) | Số câu tại thời điểm snapshot |
| `snapshot_created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Indexes
```
pk_assignment_snapshots             PRIMARY KEY (id)
uq_assignment_snapshots_assignment  UNIQUE (assignment_id)       -- 1-1 relationship
```

### Notes
- **KHÔNG** có `updated_at` — immutable after creation
- **KHÔNG** soft delete — snapshot tồn tại vĩnh viễn
- 1-1 với Assignment: UNIQUE constraint trên `assignment_id`
- ON DELETE RESTRICT: không xóa Assignment khi còn snapshot (và ngược lại về logic)

---

## Table: `snapshot_questions`

**Purpose:** Bản copy hoàn chỉnh của mỗi Question tại thời điểm snapshot. Attempt đọc từ đây, không từ `questions` gốc.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `snapshot_id` | `BIGINT` | NO | — | FK → assignment_snapshots(id) RESTRICT | |
| `original_question_id` | `BIGINT` | YES | NULL | FK → questions(id) SET NULL | Reference về gốc (NULL nếu question gốc bị xóa) |
| `type` | `TEXT` | NO | — | CHECK (type IN ('SingleChoice', 'MultipleChoice', 'TrueFalse', 'ShortWriting', 'LongWriting')) | Copy từ question.type |
| `content` | `TEXT` | NO | — | — | Copy từ question.content |
| `point` | `NUMERIC(8,2)` | NO | — | CHECK (point > 0) | Copy từ exam_questions.point |
| `display_order` | `INT` | NO | — | CHECK (display_order >= 1) | Copy từ exam_questions.display_order |
| `explanation` | `TEXT` | YES | NULL | — | Copy từ question.explanation |
| `snapshot_created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Unique Constraints
```sql
CREATE UNIQUE INDEX uq_snapshot_questions_order
  ON snapshot_questions (snapshot_id, display_order);
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `snapshot_id` | `assignment_snapshots(id)` | RESTRICT | Snapshot phải còn khi có questions |
| `original_question_id` | `questions(id)` | SET NULL | **Quan trọng**: snapshot vẫn tồn tại dù question gốc bị xóa |

### Indexes
```
pk_snapshot_questions                   PRIMARY KEY (id)
idx_snapshot_questions_snapshot_order   (snapshot_id, display_order)     -- HOT: load questions của attempt
idx_snapshot_questions_original         (original_question_id)           -- Audit: question này đang có trong snapshots nào
uq_snapshot_questions_order             UNIQUE (snapshot_id, display_order)
```

### Notes
- **Critical**: `original_question_id` dùng `ON DELETE SET NULL` (không CASCADE, không RESTRICT). Snapshot phải tồn tại độc lập.
- Khi tạo snapshot: copy `content`, `type`, `point` (từ exam_questions), `display_order`, `explanation` từ question/exam_questions gốc
- **Không** lưu `difficulty`, `teacher_id`, `visibility` — không cần thiết cho việc làm bài

---

## Table: `snapshot_options`

**Purpose:** Bản copy các options/lựa chọn của câu hỏi tại thời điểm snapshot.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `snapshot_question_id` | `BIGINT` | NO | — | FK → snapshot_questions(id) RESTRICT | |
| `original_option_id` | `BIGINT` | YES | NULL | FK → question_options(id) SET NULL | Reference về gốc |
| `content` | `TEXT` | NO | — | — | Copy từ question_options.content |
| `is_correct` | `BOOLEAN` | NO | — | — | Copy từ question_options.is_correct |
| `display_order` | `INT` | NO | — | CHECK (display_order >= 0) | Copy từ question_options.display_order |
| `snapshot_created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Unique Constraints
```sql
CREATE UNIQUE INDEX uq_snapshot_options_order
  ON snapshot_options (snapshot_question_id, display_order);
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `snapshot_question_id` | `snapshot_questions(id)` | RESTRICT | |
| `original_option_id` | `question_options(id)` | SET NULL | Snapshot tồn tại độc lập |

### Indexes
```
pk_snapshot_options                     PRIMARY KEY (id)
idx_snapshot_options_question_order     (snapshot_question_id, display_order)   -- HOT: load options khi làm bài
uq_snapshot_options_order               UNIQUE (snapshot_question_id, display_order)
```

---

## Table: `snapshot_media` (MVP-9)

**Purpose:** Bản đóng băng các media mà một câu hỏi/option đã hiển thị tại thời điểm publish. Hai vai trò: (1) bản ghi bất biến về media + `frozen_url` lúc publish; (2) **cleanup guard** — asset đã soft-delete mà `media_public_id` còn xuất hiện ở đây thì **không** bị xóa vật lý, nên Attempt cũ/đang làm vẫn xem được (BR-9-06 / BR-9-08). Append-only, immutable (RESTRICT).

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `snapshot_question_id` | `BIGINT` | NO | — | FK → snapshot_questions(id) RESTRICT | |
| `snapshot_option_id` | `BIGINT` | YES | NULL | FK → snapshot_options(id) RESTRICT | Set khi media gắn ở cấp option |
| `media_public_id` | `UUID` | NO | — | — | ID media gốc (dùng cho cleanup guard) |
| `frozen_url` | `VARCHAR(1000)` | NO | — | — | URL CDN tại thời điểm publish |
| `kind` | `TEXT` | NO | — | CHECK (kind IN ('Image', 'Audio', 'Video')) | |
| `role` | `TEXT` | NO | — | CHECK (role IN ('Inline', 'Attachment')) | |
| `display_order` | `INT` | NO | — | CHECK (display_order >= 0) | |
| `snapshot_created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `snapshot_question_id` | `snapshot_questions(id)` | RESTRICT | Immutable |
| `snapshot_option_id` | `snapshot_options(id)` | RESTRICT | Immutable |

### Indexes
```
pk_snapshot_media               PRIMARY KEY (id)
idx_snapshot_media_question     (snapshot_question_id)
idx_snapshot_media_public_id    (media_public_id)   -- cleanup guard: asset còn được pin?
```

Nguồn pin lúc publish: `question_media` (attachment) + `question_options.media_id` (ảnh option) + media inline parse từ content/explanation khớp `PublicBaseUrl`. Xem [15-schema-media.md](./15-schema-media.md).

---

## Snapshot Creation Flow

```
[Teacher publish Assignment]
        │
        ▼
1. Validate Exam có ≥ 1 question
2. INSERT INTO assignment_snapshots (assignment_id, total_point, total_questions)
3. Với mỗi exam_question:
   a. INSERT INTO snapshot_questions (snapshot_id, original_question_id, type, content, point, display_order, explanation)
   b. Nếu question có options (SingleChoice/MultipleChoice/TrueFalse):
      Với mỗi question_option:
      INSERT INTO snapshot_options (snapshot_question_id, original_option_id, content, is_correct, display_order)
      (MVP-9) nếu option có media_id → INSERT snapshot_media (snapshot_option_id, media_public_id, frozen_url, ...)
   c. (MVP-9) Với mỗi media của question (question_media attachment + inline parse từ content/explanation):
      INSERT INTO snapshot_media (snapshot_question_id, media_public_id, frozen_url, kind, role, display_order)
4. UPDATE assignments SET status = 'Scheduled'/'Open', published_at = NOW(), exam_version_at_publish = exams.version
5. [Atomic transaction — rollback nếu bất kỳ bước nào fail]
```

---

## Relationships Diagram

```
exams (1) ──────────────────── (*) assignments
classes (1) ─────────────────── (*) assignments
users/Teacher (1) ───────────── (*) assignments

assignments (1) ─────────────── (1) assignment_snapshots   [1-1]
assignment_snapshots (1) ─────── (*) snapshot_questions    [immutable]
snapshot_questions (1) ────────── (*) snapshot_options     [immutable]
snapshot_questions (1) ────────── (*) snapshot_media       [immutable, MVP-9]

questions (1? → 0) ─────────────── (*) snapshot_questions  [SET NULL on delete]
question_options (1? → 0) ─────── (*) snapshot_options    [SET NULL on delete]

assignments (1) ─────────────────── (*) attempts           [MVP-6]
```

---

## Migration Dependencies

Phụ thuộc:
- `users` (teacher_id)
- `classes` (class_id)
- `exams` (exam_id)
- `questions` (original_question_id)
- `question_options` (original_option_id)

### Migration order
1. `CREATE TABLE assignments`
2. Apply `set_updated_at` trigger cho `assignments`
3. `CREATE TABLE assignment_snapshots`
4. `CREATE TABLE snapshot_questions`
5. `CREATE TABLE snapshot_options`

---

## Open Questions

- [ ] **Snapshot cleanup**: Có bao giờ xóa snapshot không? Đề xuất: không xóa, chỉ archive. Data retention policy cần quyết định.
- [ ] **Background job**: Dùng gì để chạy Scheduled→Open và Open→Closed transitions? Hangfire (cho .NET), Quartz.NET, hay PostgreSQL `pg_cron`?
- [ ] **Atomic snapshot**: Nếu snapshot creation fails midway (network, timeout), Assignment vẫn Draft. Cần retry mechanism. Idempotent snapshot creation?
- [ ] **closes_at nullable**: Assignment không có deadline (NULL) có hợp lý không? Phải thêm constraint: nếu `time_limit_minutes` có giá trị thì `closes_at` cần có hoặc app tự auto-close

---

## Implementation notes (MVP-5, as built)

Migration `20260617173525_create_assignments_attempts_and_views`. Deviations from the design above,
kept here as the authoritative record:

- **Enum casing**: `score_policy`/`grade_publish_policy`/`status` are stored as the **PascalCase**
  enum member names (`Highest`/`Latest`; `Immediate`/`AfterDeadline`/`Manual`;
  `Draft`/`Scheduled`/`Open`/`Closed`/`Archived`) to match the app-wide `JsonStringEnumConverter` +
  EF `.HasConversion<string>()` convention. CHECK constraints use these values.
- **Snapshot tables expose `public_id`**: `snapshot_questions` and `snapshot_options` each have a
  `public_id UUID UNIQUE DEFAULT gen_random_uuid()`. The attempt-taking/answer API references questions
  and options by their public id (never the internal `long id`), consistent with the rest of the system.
- **Atomic snapshot**: publish builds the snapshot + flips status in a **single `SaveChangesAsync`**
  (one EF transaction) in `PublishAssignmentCommandHandler`. If any step fails the assignment stays
  `Draft` (publish is idempotent — rejected once status ≠ Draft).
- **Background job (resolved)**: a single **Hangfire recurring job** `assignment-lifecycle`
  (cron from `Assignment:LifecycleSweepCron`, default every minute) runs
  `RunAssignmentLifecycleCommand`: `Scheduled→Open`, `Open→Closed` (+auto-submit), and auto-submit of
  attempts past `deadline_at`. Plus a **lazy server-side check** on every attempt request. See
  `docs/database/14-background-jobs.md`.
- **Snapshot cleanup (resolved)**: snapshots are **never deleted** (append-only); assignments are
  soft-deleted and deletion is blocked while any attempt exists.

### Read-model views (created via raw SQL in the migration)
- `vw_assignments` — teacher/admin lists + detail: assignment + class/exam/owner names + snapshot
  totals + attempt counts; excludes soft-deleted assignments.
- `vw_student_assignments` — one row per (published assignment, **approved** student of its class) with
  that student's attempt stats (`used_attempts`, `has_in_progress`, `in_progress_attempt_public_id`,
  `best_score`). Only `Scheduled/Open/Closed` assignments appear.
- `vw_attempts` — one row per attempt (+ student name, assignment title, snapshot total point).
