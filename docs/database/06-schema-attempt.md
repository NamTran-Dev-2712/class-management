# 06 — Schema: Attempt & Answer

> MVP liên quan: [MVP-5 — Assignment & Online Testing](../mvp/MVP-5.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [05-schema-assignment-snapshot.md](./05-schema-assignment-snapshot.md)

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `attempts` | Một lần Student thực hiện Assignment |
| `attempt_answers` | Câu trả lời cho từng câu hỏi trong Attempt |

---

## Table: `attempts`

**Purpose:** Mỗi lần Student bấm "Start" tạo ra 1 Attempt. Lưu trữ toàn bộ lifecycle làm bài: bắt đầu, auto-save, nộp, chấm.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `assignment_id` | `BIGINT` | NO | — | FK → assignments(id) RESTRICT | |
| `student_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | |
| `attempt_number` | `INT` | NO | — | CHECK (attempt_number >= 1) | Lần thứ mấy (1-based) trong Assignment này |
| `status` | `TEXT` | NO | `'InProgress'` | CHECK (status IN ('InProgress', 'Submitted', 'AutoGraded', 'NeedManualGrading', 'Graded')) | State machine |
| `started_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Khi Student bấm Start |
| `submitted_at` | `TIMESTAMPTZ` | YES | NULL | CHECK (submitted_at >= started_at) | Khi nộp bài (thủ công hoặc auto) |
| `deadline_at` | `TIMESTAMPTZ` | YES | NULL | — | started_at + time_limit_minutes (denormalized để query nhanh) |
| `auto_submitted` | `BOOLEAN` | NO | `false` | — | TRUE nếu hệ thống auto-submit (timeout hoặc assignment closed) |
| `question_order` | `JSONB` | NO | `'[]'` | — | Mảng snapshot_question_id theo thứ tự hiển thị cho attempt này (shuffle result) |
| `total_auto_score` | `NUMERIC(8,2)` | YES | NULL | CHECK (total_auto_score >= 0) | Tổng điểm câu khách quan (tính sau submit) |
| `total_manual_score` | `NUMERIC(8,2)` | YES | NULL | CHECK (total_manual_score >= 0) | Tổng điểm câu tự luận (tính sau chấm thủ công) |
| `total_score` | `NUMERIC(8,2)` | YES | NULL | CHECK (total_score >= 0) | = total_auto_score + total_manual_score (denormalized) |
| `ip_address` | `TEXT` | YES | NULL | — | IP khi start attempt (audit) |
| `user_agent` | `TEXT` | YES | NULL | — | Browser/device info (audit) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | = started_at |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |

### Unique Constraints
```sql
-- Sequence number per (student, assignment)
CREATE UNIQUE INDEX uq_attempts_sequence
  ON attempts (assignment_id, student_id, attempt_number);

-- Chỉ 1 InProgress per (student, assignment) tại 1 thời điểm
CREATE UNIQUE INDEX uq_attempts_one_in_progress
  ON attempts (assignment_id, student_id)
  WHERE status = 'InProgress';
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `assignment_id` | `assignments(id)` | RESTRICT | Không xóa Assignment khi còn Attempt |
| `student_id` | `users(id)` | RESTRICT | Không xóa User khi còn Attempt |

### Indexes
```
pk_attempts                         PRIMARY KEY (id)
uq_attempts_public_id               UNIQUE (public_id)
uq_attempts_sequence                UNIQUE (assignment_id, student_id, attempt_number)
uq_attempts_one_in_progress         UNIQUE (assignment_id, student_id) WHERE status = 'InProgress'
idx_attempts_assignment_status      (assignment_id, status)              -- Teacher xem submissions
idx_attempts_student_assignment     (student_id, assignment_id)          -- Student xem attempts của mình
idx_attempts_deadline               (deadline_at) WHERE status = 'InProgress'   -- Background job: auto-submit
idx_attempts_need_grading           (assignment_id) WHERE status = 'NeedManualGrading'  -- Teacher pending grading
```

### State Machine
```
InProgress
  ├──[Student Submit]────────────────────────────────► Submitted
  ├──[time_limit expired (deadline_at < NOW())]───────► Submitted (auto_submitted=true)
  └──[Assignment Closed (all InProgress auto-submit)]──► Submitted (auto_submitted=true)

Submitted
  ├──[All questions are objective]──────► AutoGraded
  └──[Has subjective questions]─────────► NeedManualGrading

NeedManualGrading ──[Teacher grades all subjective]──► Graded
AutoGraded ──────────────────────────────────────────► (terminal state, unless teacher overrides)
```

### `question_order` JSONB Format
```json
[
  { "snapshot_question_id": 101, "display_position": 1 },
  { "snapshot_question_id": 98,  "display_position": 2 },
  { "snapshot_question_id": 105, "display_position": 3 }
]
```
- Lưu kết quả shuffle tại thời điểm Start Attempt
- Không thay đổi sau khi Start (ensure consistent ordering trong suốt bài làm)
- Dùng để render câu hỏi đúng thứ tự ngay cả khi Student reload

### `deadline_at` Calculation
```
deadline_at =
  IF time_limit_minutes IS NOT NULL:
    started_at + INTERVAL '${time_limit_minutes} minutes'
    (capped at closes_at nếu closes_at IS NOT NULL)
  ELSE IF closes_at IS NOT NULL:
    closes_at
  ELSE:
    NULL (không giới hạn)
```

### Notes
- `total_score` là denormalized cache — recalculate khi `manual_grades` thay đổi (trigger hoặc app)
- `deadline_at` denormalized để background job query `WHERE deadline_at <= NOW()` hiệu quả (không phải JOIN với assignments)
- Partial unique index `uq_attempts_one_in_progress`: chặn Student start 2 bài cùng lúc ở DB level

---

## Table: `attempt_answers`

**Purpose:** Lưu câu trả lời của Student cho từng câu hỏi. Được upsert mỗi lần auto-save hoặc khi submit.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `attempt_id` | `BIGINT` | NO | — | FK → attempts(id) CASCADE | |
| `snapshot_question_id` | `BIGINT` | NO | — | FK → snapshot_questions(id) RESTRICT | |
| `selected_option_ids` | `JSONB` | YES | NULL | — | Array của snapshot_option_id. NULL nếu chưa trả lời hoặc câu tự luận |
| `text_answer` | `TEXT` | YES | NULL | CHECK (length(text_answer) <= 50000) | Câu trả lời tự luận. NULL nếu câu choice |
| `auto_score` | `NUMERIC(8,2)` | YES | NULL | CHECK (auto_score >= 0) | Điểm tự động chấm. NULL trước khi submit/grade |
| `is_auto_graded` | `BOOLEAN` | NO | `false` | — | TRUE sau khi đã auto-grade |
| `last_saved_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Lần cuối auto-save hoặc submit |
| `submitted_at` | `TIMESTAMPTZ` | YES | NULL | — | Thời điểm cuối cùng khi attempt được submit (denorm từ attempts) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |

### Unique Constraints
```sql
-- 1 câu trả lời per (attempt, câu hỏi)
CREATE UNIQUE INDEX uq_attempt_answers_unique
  ON attempt_answers (attempt_id, snapshot_question_id);
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `attempt_id` | `attempts(id)` | CASCADE | Xóa attempt → xóa hết answers |
| `snapshot_question_id` | `snapshot_questions(id)` | RESTRICT | Snapshot không được xóa khi còn answers |

### Indexes
```
pk_attempt_answers                      PRIMARY KEY (id)
uq_attempt_answers_unique               UNIQUE (attempt_id, snapshot_question_id)
idx_attempt_answers_attempt             (attempt_id)                                -- Load tất cả answers của attempt
idx_attempt_answers_not_graded          (attempt_id) WHERE is_auto_graded = false AND submitted_at IS NOT NULL  -- Auto-grade queue
```

### `selected_option_ids` JSONB Format
```json
-- SingleChoice: 1 option
[snapshot_option_id]

-- MultipleChoice: nhiều options
[snapshot_option_id_1, snapshot_option_id_2]

-- TrueFalse: 1 option (True hoặc False)
[snapshot_option_id]

-- Chưa trả lời
null
```

### Auto-grading Logic

```
Khi Attempt được Submit:
  FOR EACH attempt_answer WHERE snapshot_question.type IN ('SingleChoice', 'MultipleChoice', 'TrueFalse'):
    correct_option_ids = snapshot_options WHERE snapshot_question_id = X AND is_correct = true
    student_option_ids = attempt_answer.selected_option_ids

    IF type = 'SingleChoice' OR type = 'TrueFalse':
      score = snapshot_question.point IF student_option_ids = [correct_option_id] ELSE 0

    IF type = 'MultipleChoice':
      -- All-or-nothing (MVP-5)
      score = snapshot_question.point IF SET(student_option_ids) = SET(correct_option_ids) ELSE 0

    UPDATE attempt_answers SET auto_score = score, is_auto_graded = true

  UPDATE attempts SET
    total_auto_score = SUM(auto_score),
    status = IF has_subjective_questions THEN 'NeedManualGrading' ELSE 'AutoGraded',
    total_score = total_auto_score (if AutoGraded)
```

### Auto-save Pattern

```
Client auto-save every 30s:
  UPSERT attempt_answers
    ON CONFLICT (attempt_id, snapshot_question_id)
    DO UPDATE SET
      selected_option_ids = EXCLUDED.selected_option_ids,
      text_answer = EXCLUDED.text_answer,
      last_saved_at = NOW(),
      updated_at = NOW()
    WHERE attempts.status = 'InProgress'  -- Guard: không save nếu đã submit
```

### Notes
- `attempt_answers` là **heavy write table** — upsert mỗi 30s per câu hỏi đang làm
- CASCADE delete từ attempts: khi attempt bị xóa (trường hợp hiếm), answers cũng xóa
- `submitted_at` denormalized từ `attempts.submitted_at` để filter nhanh answers cần grading
- Không lưu thứ tự shuffle trong `attempt_answers` — thứ tự lấy từ `attempts.question_order`

---

## Relationships Diagram

```
assignments (1) ─────────────── (*) attempts
users/Student (1) ──────────── (*) attempts

attempts (1) ─────────────────── (*) attempt_answers   [CASCADE]
snapshot_questions (1) ─────── (*) attempt_answers

attempts (1) ─────────────────── (*) manual_grades     [MVP-7]
```

---

## Migration Dependencies

Phụ thuộc:
- `assignments` (từ 05)
- `assignment_snapshots`, `snapshot_questions` (từ 05)

### Migration order
1. `CREATE TABLE attempts`
2. Apply `set_updated_at` trigger cho `attempts`
3. `CREATE TABLE attempt_answers`
4. Apply `set_updated_at` trigger cho `attempt_answers`

---

## Open Questions

- [ ] **Partial score MultipleChoice**: Hiện tại all-or-nothing. Future: `score = point * (correct_selected / total_correct) - penalty * wrong_selected`? Cần thêm cột `scoring_formula TEXT` trong `snapshot_questions`
- [ ] **attempt_answers partitioning**: Khi có nhiều users, table này sẽ phình rất nhanh. Partition theo `attempt_id` range hoặc `created_at` (monthly). Khi nào cần?
- [ ] **Draft answers retention**: Auto-save lưu draft khi InProgress. Khi Attempt expire và không submit, draft answers có xóa không? Đề xuất: giữ lại (debug + evidence)
- [ ] **Reconnect support**: Student mất mạng → reconnect → `GET /attempts/{id}` trả về current draft. Server validate deadline_at và trả về remaining time. OK với schema hiện tại.
