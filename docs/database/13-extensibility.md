# 13 — Database Extensibility

> Mapping từ [ROADMAP-FUTURE.md](../mvp/ROADMAP-FUTURE.md) sang database changes.
> Mục tiêu: chứng minh schema hiện tại **không block** việc thêm features trong tương lai.

---

## Design Philosophy

Schema production tốt phải thỏa mãn 2 điều kiện:
1. **Đủ cho hôm nay**: không over-engineer, không premature abstraction
2. **Không khoá cửa tương lai**: thêm feature sau mà không cần viết lại schema hiện tại

File này liệt kê từng nhóm feature future và database changes cần thiết. Nếu **"Ảnh hưởng schema hiện tại = Không"** → thiết kế đang tốt.

---

## 1. OAuth Social Login (Google / Facebook / Zalo)

### Bảng mới cần thêm

```sql
external_identities
├── id                BIGINT IDENTITY PK
├── user_id           BIGINT NOT NULL FK → users(id) CASCADE
├── provider          TEXT NOT NULL CHECK (provider IN ('google', 'facebook', 'zalo'))
├── external_id       TEXT NOT NULL        -- Provider's user ID
├── external_email    TEXT                 -- Email từ provider (informational)
├── access_token_hint TEXT                 -- Masked/hashed, audit only
├── linked_at         TIMESTAMPTZ NOT NULL DEFAULT NOW()
└── UNIQUE (provider, external_id)
```

### Thay đổi schema hiện tại

- `users.password_hash` đã là **nullable** (`YES`) → OAuth users có `password_hash = NULL`. Đã chuẩn bị.
- Không thay đổi bảng nào khác.

**Impact: Minimal** ✅

---

## 2. Mobile App (React Native / Flutter)

### Bảng mới cần thêm

```sql
device_push_tokens
├── id          BIGINT IDENTITY PK
├── user_id     BIGINT NOT NULL FK → users(id) CASCADE
├── fcm_token   TEXT NOT NULL       -- Firebase Cloud Messaging token
├── platform    TEXT NOT NULL CHECK (platform IN ('ios', 'android'))
├── app_version TEXT
├── is_active   BOOLEAN NOT NULL DEFAULT true
├── created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
├── updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
└── UNIQUE (user_id, fcm_token)
```

### Thay đổi schema hiện tại

- `notifications` table đã có `payload JSONB` → thêm push notification metadata vào đó khi cần.
- Có thể thêm `notifications.push_sent_at TIMESTAMPTZ NULL` để track delivery.
- Không thay đổi logic notification core.

**Impact: Minimal** ✅

---

## 3. AI Features

### 3.1 AI Question Generation

```sql
ai_generation_jobs
├── id              BIGINT IDENTITY PK
├── public_id       UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE
├── teacher_id      BIGINT NOT NULL FK → users(id) RESTRICT
├── subject_id      BIGINT         FK → subjects(id) SET NULL
├── prompt          TEXT NOT NULL                    -- Input prompt
├── model_used      TEXT                             -- e.g., 'gpt-4o', 'gemini-1.5'
├── status          TEXT NOT NULL DEFAULT 'Pending'
│                   CHECK (status IN ('Pending', 'Processing', 'Completed', 'Failed'))
├── result_count    INT                              -- Số câu hỏi generated
├── tokens_used     INT                              -- Cost tracking
├── error_message   TEXT
├── created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
├── completed_at    TIMESTAMPTZ
```

Thêm vào `questions` table:
```sql
ALTER TABLE questions ADD COLUMN generated_by_ai BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE questions ADD COLUMN ai_job_id BIGINT REFERENCES ai_generation_jobs(id) SET NULL;
```

**Impact: None trên core schema** ✅

### 3.2 AI Grading Suggestions

```sql
ai_grading_suggestions
├── id                      BIGINT IDENTITY PK
├── attempt_id              BIGINT NOT NULL FK → attempts(id) CASCADE
├── snapshot_question_id    BIGINT NOT NULL FK → snapshot_questions(id) RESTRICT
├── suggested_score         NUMERIC(8,2)
├── reasoning               TEXT
├── confidence              NUMERIC(3,2)   -- 0.00–1.00
├── model_used              TEXT
├── created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
└── UNIQUE (attempt_id, snapshot_question_id)
```

