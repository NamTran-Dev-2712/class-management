# 10 — Indexing & Performance

> Cross-cutting: áp dụng cho toàn bộ schema từ 01–09.

---

## 1. Index Design Principles

### Rule 1: Mọi FK column đều cần index
PostgreSQL **không tự động** tạo index cho FK. Phải thêm thủ công.

```sql
-- Template:
CREATE INDEX idx_<table>_<fk_col> ON <table> (<fk_col>);
```

**FK indexes cần tạo** (ngoài những đã liệt kê trong schema files):

| Table | FK Column | Index Name |
|---|---|---|
| `user_roles` | `role_id` | `idx_user_roles_role_id` |
| `refresh_tokens` | `user_id` | `idx_refresh_tokens_user_id` |
| `password_reset_tokens` | `user_id` | `idx_prt_user_id` |
| `classes` | `subject_id`, `owner_id` | `idx_classes_subject_id`, `idx_classes_owner_id` |
| `class_memberships` | `class_id`, `student_id`, `processed_by` | listed |
| `questions` | `teacher_id`, `subject_id` | listed |
| `question_options` | `question_id` | listed |
| `question_tags` | `question_id` | listed |
| `exams` | `teacher_id`, `subject_id` | listed |
| `exam_questions` | `exam_id`, `question_id` | listed |
| `assignments` | `exam_id`, `class_id`, `teacher_id` | listed |
| `snapshot_questions` | `snapshot_id`, `original_question_id` | listed |
| `snapshot_options` | `snapshot_question_id`, `original_option_id` | listed |
| `attempts` | `assignment_id`, `student_id` | listed |
| `attempt_answers` | `attempt_id`, `snapshot_question_id` | listed |
| `manual_grades` | `attempt_id`, `graded_by` | listed |
| `reports` | `reporter_id`, `admin_id` | listed |
| `notifications` | `user_id` | listed |
| `subscriptions` | `teacher_id`, `plan_id` | listed |
| `payments` | `teacher_id`, `plan_id` | listed |
| `invoices` | `teacher_id` | listed |

### Rule 2: Composite index — Left-most matching
```sql
-- Index (a, b, c) có thể serve queries:
-- WHERE a = ?
-- WHERE a = ? AND b = ?
-- WHERE a = ? AND b = ? AND c = ?
-- KHÔNG serve: WHERE b = ? hoặc WHERE c = ?

-- Ví dụ:
CREATE INDEX idx_assignments_class_status ON assignments (class_id, status);
-- Serves: WHERE class_id = ? (left-most)
-- Serves: WHERE class_id = ? AND status = ?
-- Không serves: WHERE status = ? (chỉ mình)
```

### Rule 3: Partial index cho booleans & enums
```sql
-- Tốt hơn full index khi chỉ query 1 value của low-cardinality column
CREATE INDEX idx_users_locked ON users (id) WHERE is_locked = true;
CREATE INDEX idx_assignments_open ON assignments (class_id, opens_at) WHERE status = 'Open';
```

### Rule 4: Index covering (include) cho hot read queries
```sql
-- Tránh heap fetch bằng covering index (PostgreSQL: INCLUDE clause)
CREATE INDEX idx_attempts_assignment_student
  ON attempts (assignment_id, student_id)
  INCLUDE (status, total_score, submitted_at);
-- Query: SELECT status, total_score, submitted_at WHERE assignment_id = ? AND student_id = ?
-- → Index-only scan, không touch table
```

---

## 2. Hot Query Analysis

### Auth & Session (MVP-1)
```sql
-- [HOT] Login: lookup user by email
SELECT id, password_hash, is_locked FROM users WHERE email = $1 AND deleted_at IS NULL;
-- Index: uq_users_email_active (UNIQUE WHERE deleted_at IS NULL)

-- [HOT] Refresh token validation
SELECT user_id, expires_at, revoked_at FROM refresh_tokens WHERE token_hash = $1;
-- Index: uq_refresh_tokens_hash (UNIQUE)

-- [MEDIUM] Load active tokens for user (logout all devices)
SELECT id FROM refresh_tokens WHERE user_id = $1 AND revoked_at IS NULL;
-- Index: idx_refresh_tokens_user_active
```

### Classroom (MVP-2)
```sql
-- [HOT] Student xem lớp của mình
SELECT c.* FROM classes c
JOIN class_memberships cm ON cm.class_id = c.id
WHERE cm.student_id = $1 AND cm.status = 'Approved' AND c.deleted_at IS NULL;
-- Index: idx_memberships_student_approved

-- [MEDIUM] Teacher xem pending requests
SELECT * FROM class_memberships
WHERE class_id = $1 AND status = 'Pending'
ORDER BY created_at;
-- Index: idx_memberships_class_status

-- [LOW] Lookup class by invite code
SELECT id, name, status FROM classes WHERE invite_code = $1;
-- Index: uq_classes_invite_code
```

