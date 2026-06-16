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
| `content` | `TEXT` | NO | — | CHECK (length(content) >= 10 AND length(content) <= 10000) | Nội dung câu hỏi (**Markdown**, render ở FE bằng react-markdown + rehype-sanitize — cho phép HTML subset an toàn) |
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
> **Trạng thái triển khai (MVP-3):** đã quyết & hiện thực như sau (cập nhật so với thiết kế gốc):
> - `content` lưu **Markdown** (không phải HTML). FE render bằng `react-markdown` + `remark-gfm`, có **`rehype-raw` + `rehype-sanitize`** (allowlist chặt: chỉ thêm vài thẻ inline `u/ins/mark/sub/sup`, chặn script/onevent/`javascript:`) ⇒ vẫn an toàn XSS; backend **không cần** HTML sanitizer. Editor là kiểu GitHub-PR (toolbar đậm/nghiêng/gạch ngang/gạch chân/heading/list/quote/code/link + Write/Preview).
> - `subject_id` để **nullable + SET NULL** và **tùy chọn lúc tạo** (không có môn học vẫn tạo được — vì Admin có thể chưa seed môn học nào). Khi *có* chọn subject thì handler kiểm tra subject phải active (BR-3-01 nới lỏng).
> - `updated_at` được set bởi `AuditableEntityInterceptor` + `DEFAULT now()` (giống `classes`), **không dùng DB trigger**.
> - Tìm kiếm keyword: **ILIKE `%term%` trên `lower(content)`** + index GIN `pg_trgm` (xem Full-Text Search bên dưới) — chọn thay cho tsvector để EF translate đơn giản và hỗ trợ substring.
> - Migration: `20260615180014_create_questions_and_views` (tạo 3 bảng + extension `pg_trgm` + index `idx_questions_content_trgm` + view `vw_questions`).
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

### Tìm kiếm trong Question Bank — **đã triển khai: ILIKE + pg_trgm GIN**

MVP-3 dùng substring ILIKE trên `lower(content)` với một functional GIN trigram index, để (a) cho
phép tìm **substring** (UX tốt hơn so với tsvector vốn match theo từ), và (b) biểu diễn được bằng
LINQ thuần (`lower(content) LIKE '%term%'`) — Application layer **không** phụ thuộc EF/Npgsql.

```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX idx_questions_content_trgm ON questions USING gin (lower(content) gin_trgm_ops);
-- Query: WHERE lower(content) LIKE '%' || lower(:keyword) || '%'   (index-backed)
```

> tsvector (`to_tsvector` + GIN) vẫn là hướng nâng cấp nếu cần ranking/ngôn ngữ; chưa cần ở MVP-3.

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

## Read model — view `vw_questions`

List/detail reads dùng một read-model view (giống `vw_classes`) để tái sử dụng `BaseGetQueryHandler`
mà không phải JOIN tới Identity `ApplicationUser`. Một dòng / mỗi question chưa bị xóa, kèm tên
teacher, subject (live, `deleted_at IS NULL`), số option, và mảng tags đã gộp.

```sql
CREATE VIEW vw_questions AS
SELECT q.id, q.public_id, q.type, q.content, q.difficulty, q.suggested_point, q.visibility,
       q.explanation, q.created_at, q.updated_at,
       q.subject_id, sub.public_id AS subject_public_id, sub.name AS subject_name,
       q.teacher_id, t.public_id AS teacher_public_id, t.display_name AS teacher_name,
       COALESCE(oc.option_count, 0) AS option_count,
       COALESCE(tg.tags, '{}'::text[]) AS tags
FROM questions q
JOIN users t ON t.id = q.teacher_id
LEFT JOIN subjects sub ON sub.id = q.subject_id AND sub.deleted_at IS NULL
LEFT JOIN (SELECT question_id, COUNT(*) AS option_count FROM question_options GROUP BY question_id) oc
       ON oc.question_id = q.id
LEFT JOIN (SELECT question_id, array_agg(tag ORDER BY tag) AS tags FROM question_tags GROUP BY question_id) tg
       ON tg.question_id = q.id
WHERE q.deleted_at IS NULL;
```

`tags` là cột `text[]` → map sang `List<string>` (lọc tag bằng `Tags.Contains(slug)` ⇒ `slug = ANY(tags)`).

---

## Open Questions — đã chốt ở MVP-3

- [x] `content` format → **Markdown** + editor kiểu GitHub-PR; render ở FE qua `rehype-raw` + `rehype-sanitize` (HTML subset an toàn), không sanitize phía server.
- [x] `subject_id` → **tùy chọn** lúc tạo (không bắt buộc chọn môn học); active-check chỉ khi có chọn.
- [ ] Hình ảnh trong câu hỏi: hiện chèn bằng Markdown image (URL external). Upload riêng (`question_attachments`) để dành phase sau.
- [x] `TrueFalse` options → app **tự seed** "True"/"False"; teacher chỉ chọn đáp án đúng.
- [x] Full-text search → ILIKE + `pg_trgm` GIN trên `lower(content)` (xem trên); tsvector để dành.
- [x] Giới hạn tags/câu hỏi → **max 10** (validate ở app, normalize `^[a-z0-9-]{1,50}$`).