`manual_grades` không thay đổi — Teacher vẫn nhập điểm cuối, có thể tham khảo AI suggestion.

**Impact: None trên core schema** ✅

---

## 4. Gamification

### Bảng mới cần thêm

```sql
-- XP events (append-only log)
user_xp_events
├── id          BIGINT IDENTITY PK
├── user_id     BIGINT NOT NULL FK → users(id) RESTRICT
├── event_type  TEXT NOT NULL   -- 'submission', 'perfect_score', 'streak', 'first_join'
├── xp_earned   INT NOT NULL CHECK (xp_earned > 0)
├── reference_type TEXT         -- 'Attempt', 'Class', etc.
├── reference_id   BIGINT
├── created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()

-- Aggregated XP per user (denormalized cache)
user_xp_totals
├── user_id     BIGINT PK FK → users(id) CASCADE
├── total_xp    INT NOT NULL DEFAULT 0
├── level       INT NOT NULL DEFAULT 1
├── updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()

-- Badge definitions
badges
├── id          BIGINT IDENTITY PK
├── slug        TEXT NOT NULL UNIQUE   -- 'first_submission', 'perfect_score_5', etc.
├── name        TEXT NOT NULL
├── description TEXT
├── icon_url    TEXT
├── xp_reward   INT NOT NULL DEFAULT 0

-- User earned badges
user_badges
├── user_id     BIGINT NOT NULL FK → users(id) CASCADE
├── badge_id    BIGINT NOT NULL FK → badges(id) RESTRICT
├── earned_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
└── PRIMARY KEY (user_id, badge_id)

-- Daily streak tracking
user_streaks
├── user_id          BIGINT PK FK → users(id) CASCADE
├── current_streak   INT NOT NULL DEFAULT 0
├── longest_streak   INT NOT NULL DEFAULT 0
├── last_activity_at TIMESTAMPTZ
├── updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
```

**Impact: None trên core schema** ✅

---

## 5. Contest Module

> **Critical design note**: Contest là **tính năng riêng biệt**, KHÔNG reuse `assignments` hay `attempts`. Lý do: contest có leaderboard public, anonymous mode, multiple rounds, prize tracking — hoàn toàn khác Assignment trong lớp học.

### Bảng mới cần thêm

```sql
contests
├── id              BIGINT IDENTITY PK
├── public_id       UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE
├── creator_id      BIGINT NOT NULL FK → users(id) RESTRICT   -- Teacher hoặc Admin
├── exam_id         BIGINT         FK → exams(id) RESTRICT    -- Reuse Exam template
├── title           TEXT NOT NULL
├── description     TEXT
├── starts_at       TIMESTAMPTZ NOT NULL
├── ends_at         TIMESTAMPTZ NOT NULL
├── max_participants INT
├── is_public       BOOLEAN NOT NULL DEFAULT true
├── status          TEXT NOT NULL DEFAULT 'Draft'
│                   CHECK (status IN ('Draft', 'Open', 'Closed', 'Archived'))
├── created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
├── updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
├── deleted_at      TIMESTAMPTZ

contest_registrations
├── id              BIGINT IDENTITY PK
├── contest_id      BIGINT NOT NULL FK → contests(id) RESTRICT
├── user_id         BIGINT NOT NULL FK → users(id) RESTRICT
├── registered_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
└── UNIQUE (contest_id, user_id)

contest_attempts
├── id              BIGINT IDENTITY PK
├── contest_id      BIGINT NOT NULL FK → contests(id) RESTRICT
├── user_id         BIGINT NOT NULL FK → users(id) RESTRICT
├── snapshot_id     BIGINT NOT NULL FK → assignment_snapshots(id) RESTRICT  -- Reuse snapshot
├── status          TEXT NOT NULL ...
├── started_at      TIMESTAMPTZ NOT NULL
├── submitted_at    TIMESTAMPTZ
├── total_score     NUMERIC(8,2)
├── rank            INT  -- Computed after contest ends
└── UNIQUE (contest_id, user_id)
```

**Reuse từ core schema:**
- `exams` (exam template)
- `assignment_snapshots` pattern → tạo snapshot riêng cho contest khi open