### Question Bank (MVP-3)
```sql
-- [HOT] Load teacher's questions with filters
SELECT * FROM questions
WHERE teacher_id = $1 AND deleted_at IS NULL
  AND ($2::text IS NULL OR type = $2)
  AND ($3::text IS NULL OR difficulty = $3)
  AND ($4::text IS NULL OR visibility = $4)
ORDER BY created_at DESC
LIMIT 20 OFFSET $5;
-- Index: idx_questions_teacher_type (teacher_id, type, difficulty) WHERE deleted_at IS NULL

-- [MEDIUM] Search by keyword (full-text)
SELECT * FROM questions
WHERE teacher_id = $1 AND content_search @@ plainto_tsquery('simple', $2)
  AND deleted_at IS NULL;
-- Index: gin_questions_content_search

-- [MEDIUM] Questions by tag
SELECT q.* FROM questions q
JOIN question_tags qt ON qt.question_id = q.id
WHERE qt.tag = $1 AND q.teacher_id = $2 AND q.deleted_at IS NULL;
-- Index: idx_question_tags_tag + idx_question_tags_question
```

### Assignment (MVP-5)
```sql
-- [HOT] Student xem assignments của lớp đang open
SELECT a.* FROM assignments a
JOIN class_memberships cm ON cm.class_id = a.class_id
WHERE cm.student_id = $1 AND cm.status = 'Approved'
  AND a.status = 'Open'
ORDER BY a.closes_at ASC NULLS LAST;
-- Index: idx_assignments_class_open + idx_memberships_student_approved

-- [HOT] Background job: Scheduled → Open
SELECT id FROM assignments
WHERE status = 'Scheduled' AND opens_at <= NOW();
-- Index: idx_assignments_scheduled

-- [HOT] Background job: Open → Closed (auto-close)
SELECT id FROM assignments
WHERE status = 'Open' AND closes_at <= NOW();
-- Index: idx_assignments_closes_at

-- [HOT] Load snapshot for attempt
SELECT sq.*, so.*
FROM snapshot_questions sq
LEFT JOIN snapshot_options so ON so.snapshot_question_id = sq.id
WHERE sq.snapshot_id = $1
ORDER BY sq.display_order, so.display_order;
-- Index: idx_snapshot_questions_snapshot_order + idx_snapshot_options_question_order
```

### Attempt (MVP-5)
```sql
-- [HOT] Student reload bài đang làm
SELECT a.*, aa.*
FROM attempts a
JOIN attempt_answers aa ON aa.attempt_id = a.id
WHERE a.id = $1 AND a.student_id = $2;
-- Index: idx_attempt_answers_attempt

-- [HOT] Background job: auto-submit expired attempts
SELECT id, assignment_id, student_id FROM attempts
WHERE status = 'InProgress' AND deadline_at <= NOW();
-- Index: idx_attempts_deadline

-- [MEDIUM] Teacher xem submissions
SELECT a.*, u.display_name, u.email
FROM attempts a
JOIN users u ON u.id = a.student_id
WHERE a.assignment_id = $1
ORDER BY a.submitted_at DESC NULLS LAST;
-- Index: idx_attempts_assignment_status
```

### Grading (MVP-6)
```sql
-- [MEDIUM] Teacher xem bài cần chấm
SELECT a.id, a.public_id, u.display_name, a.started_at, a.submitted_at
FROM attempts a
JOIN users u ON u.id = a.student_id
WHERE a.assignment_id = $1 AND a.status = 'NeedManualGrading'
ORDER BY a.submitted_at;
-- Index: idx_attempts_need_grading

-- [MEDIUM] Load grades của attempt
SELECT mg.*, sq.content, sq.point
FROM manual_grades mg
JOIN snapshot_questions sq ON sq.id = mg.snapshot_question_id
WHERE mg.attempt_id = $1;
-- Index: idx_manual_grades_attempt
```

### Admin (MVP-7)
```sql
-- [MEDIUM] Admin xem audit log
SELECT * FROM audit_logs
WHERE actor_id = $1 OR target_id = $1
ORDER BY created_at DESC
LIMIT 50;
-- Index: idx_audit_logs_actor_created

-- [HOT] Notification badge count
SELECT COUNT(*) FROM notifications
WHERE user_id = $1 AND status = 'Unread';
-- Index: idx_notifications_user_unread

-- [HOT] Load notifications
SELECT * FROM notifications
WHERE user_id = $1 AND status != 'Archived'
ORDER BY created_at DESC
LIMIT 20;
-- Index: idx_notifications_user_status
```

---

## 3. Partitioning Candidates

### `audit_logs` — Partition by Range (created_at)

**Khi nào**: > 10M rows hoặc table > 10GB

```sql
-- Range partition monthly
CREATE TABLE audit_logs (
  -- columns...
) PARTITION BY RANGE (created_at);

CREATE TABLE audit_logs_2026_01 PARTITION OF audit_logs
  FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');

CREATE TABLE audit_logs_2026_02 PARTITION OF audit_logs
  FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
-- Dùng pg_partman để tự động tạo partition mới
```

**Lợi ích**: Query recent logs nhanh hơn nhiều; old partitions có thể archive/detach.

