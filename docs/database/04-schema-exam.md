# 04 — Schema: Exam Builder

> MVP liên quan: [MVP-4 — Exam Builder](../mvp/MVP-4.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [03-schema-question-bank.md](./03-schema-question-bank.md) — cần `questions`

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `exams` | Template đề thi — tập hợp có thứ tự của Questions |
| `exam_questions` | Junction table: Exam ↔ Question với điểm và thứ tự |
| `exam_tags` | Thẻ tự do (nhãn) gắn vào Exam để tìm kiếm/lọc — giống `question_tags` |

---

## Table: `exams`

**Purpose:** Exam là template/đề mẫu tái sử dụng được. Khi giao bài (MVP-5), Exam được snapshot để bảo toàn nội dung tại thời điểm đó.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `teacher_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | Teacher sở hữu đề |
| `subject_id` | `BIGINT` | YES | NULL | FK → subjects(id) SET NULL | Nhãn môn học (tùy chọn). NULL = đề không gắn môn học cụ thể |
| `title` | `TEXT` | NO | — | CHECK (length >= 3 AND length <= 300) | Tên đề thi |
| `description` | `TEXT` | YES | NULL | CHECK (length <= 1000) | Mô tả đề thi |
| `visibility` | `TEXT` | NO | `'Private'` | CHECK (visibility IN ('Private', 'Public')) | |
| `version` | `INT` | NO | `1` | CHECK (version >= 1) | Tăng mỗi lần save thay đổi nội dung |
| `total_point` | `NUMERIC(8,2)` | NO | `0` | CHECK (total_point >= 0) | Tổng điểm (computed từ exam_questions.point — denormalized cache) |
| `total_questions` | `INT` | NO | `0` | CHECK (total_questions >= 0) | Số câu (denormalized cache) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |
| `deleted_at` | `TIMESTAMPTZ` | YES | NULL | — | Soft delete |
| `created_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | |

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `teacher_id` | `users(id)` | RESTRICT | |
| `subject_id` | `subjects(id)` | SET NULL | Subject bị xóa → `subject_id` thành NULL, Exam vẫn tồn tại |

### Indexes
```
pk_exams                        PRIMARY KEY (id)
uq_exams_public_id              UNIQUE (public_id)
idx_exams_teacher               (teacher_id) WHERE deleted_at IS NULL
idx_exams_subject               (subject_id) WHERE subject_id IS NOT NULL AND deleted_at IS NULL   -- Exams có gắn subject
idx_exams_visibility_public     (visibility) WHERE visibility = 'Public' AND deleted_at IS NULL    -- subject_id nullable, bỏ khỏi composite
idx_exams_teacher_visibility    (teacher_id, visibility) WHERE deleted_at IS NULL   -- Filter bank của teacher
```

### Versioning Strategy
```
version tăng khi:
  - Thêm / bỏ question khỏi exam
  - Thay đổi điểm (point) của question trong exam
  - Thay đổi thứ tự (display_order) của question
  - KHÔNG tăng khi: sửa title, description, visibility

Khi tạo Assignment (MVP-5):
  - assignments.exam_version_at_publish = exams.version tại thời điểm publish
  - Snapshot được tạo với nội dung hiện tại của version đó
```

### Denormalized Cache Columns
`total_point` và `total_questions` được tính lại khi `exam_questions` thay đổi.

> **Quyết định (đã triển khai MVP-4)**: dùng **App layer** — KHÔNG dùng DB trigger.
> `UpdateExamQuestionsCommandHandler` thay thế toàn bộ `exam_questions` (wholesale-replace), rồi tính
> lại `total_point = Σ point`, `total_questions = count` và tăng `version += 1` trong **cùng một**
> `SaveChangesAsync`. Lý do: dễ debug, đúng chuẩn Unit-of-Work hiện tại của repo, và `version` tăng
> đúng **+1 mỗi lần lưu** (đúng spec MVP-4) thay vì nhảy nhiều như khi trigger chạy trên từng row.
> Metadata (title/description/subject/visibility) sửa qua `UpdateExamCommand` **không** tăng version.

### Notes
- Không xóa Exam đang có Assignment (app kiểm tra). Soft delete chỉ khi không còn reference.
- Teacher sửa Exam sau khi có Assignment → version tăng → không ảnh hưởng snapshot cũ (đã được tạo với version cũ)
- Khi Teacher duplicate Exam Public: INSERT exam mới + INSERT exam_questions mới (không có link về original)

---

## Table: `exam_questions`

**Purpose:** Junction table với attributes. Mỗi row = 1 Question trong 1 Exam, với điểm và thứ tự riêng.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `exam_id` | `BIGINT` | NO | — | FK → exams(id) CASCADE | |
| `question_id` | `BIGINT` | NO | — | FK → questions(id) RESTRICT | |
| `display_order` | `INT` | NO | — | CHECK (display_order >= 1) | Thứ tự câu trong đề (1-based) |
| `point` | `NUMERIC(8,2)` | NO | — | CHECK (point > 0 AND point <= 100) | Điểm của câu này trong đề (override suggested_point) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Unique Constraints
```sql
-- Không trùng thứ tự trong cùng đề
CREATE UNIQUE INDEX uq_exam_questions_order
  ON exam_questions (exam_id, display_order);

-- Không thêm cùng câu hỏi 2 lần vào 1 đề
CREATE UNIQUE INDEX uq_exam_questions_unique
  ON exam_questions (exam_id, question_id);
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `exam_id` | `exams(id)` | CASCADE | Xóa Exam → xóa hết exam_questions |
| `question_id` | `questions(id)` | RESTRICT | Không xóa Question khi còn trong Exam |

### Indexes
```
pk_exam_questions               PRIMARY KEY (id)
uq_exam_questions_order         UNIQUE (exam_id, display_order)
uq_exam_questions_unique        UNIQUE (exam_id, question_id)
idx_exam_questions_exam         (exam_id, display_order)       -- Load questions của exam theo thứ tự
idx_exam_questions_question     (question_id)                  -- "Question này đang trong Exams nào?" (dùng khi check trước khi xóa question)
```

### Sync `exams` denormalized columns — App layer (KHÔNG dùng trigger)

> Thiết kế ban đầu đề xuất một trigger `sync_exam_stats()`. **MVP-4 không dùng trigger** (xem mục
> *Denormalized Cache Columns* ở trên). Thay vào đó application handler tính lại totals + version trong
> một transaction khi lưu danh sách câu hỏi. Vì vậy migration `20260617150412_create_exams_and_views`
> **không** tạo trigger/function nào trên `exam_questions`.

### Notes
- ON DELETE RESTRICT trên `question_id`: đây là guard quan trọng — không cho soft delete question khi còn trong exam
- Khi Teacher soft delete question đang trong exam: app trả lỗi "Question đang được dùng trong N Exam"
- App phải remove question khỏi tất cả exam trước khi soft delete

---

## Table: `exam_tags`

**Purpose:** Thẻ tự do gắn vào Exam (chuẩn hóa lowercase `[a-z0-9-]`, ≤10 thẻ/đề) để giáo viên gắn nhãn,
tìm kiếm và lọc đề khi giao bài. Append-only child của `exams` (cascade-delete, thay thế toàn bộ khi
sửa). Là **metadata** — thẻ Exam **không** đi vào assignment snapshot (MVP-5). Mirror `question_tags`.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `exam_id` | `BIGINT` | NO | — | FK → exams(id) CASCADE | |
| `tag` | `VARCHAR(50)` | NO | — | CHECK (tag ~ `'^[a-z0-9-]{1,50}$'`) | Slug đã chuẩn hóa |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Indexes
```
pk_exam_tags                    PRIMARY KEY (id)
uq_exam_tags_unique             UNIQUE (exam_id, tag)   -- không trùng thẻ trong 1 đề
idx_exam_tags_tag               (tag)                   -- lọc theo thẻ
```

### Notes
- Chuẩn hóa + cap ≤10 ở app (`ExamTagNormalizer`, `ExamAssembler.BuildTags`); validator chặn >10 thẻ thô.
- `UpdateExamCommandHandler` thay thế toàn bộ tập thẻ (clear + re-add) trong cùng `SaveChangesAsync`,
  **không** tăng `version` (chỉ là metadata). `DuplicateExamCommandHandler` copy luôn thẻ sang bản sao.

---

## Relationships Diagram

```
users/Teacher (1) ─────────── (*) exams
subjects (0..1) ────────────── (*) exams        [nullable FK, SET NULL]

exams (1) ──────────────────── (*) exam_questions
questions (1) ──────────────── (*) exam_questions
exams (1) ──────────────────── (*) exam_tags        [cascade]

exams (*) ──────────────────── (*) assignments  [via assignments.exam_id — MVP-5]
```

---

## Read-model view: `vw_exams`

Một row mỗi exam **chưa xóa**, kèm tên hiển thị của owner + tên/public_id môn học còn sống. Cho phép
list/detail queries dùng `BaseGetQueryHandler` mà không phải join thẳng vào `ApplicationUser`
(Identity). `total_point`/`total_questions`/`version` lấy trực tiếp từ cột denormalized trên `exams`.

`tags` là `text[]` gộp từ `exam_tags` (rỗng `{}` khi đề không có thẻ) để list/detail dùng chung, lọc
bằng `tag = ANY(tags)`.

```sql
CREATE VIEW vw_exams AS
SELECT e.id, e.public_id, e.title, e.description, e.visibility, e.version,
       e.total_point, e.total_questions, e.created_at, e.updated_at,
       e.subject_id, sub.public_id AS subject_public_id, sub.name AS subject_name,
       e.teacher_id, t.public_id AS teacher_public_id, t.display_name AS teacher_name,
       COALESCE(tg.tags, '{}'::text[]) AS tags
FROM exams e
JOIN users t ON t.id = e.teacher_id
LEFT JOIN subjects sub ON sub.id = e.subject_id AND sub.deleted_at IS NULL
LEFT JOIN (
    SELECT exam_id, array_agg(tag ORDER BY tag) AS tags
    FROM exam_tags GROUP BY exam_id
) tg ON tg.exam_id = e.id
WHERE e.deleted_at IS NULL;
```

> Thẻ Exam được thêm ở migration `20260618150808_add_exam_tags_and_update_view` (tạo bảng `exam_tags`
> rồi DROP + tạo lại `vw_exams` kèm cột `tags`).

> Detail/preview reads load thêm `exam_questions` (theo `display_order`) rồi join `vw_questions` để
> lấy nội dung/loại/độ khó/số phương án từng câu. Câu hỏi đã soft-delete (vắng mặt trong `vw_questions`)
> được đánh dấu `is_available = false` để teacher thay trước khi publish (MVP-5).

---

## Migration Dependencies

Phụ thuộc:
- `users` (từ 01-schema-auth.md)
- `subjects` (từ 01-schema-auth.md — nullable FK, không bắt buộc)
- `questions` (từ 03-schema-question-bank.md)

### Migration order (đã triển khai — `20260617150412_create_exams_and_views`)
1. `CREATE TABLE exams` (phụ thuộc users; subjects nullable nên không bắt buộc tồn tại trước)
2. `CREATE TABLE exam_questions` (phụ thuộc exams, questions)
3. `CREATE VIEW vw_exams`
4. `updated_at` set qua `AuditableEntityInterceptor` (không trigger); **không** có `sync_exam_stats`
   (totals + version sync ở app layer — xem mục *Denormalized Cache Columns*).

---

## Open Questions (đã chốt cho MVP-4)

- [x] **DB trigger hay app layer** để sync `total_point`/`total_questions`/`version`? → **App layer**
  (handler tính lại trong 1 transaction; `version += 1` mỗi lần lưu danh sách câu).
- [x] **Reorder nhiều câu cùng lúc** → wholesale-replace toàn bộ `exam_questions` trong 1
  `SaveChangesAsync`: xóa hết rồi insert lại theo thứ tự mới ⇒ không va chạm unique
  `(exam_id, display_order)`, không cần deferred constraint.
- [x] **Max questions per exam** → **app validate** (`UpdateExamQuestionsValidator`, hard limit 500),
  không CHECK constraint cứng trên bảng.
- [ ] **Exam import** (DOCX/PDF) → ROADMAP-FUTURE; sẽ cần `exam_import_jobs` + `imported_from`.