**Impact: None trên core schema** ✅

---

## 6. Advanced Question Types

### Fill-in-the-Blank

```sql
-- Thêm bảng riêng, không phá question_options
question_blanks
├── id              BIGINT IDENTITY PK
├── question_id     BIGINT NOT NULL FK → questions(id) CASCADE
├── position        INT NOT NULL     -- Vị trí blank trong content (1-based)
├── accepted_answers JSONB NOT NULL  -- Array of accepted strings
├── case_sensitive  BOOLEAN NOT NULL DEFAULT false
├── trim_whitespace BOOLEAN NOT NULL DEFAULT true
└── UNIQUE (question_id, position)

-- Tương ứng trong snapshot:
snapshot_blanks
├── id                    BIGINT IDENTITY PK
├── snapshot_question_id  BIGINT NOT NULL FK → snapshot_questions(id) RESTRICT
├── position              INT NOT NULL
├── accepted_answers      JSONB NOT NULL
├── case_sensitive        BOOLEAN NOT NULL DEFAULT false
└── UNIQUE (snapshot_question_id, position)
```

Thêm vào `questions.type` CHECK: thêm `'FillInTheBlank'` (backward-compatible ALTER constraint).

**Impact: Nhỏ — chỉ thêm bảng mới và ALTER CHECK constraint** ✅

---

## 7. B2B Multi-Tenant (Organizations)

Đây là **thay đổi lớn nhất** trong extensibility plan. Thiết kế hiện tại đã chuẩn bị.

### Bảng mới cần thêm

```sql
organizations
├── id          BIGINT IDENTITY PK
├── public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE
├── name        TEXT NOT NULL
├── slug        TEXT NOT NULL UNIQUE   -- URL-friendly name
├── plan_id     BIGINT FK → plans(id)  -- Organization-level plan
├── owner_id    BIGINT FK → users(id) RESTRICT
├── created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
├── updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
├── deleted_at  TIMESTAMPTZ

organization_members
├── organization_id   BIGINT NOT NULL FK → organizations(id) RESTRICT
├── user_id           BIGINT NOT NULL FK → users(id) RESTRICT
├── role              TEXT NOT NULL CHECK (role IN ('Owner', 'Admin', 'Member'))
├── joined_at         TIMESTAMPTZ NOT NULL DEFAULT NOW()
└── PRIMARY KEY (organization_id, user_id)
```

### Schema Changes cho core tables

Thêm `organization_id` vào các bảng chính:

```sql
ALTER TABLE users ADD COLUMN organization_id BIGINT REFERENCES organizations(id) SET NULL;
ALTER TABLE classes ADD COLUMN organization_id BIGINT REFERENCES organizations(id) SET NULL;
ALTER TABLE questions ADD COLUMN organization_id BIGINT REFERENCES organizations(id) SET NULL;
ALTER TABLE exams ADD COLUMN organization_id BIGINT REFERENCES organizations(id) SET NULL;
```

Đổi unique constraints:

```sql
-- Email unique per organization (thay vì global)
DROP INDEX uq_users_email_active;
CREATE UNIQUE INDEX uq_users_email_org ON users (email, COALESCE(organization_id, 0))
WHERE deleted_at IS NULL;
```

**Chuẩn bị hiện tại:**
- Tất cả unique constraints dùng partial index → dễ convert sang composite với `organization_id`
- Không hard-code global uniqueness ở business logic
- `system_settings` có thể override per-organization (thêm `organization_id` FK sau)

**Impact: Additive — thêm column nullable, không phá data hiện tại** ✅

---

## 8. Advanced Payment (Stripe, Coupon)

### Coupon/Discount

```sql
coupons
├── id              BIGINT IDENTITY PK
├── code            TEXT NOT NULL UNIQUE
├── discount_type   TEXT NOT NULL CHECK (discount_type IN ('percent', 'fixed_vnd'))
├── discount_value  NUMERIC(10,2) NOT NULL
├── max_uses        INT     -- NULL = unlimited
├── used_count      INT NOT NULL DEFAULT 0
├── expires_at      TIMESTAMPTZ
├── applicable_plan_id BIGINT FK → plans(id)  -- NULL = all plans
├── created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
├── is_active       BOOLEAN NOT NULL DEFAULT true
```