### `notifications` — Partition by Range (created_at)

**Khi nào**: > 50M rows

```sql
CREATE TABLE notifications (
  -- columns...
) PARTITION BY RANGE (created_at);
-- Monthly partitions, tương tự audit_logs
```

### `attempt_answers` — Partition by Range (created_at hoặc attempt_id range)

**Khi nào**: > 100M rows (heavy write với nhiều concurrent users)

```sql
-- Partition by creation date
CREATE TABLE attempt_answers (
  -- columns...
) PARTITION BY RANGE (created_at);
```

> **Note**: Partitioning là premature optimization cho MVP. Thiết kế schema hiện tại không block việc thêm partitioning sau. Chỉ thực hiện khi có monitoring data cho thấy cần thiết.

---

## 4. Full-Text Search

### `questions.content` — Vietnamese Text

```sql
-- Generated column với tsvector
ALTER TABLE questions
  ADD COLUMN content_search TSVECTOR
  GENERATED ALWAYS AS (to_tsvector('simple', content)) STORED;
-- 'simple' dictionary: không stem, phù hợp tiếng Việt

CREATE INDEX gin_questions_content_search
  ON questions USING GIN (content_search);

-- Query
WHERE content_search @@ plainto_tsquery('simple', :keyword)
-- Hoặc prefix search:
WHERE content_search @@ to_tsquery('simple', :keyword || ':*')
```

### `question_tags` — Tag search

```sql
-- Exact match (đã có unique index)
WHERE tag = 'dai-so'

-- Prefix match
WHERE tag LIKE 'dai%'  -- Works với btree index
```

---

## 5. Connection Pooling

### PgBouncer (Recommended)

```
App Servers ──► PgBouncer ──► PostgreSQL
             (pool mode: transaction)

Config:
  pool_mode = transaction
  max_client_conn = 1000
  default_pool_size = 20  (per database)
  min_pool_size = 5
  reserve_pool_size = 5
```

**Transaction pool mode**: connection về pool sau mỗi transaction. Phù hợp với EF Core.

**Không dùng statement pool mode**: EF Core dùng prepared statements và temp tables.

---

## 6. Query Performance Targets

| Query Category | Target p50 | Target p95 | Alert at |
|---|---|---|---|
| Auth (login, token validate) | < 5ms | < 20ms | > 100ms |
| Student load assignments | < 20ms | < 50ms | > 200ms |
| Load snapshot for attempt | < 10ms | < 30ms | > 100ms |
| Auto-save answer (upsert) | < 5ms | < 20ms | > 50ms |
| Submit attempt | < 50ms | < 100ms | > 500ms |
| Teacher load submissions | < 30ms | < 100ms | > 300ms |
| Admin audit log query | < 50ms | < 200ms | > 1s |

---

## 7. Monitoring & Observability

### pg_stat_statements

```sql
-- Enable extension
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Top slow queries
SELECT
  query,
  calls,
  mean_exec_time,
  total_exec_time,
  rows
FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 20;
```

### Slow Query Log

```
# postgresql.conf
log_min_duration_statement = 100   -- Log queries > 100ms
log_statement = 'none'
log_duration = off
```

### Key Metrics to Monitor

| Metric | Warning | Critical |
|---|---|---|
| Active connections | > 80% of max | > 95% of max |
| Cache hit ratio | < 95% | < 90% |
| Dead tuples (bloat) | > 10% | > 20% |
| Index usage | < 99% index scan | < 95% |
| Replication lag (if replica) | > 1s | > 10s |

---

## 8. Vacuum & Autovacuum Tuning

Heavy write tables cần autovacuum aggressive hơn default:

```sql
-- attempt_answers: heavy write (auto-save)
ALTER TABLE attempt_answers SET (
  autovacuum_vacuum_scale_factor = 0.01,   -- Vacuum khi 1% rows dead (default: 20%)
  autovacuum_analyze_scale_factor = 0.005,
  autovacuum_vacuum_cost_delay = 2         -- ms, aggressive
);

-- notifications: bulk insert, nhiều updates
ALTER TABLE notifications SET (
  autovacuum_vacuum_scale_factor = 0.05,
  autovacuum_analyze_scale_factor = 0.02
);

-- audit_logs: append only, minimal vacuum needed
ALTER TABLE audit_logs SET (
  autovacuum_vacuum_scale_factor = 0.1,    -- Less aggressive
  autovacuum_analyze_scale_factor = 0.05
);
```

---

## 9. Index Maintenance

```sql
-- Rebuild bloated indexes (không lock table)
REINDEX INDEX CONCURRENTLY idx_<name>;

-- Check index bloat
SELECT
  schemaname,
  tablename,
  indexname,
  pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
ORDER BY pg_relation_size(indexrelid) DESC;

-- Unused indexes (review periodically)
SELECT
  schemaname,
  tablename,
  indexname,
  idx_scan  -- 0 = never used
FROM pg_stat_user_indexes
WHERE idx_scan = 0
ORDER BY pg_relation_size(indexrelid) DESC;
```
