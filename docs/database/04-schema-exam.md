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

Cách maintain (chọn 1):
1. **App layer**: recalculate sau mỗi INSERT/UPDATE/DELETE vào `exam_questions`
2. **DB Trigger**: trigger trên `exam_questions` → UPDATE `exams.total_point`, `exams.total_questions`, `exams.version`

> **Đề xuất**: Dùng DB trigger để đảm bảo consistency ngay cả khi app có bug.

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

### Trigger: Sync `exams` denormalized columns

```sql
-- Sau mỗi INSERT/UPDATE/DELETE trên exam_questions → update exams
CREATE OR REPLACE FUNCTION sync_exam_stats()
RETURNS TRIGGER AS $$
BEGIN
  UPDATE exams
  SET
    total_point = (SELECT COALESCE(SUM(point), 0) FROM exam_questions WHERE exam_id = COALESCE(NEW.exam_id, OLD.exam_id)),
    total_questions = (SELECT COUNT(*) FROM exam_questions WHERE exam_id = COALESCE(NEW.exam_id, OLD.exam_id)),
    version = version + 1,
    updated_at = NOW()
  WHERE id = COALESCE(NEW.exam_id, OLD.exam_id);
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_exam_questions_sync
  AFTER INSERT OR UPDATE OR DELETE ON exam_questions
  FOR EACH ROW EXECUTE FUNCTION sync_exam_stats();
```

### Notes
- ON DELETE RESTRICT trên `question_id`: đây là guard quan trọng — không cho soft delete question khi còn trong exam
- Khi Teacher soft delete question đang trong exam: app trả lỗi "Question đang được dùng trong N Exam"
- App phải remove question khỏi tất cả exam trước khi soft delete

---

## Relationships Diagram

```
users/Teacher (1) ─────────── (*) exams
subjects (0..1) ────────────── (*) exams        [nullable FK, SET NULL]

exams (1) ──────────────────── (*) exam_questions
questions (1) ──────────────── (*) exam_questions

exams (*) ──────────────────── (*) assignments  [via assignments.exam_id — MVP-5]
```

---

## Migration Dependencies

Phụ thuộc:
- `users` (từ 01-schema-auth.md)
- `subjects` (từ 01-schema-auth.md — nullable FK, không bắt buộc)
- `questions` (từ 03-schema-question-bank.md)

### Migration order
1. `CREATE TABLE exams` (phụ thuộc users; subjects nullable nên không bắt buộc tồn tại trước)
2. Apply `set_updated_at` trigger cho `exams`
3. `CREATE TABLE exam_questions` (phụ thuộc exams, questions)
4. Create `sync_exam_stats` trigger function + trigger

---

## Open Questions

- [ ] Nên dùng **DB trigger** hay **app layer** để sync `total_point`/`total_questions`/`version`? Trigger an toàn hơn nhưng khó debug hơn trong EF Core
- [ ] **Reorder nhiều câu cùng lúc**: khi drag-and-drop nhiều câu, cần UPDATE nhiều rows display_order. Cách tốt: batch UPDATE trong 1 transaction với deferred unique constraint check
- [ ] **Exam import**: future feature cho phép import đề từ DOCX/PDF → cần `exam_import_jobs` table. Cột `imported_from` TEXT nullable trong `exams`?
- [ ] **Max questions per exam**: app validate hoặc CHECK constraint? Đề xuất: app validate với warning ở ~100 câu, hard limit ở 500 câu