Thêm vào `payments`:
```sql
ALTER TABLE payments ADD COLUMN coupon_id BIGINT REFERENCES coupons(id) SET NULL;
ALTER TABLE payments ADD COLUMN discount_amount_vnd BIGINT NOT NULL DEFAULT 0;
```

### Stripe Integration

```sql
-- Thêm payment method storage (PCI: chỉ lưu Stripe card ID, không raw number)
payment_methods
├── id                BIGINT IDENTITY PK
├── user_id           BIGINT NOT NULL FK → users(id) CASCADE
├── provider          TEXT NOT NULL CHECK (provider IN ('stripe', 'momo', 'vnpay'))
├── external_id       TEXT NOT NULL   -- Stripe PaymentMethod ID
├── type              TEXT NOT NULL   -- 'card', 'bank_transfer'
├── last_four         TEXT            -- Masked
├── brand             TEXT            -- 'visa', 'mastercard'
├── is_default        BOOLEAN NOT NULL DEFAULT false
├── created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
```

`payments` table đã có `provider TEXT` → thêm `'stripe'` vào CHECK constraint (ALTER).

**Impact: Additive** ✅

---

## 9. LMS Integration (Google Classroom, SCORM)

```sql
-- Track external sync jobs
lms_sync_jobs
├── id              BIGINT IDENTITY PK
├── class_id        BIGINT FK → classes(id) SET NULL
├── provider        TEXT NOT NULL CHECK (provider IN ('google_classroom', 'canvas', 'moodle'))
├── external_id     TEXT              -- External class/course ID
├── status          TEXT NOT NULL CHECK (status IN ('Pending', 'Running', 'Completed', 'Failed'))
├── last_synced_at  TIMESTAMPTZ
├── error_log       JSONB
├── created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()

-- External user mapping
external_user_mappings
├── user_id         BIGINT NOT NULL FK → users(id) CASCADE
├── provider        TEXT NOT NULL
├── external_id     TEXT NOT NULL
└── PRIMARY KEY (user_id, provider)
```

**Impact: None trên core schema** ✅

---

## Summary: Impact Matrix

| Feature | New Tables | Schema Changes | Impact |
|---|---|---|---|
| OAuth Social Login | `external_identities` | None (users.password_hash already nullable) | ✅ Minimal |
| Mobile App | `device_push_tokens` | Optional: `notifications.push_sent_at` | ✅ Minimal |
| AI Question Gen | `ai_generation_jobs` | Add 2 columns to `questions` | ✅ Minimal |
| AI Grading | `ai_grading_suggestions` | None | ✅ None |
| Gamification | 5 new tables | None | ✅ None |
| Contest Module | 3 new tables | None (reuse snapshots) | ✅ None |
| Fill-in-the-Blank | `question_blanks`, `snapshot_blanks` | ALTER CHECK on questions.type | ✅ Small |
| B2B Multi-tenant | `organizations`, `organization_members` | Add nullable `organization_id` to 4 tables; update unique indexes | ⚠️ Medium (but safe additive) |
| Coupon/Discount | `coupons` | Add 2 columns to `payments` | ✅ Minimal |
| Stripe | `payment_methods` | ALTER CHECK on payments.provider | ✅ Minimal |
| LMS Integration | `lms_sync_jobs`, `external_user_mappings` | None | ✅ None |

---

## Schema Lock Checklist (Pre-production Validation)

Trước khi go-live, verify schema không khoá các extension points:

- [ ] `users.password_hash` là nullable → OAuth support OK
- [ ] Unique indexes là partial (WHERE deleted_at IS NULL) → B2B scope OK
- [ ] `payments.provider` CHECK có thể ALTER để thêm 'stripe' → Stripe OK
- [ ] `questions.type` CHECK có thể ALTER để thêm 'FillInTheBlank' → Question types OK
- [ ] Không hard-code `teacher_id`/`student_id` assumptions vào DB constraints → Role flexibility OK
- [ ] `assignment_snapshots` pattern tách biệt → Contest reuse OK
- [ ] `notifications.payload JSONB` → Push notification data OK
- [ ] `audit_logs.metadata JSONB` → Any new action metadata OK
