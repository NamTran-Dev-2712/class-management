# 03 — Schema: Question Bank

> MVP liên quan: [MVP-3 — Question Bank](../mvp/MVP-3.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [01-schema-auth.md](./01-schema-auth.md) — cần `users`; `subjects` là tùy chọn (nullable FK)

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `questions` | Câu hỏi đơn lẻ trong ngân hàng của Teacher |
| `question_options` | Các lựa chọn đáp án cho câu hỏi loại choice |
| `question_tags` | Tags gán cho câu hỏi (free-form, nhiều-nhiều) |

---

## Table: `questions`

**Purpose:** Đơn vị nội dung nhỏ nhất. Teacher tạo và quản lý. Có thể Public để share với Teacher khác hoặc Private.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `teacher_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | Teacher sở hữu câu hỏi |
| `subject_id` | `BIGINT` | YES | NULL | FK → subjects(id) SET NULL | Nhãn môn học (tùy chọn). NULL = câu hỏi chưa/không gắn môn học |
| `type` | `TEXT` | NO | — | CHECK (type IN ('SingleChoice', 'MultipleChoice', 'TrueFalse', 'ShortWriting', 'LongWriting')) | Loại câu hỏi |
| `content` | `TEXT` | NO | — | CHECK (length(content) >= 10 AND length(content) <= 10000) | Nội dung câu hỏi (plain text hoặc HTML sanitized) |
| `difficulty` | `TEXT` | NO | `'Medium'` | CHECK (difficulty IN ('Easy', 'Medium', 'Hard')) | Độ khó |
| `suggested_point` | `NUMERIC(8,2)` | NO | `1.00` | CHECK (suggested_point > 0 AND suggested_point <= 100) | Điểm gợi ý (teacher có thể override khi thêm vào Exam) |
| `visibility` | `TEXT` | NO | `'Private'` | CHECK (visibility IN ('Private', 'Public')) | |
| `explanation` | `TEXT` | YES | NULL | CHECK (length(explanation) <= 2000) | Giải thích đáp án (hiện sau khi chấm) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |
| `deleted_at` | `TIMESTAMPTZ` | YES | NULL | — | Soft delete |
| `created_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | = teacher_id |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | |

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `teacher_id` | `users(id)` | RESTRICT | Không xóa Teacher khi còn câu hỏi |
| `subject_id` | `subjects(id)` | SET NULL | Subject bị xóa → `subject_id` thành NULL, Question vẫn tồn tại |

### Indexes
```
pk_questions                        PRIMARY KEY (id)
uq_questions_public_id              UNIQUE (public_id)
idx_questions_teacher               (teacher_id, deleted_at) WHERE deleted_at IS NULL
idx_questions_subject               (subject_id) WHERE subject_id IS NOT NULL   -- Câu hỏi có gắn subject
idx_questions_type                  (type)
idx_questions_difficulty            (difficulty)
idx_questions_visibility_public     (visibility) WHERE visibility = 'Public' AND deleted_at IS NULL   -- subject_id nullable, không dùng trong composite
idx_questions_teacher_type          (teacher_id, type, difficulty) WHERE deleted_at IS NULL   -- Filter trong bank
-- Full-text search index (xem file 10)
```

### Notes
- `content` format: app layer sanitize HTML trước khi store. DB chỉ lưu sanitized string.
- Soft delete question: `exam_questions` vẫn giữ FK đến `question_id`. Khi Exam được snapshot (MVP-5), content đã được copy. Nếu question bị soft delete sau khi snapshot → snapshot vẫn intact.
- **Không xóa** question đang có trong `exam_questions` active (app kiểm tra). Nếu muốn xóa: phải remove khỏi Exam trước.
- `explanation`: Teacher nhập để show cho Student sau khi làm bài (theo setting `show_answers_after_grade` trong Assignment)
- `suggested_point` là gợi ý; điểm thực tế được set trong `exam_questions.point`

---

## Table: `question_options`

**Purpose:** Lựa chọn đáp án cho các loại câu hỏi có options: `SingleChoice`, `MultipleChoice`, `TrueFalse`.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `question_id` | `BIGINT` | NO | — | FK → questions(id) CASCADE | |
| `content` | `TEXT` | NO | — | CHECK (length >= 1 AND length <= 2000) | Nội dung lựa chọn |
| `is_correct` | `BOOLEAN` | NO | `false` | — | Đây có phải đáp án đúng không |
| `display_order` | `INT` | NO | — | CHECK (display_order >= 0) | Thứ tự hiển thị |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Unique Constraints
```sql
-- Không trùng thứ tự trong cùng câu hỏi
CREATE UNIQUE INDEX uq_question_options_order
  ON question_options (question_id, display_order);
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `question_id` | `questions(id)` | CASCADE | Xóa question → xóa hết options |

### Indexes
```
pk_question_options             PRIMARY KEY (id)
idx_question_options_question   (question_id, display_order)   -- Load options cho câu hỏi, đúng thứ tự
uq_question_options_order       UNIQUE (question_id, display_order)
```

### Business Rules (validate ở app layer)
| Rule | Check |
|---|---|
| `SingleChoice`: đúng 1 option `is_correct = true` | App validate |
| `MultipleChoice`: ≥ 1 option `is_correct = true` | App validate |
| `TrueFalse`: đúng 2 options ("True", "False"), 1 là correct | App validate + seed options khi tạo |
| `ShortWriting`, `LongWriting`: không có options | App validate — không INSERT vào `question_options` |

### Notes
- ON DELETE CASCADE: phù hợp vì option không có ý nghĩa độc lập nếu câu hỏi bị xóa
- `TrueFalse`: app tự generate 2 options cố định. Teacher chỉ chọn cái nào là đúng.
- Không có `updated_at` — nếu cần edit, app DELETE + INSERT lại (simpler) hoặc UPDATE (cần thêm updated_at)

---

## Table: `question_tags`

**Purpose:** Free-form tags gán cho câu hỏi để phân loại và tìm kiếm. Mỗi row = 1 tag cho 1 câu hỏi.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `question_id` | `BIGINT` | NO | — | FK → questions(id) CASCADE | |
| `tag` | `TEXT` | NO | — | CHECK (tag ~ '^[a-z0-9\-]{1,50}$') | Lowercase, alphanumeric + dash |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Unique Constraints
```sql
-- Không tag trùng trên cùng câu hỏi
CREATE UNIQUE INDEX uq_question_tags_unique
  ON question_tags (question_id, tag);
```

### Foreign Keys
| Column | References | On Delete |
|---|---|---|
| `question_id` | `questions(id)` | CASCADE |

### Indexes
```
pk_question_tags                PRIMARY KEY (id)
uq_question_tags_unique         UNIQUE (question_id, tag)
idx_question_tags_tag           (tag)                           -- Tìm kiếm theo tag
idx_question_tags_question      (question_id)                   -- Tags của 1 câu hỏi
```

### Notes
- App normalize tag: lowercase, trim whitespace, replace spaces với `-`, remove special chars
- CHECK constraint chỉ nhận lowercase + alphanumeric + dash (sau khi app normalize)
- `tag` max 50 chars
- Không có `updated_at` — tag management là DELETE + INSERT
- Để lấy tất cả tags của teacher: JOIN với questions.teacher_id

---

## Relationships Diagram

```
users/Teacher (1) ─────────── (*) questions
subjects (0..1) ────────────── (*) questions   [nullable FK, SET NULL]

questions (1) ──────────────── (*) question_options    [CASCADE]
questions (1) ──────────────── (*) question_tags       [CASCADE]

questions (*) ─────────────── (*) exams                [via exam_questions — MVP-4]
```

---

## Full-Text Search Design

### Tìm kiếm trong Question Bank

Hai cách approach cho MVP-3:

**Option A — ILIKE (đơn giản, đủ dùng cho MVP)**
```sql
-- Index hỗ trợ LIKE prefix
CREATE INDEX idx_questions_content_pattern ON questions USING btree (content text_pattern_ops);
-- Query: WHERE content ILIKE '%keyword%' -- chậm với large dataset
```

**Option B — tsvector full-text search (recommended cho production)**
```sql
-- Generated column lưu tsvector
ALTER TABLE questions ADD COLUMN content_search TSVECTOR
  GENERATED ALWAYS AS (to_tsvector('english', content)) STORED;

-- GIN index trên tsvector
CREATE INDEX gin_questions_content_search ON questions USING GIN (content_search);

-- Query: WHERE content_search @@ plainto_tsquery('english', :keyword)
```

> **Đề xuất:** Implement Option B ngay từ đầu — generated column không tốn effort maintain, GIN index nhanh hơn ILIKE rất nhiều khi data lớn.

### Tag search
```sql
-- Đơn giản: exact match + ILIKE
WHERE tag = 'dai-so'           -- exact
WHERE tag ILIKE 'dai%'         -- prefix (btree index đủ)
```

---

## Migration Dependencies

Phụ thuộc:
- `users` (cho `teacher_id`)
- `subjects` (cho `subject_id` — nullable FK, không bắt buộc)

### Migration order
1. `CREATE TABLE questions`
2. Apply `set_updated_at` trigger cho `questions`
3. `CREATE TABLE question_options`
4. `CREATE TABLE question_tags`
5. Tạo full-text search generated column + GIN index

---

## Open Questions

- [ ] `content` format: plain text hay HTML/Markdown? HTML cần sanitize server-side. Ảnh hưởng CHECK constraint.
- [ ] Hình ảnh trong câu hỏi: inline base64 (không khuyến nghị), URL external, hay upload riêng? Nếu upload riêng: thêm bảng `question_attachments`
- [ ] `TrueFalse` options: seed "True"/"False" tự động ở app hay cho teacher nhập tên option ("Đúng"/"Sai")?
- [ ] Language-aware full-text search: `'english'` dictionary cho tiếng Anh, nhưng nội dung tiếng Việt cần `simple` dictionary. Quyết định khi implement.
- [ ] Giới hạn số tags per câu hỏi? Đề xuất: max 10 tags (validate app)
