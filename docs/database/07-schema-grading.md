# 07 — Schema: Manual Grading & Grade Release

> MVP liên quan: [MVP-6 — Manual Grading & Reports](../mvp/MVP-6.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [06-schema-attempt.md](./06-schema-attempt.md)

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `manual_grades` | Điểm và feedback của Teacher cho câu tự luận |
| `assignment_grade_releases` | Ghi nhận khi Teacher công bố điểm (manual policy) |

---

## Table: `manual_grades`

**Purpose:** Lưu điểm thủ công và feedback của Teacher cho từng câu `ShortWriting`/`LongWriting`. 1 record per (attempt, câu hỏi).

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `attempt_id` | `BIGINT` | NO | — | FK → attempts(id) CASCADE | |
| `snapshot_question_id` | `BIGINT` | NO | — | FK → snapshot_questions(id) RESTRICT | Câu hỏi được chấm |
| `score` | `NUMERIC(8,2)` | NO | — | CHECK (score >= 0) | Điểm chấm. App validate: score ≤ snapshot_question.point |
| `feedback` | `TEXT` | YES | NULL | CHECK (length(feedback) <= 5000) | Nhận xét của Teacher |
| `graded_by` | `BIGINT` | NO | — | FK → users(id) RESTRICT | Teacher chấm bài |
| `graded_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Lần chấm đầu tiên |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Cập nhật khi Teacher sửa điểm |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | Teacher sửa (có thể khác graded_by) |

### Unique Constraints
```sql
-- 1 manual grade per (attempt, câu hỏi)
CREATE UNIQUE INDEX uq_manual_grades_attempt_question
  ON manual_grades (attempt_id, snapshot_question_id);
```

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `attempt_id` | `attempts(id)` | CASCADE | Xóa attempt → xóa grades |
| `snapshot_question_id` | `snapshot_questions(id)` | RESTRICT | Snapshot không xóa khi còn grades |
| `graded_by` | `users(id)` | RESTRICT | Không xóa Teacher khi còn grades |
| `updated_by` | `users(id)` | SET NULL | Audit, nullable |

### Indexes
```
pk_manual_grades                        PRIMARY KEY (id)
uq_manual_grades_attempt_question       UNIQUE (attempt_id, snapshot_question_id)
idx_manual_grades_attempt               (attempt_id)                           -- Load tất cả grades của attempt
idx_manual_grades_graded_by             (graded_by)                            -- Audit: teacher đã chấm gì
idx_manual_grades_updated               (updated_at DESC)                      -- Audit: sửa điểm gần nhất
```

### Grade Completion Trigger

Khi tất cả câu tự luận của 1 attempt đã có manual_grade → tự động cập nhật attempt status và total_score:

```sql
CREATE OR REPLACE FUNCTION check_attempt_grading_complete()
RETURNS TRIGGER AS $$
DECLARE
  v_attempt_id BIGINT;
  v_total_manual NUMERIC;
  v_pending_count INT;
BEGIN
  v_attempt_id := NEW.attempt_id;

  -- Đếm câu tự luận chưa có grade
  SELECT COUNT(*) INTO v_pending_count
  FROM snapshot_questions sq
  LEFT JOIN manual_grades mg ON mg.snapshot_question_id = sq.id AND mg.attempt_id = v_attempt_id
  JOIN assignment_snapshots asn ON asn.id = sq.snapshot_id
  JOIN attempts a ON a.assignment_id = (SELECT assignment_id FROM assignment_snapshots WHERE id = asn.id) AND a.id = v_attempt_id
  WHERE sq.snapshot_id = (SELECT snapshot_id FROM snapshot_questions WHERE id = NEW.snapshot_question_id LIMIT 1)
    AND sq.type IN ('ShortWriting', 'LongWriting')
    AND mg.id IS NULL;

  IF v_pending_count = 0 THEN
    -- Tính tổng manual score
    SELECT COALESCE(SUM(score), 0) INTO v_total_manual
    FROM manual_grades
    WHERE attempt_id = v_attempt_id;

    -- Update attempt
    UPDATE attempts
    SET
      status = 'Graded',
      total_manual_score = v_total_manual,
      total_score = COALESCE(total_auto_score, 0) + v_total_manual,
      updated_at = NOW()
    WHERE id = v_attempt_id AND status = 'NeedManualGrading';
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_manual_grade_check_complete
  AFTER INSERT OR UPDATE ON manual_grades
  FOR EACH ROW EXECUTE FUNCTION check_attempt_grading_complete();
```

### Notes
- `score` CHECK chỉ `>= 0`. Validate `score <= snapshot_question.point` ở app layer (cần JOIN để check).
- Khi Teacher **sửa điểm** đã chấm (UPDATE manual_grades): trigger tính lại total_score của attempt. Ghi vào `audit_logs` (MVP-7).
- Unique constraint: nếu Teacher chấm lại thì UPDATE (không INSERT mới). `graded_at` giữ nguyên lần đầu, `updated_at` cập nhật.

---

## Table: `assignment_grade_releases`

**Purpose:** Ghi nhận thời điểm Teacher bấm "Công bố điểm" cho Assignment. Chỉ cần thiết với `grade_publish_policy = 'manual'`.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `assignment_id` | `BIGINT` | NO | — | FK → assignments(id) RESTRICT, UNIQUE | 1-1 với assignment |
| `released_by` | `BIGINT` | NO | — | FK → users(id) RESTRICT | Teacher đã công bố |
| `released_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `assignment_id` | `assignments(id)` | RESTRICT | |
| `released_by` | `users(id)` | RESTRICT | |

### Indexes
```
pk_assignment_grade_releases            PRIMARY KEY (id)
uq_grade_releases_assignment            UNIQUE (assignment_id)
idx_grade_releases_released_by          (released_by)
```

### Grade Visibility Logic

Đây là logic app-layer, không enforce ở DB:

```
Student có thể xem điểm của Attempt khi:

grade_publish_policy = 'immediate':
  attempt.status IN ('AutoGraded', 'Graded')

grade_publish_policy = 'after_deadline':
  attempt.status IN ('AutoGraded', 'Graded')
  AND assignments.closes_at <= NOW()

grade_publish_policy = 'manual':
  attempt.status IN ('AutoGraded', 'Graded')
  AND assignment_grade_releases.released_at IS NOT NULL  -- record exists
  (hoặc check assignments.grades_released_at IS NOT NULL)
```

> **Note**: `assignments.grades_released_at` (cột trong bảng `assignments`) và `assignment_grade_releases` table đều capture cùng thông tin. `grades_released_at` là denormalized shortcut để tránh JOIN. Chọn một trong hai hoặc dùng cả hai (với trigger sync).

---

## Score Recalculation

Khi `manual_grades` INSERT hoặc UPDATE:

```
attempts.total_score =
  COALESCE(attempts.total_auto_score, 0) +
  SUM(manual_grades.score WHERE attempt_id = this.attempt_id)
```

Kết quả cuối cùng (dùng cho báo cáo, export):
- Với `score_policy = 'highest'`: `MAX(total_score)` trong tất cả Graded/AutoGraded attempts của student
- Với `score_policy = 'latest'`: `total_score` của Attempt có `attempt_number` cao nhất trong trạng thái Graded/AutoGraded

---

## Relationships Diagram

```
attempts (1) ─────────────── (*) manual_grades   [CASCADE]
snapshot_questions (1) ─────── (*) manual_grades

assignments (1) ─────────────── (0..1) assignment_grade_releases
users/Teacher (1) ──────────── (*) manual_grades [via graded_by]
users/Teacher (1) ──────────── (*) assignment_grade_releases [via released_by]
```

---

## Migration Dependencies

Phụ thuộc:
- `attempts` (từ 06)
- `snapshot_questions` (từ 05)
- `assignments` (từ 05)
- `users` (từ 01)

### Migration order
1. `CREATE TABLE manual_grades`
2. Apply `set_updated_at` trigger cho `manual_grades`
3. Create `check_attempt_grading_complete` function + trigger
4. `CREATE TABLE assignment_grade_releases`

---

## Open Questions

- [ ] **Trigger vs App**: Trigger `check_attempt_grading_complete` có thể phức tạp khi debug. Alternative: app kiểm tra mỗi lần Teacher save manual_grade. Nên chọn app approach nếu muốn đơn giản hơn.
- [ ] **Score history**: Khi Teacher sửa điểm, hiện tại chỉ ghi audit_log. Nếu muốn hiển thị "điểm đã được sửa từ X thành Y" cho Student, cần thêm `manual_grade_history` table.
- [ ] **Partial grading**: Nếu Assignment có 3 câu tự luận và Teacher mới chấm 2, Student có xem được điểm tạm thời không? Hiện tại: không. Cần `show_partial_grade BOOLEAN` trong assignments?
