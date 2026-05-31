# 09 — Schema: Premium & Payment

> MVP liên quan: [MVP-8 — Premium & Payment](../mvp/MVP-8.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [01-schema-auth.md](./01-schema-auth.md) — cần `users`

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `plans` | Gói dịch vụ (Free, Pro) với giới hạn tài nguyên |
| `subscriptions` | Subscription của Teacher với 1 plan |
| `payments` | Giao dịch thanh toán (Momo, VNPay) |
| `invoices` | Hóa đơn tự động sau payment thành công |

---

## Table: `plans`

**Purpose:** Định nghĩa các gói dịch vụ. Seed data cố định cho MVP-8. Admin có thể thêm gói mới.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `name` | `TEXT` | NO | — | UNIQUE, CHECK (length >= 2 AND length <= 50) | Tên gói: Free, Pro |
| `billing_cycle` | `TEXT` | YES | NULL | CHECK (billing_cycle IN ('monthly', 'annual') OR billing_cycle IS NULL) | NULL cho Free plan |
| `price_vnd` | `BIGINT` | NO | `0` | CHECK (price_vnd >= 0) | Giá VND. 0 cho Free plan |
| `max_classes` | `INT` | YES | NULL | CHECK (max_classes > 0 OR max_classes IS NULL) | NULL = unlimited |
| `max_questions` | `INT` | YES | NULL | CHECK (max_questions > 0 OR max_questions IS NULL) | NULL = unlimited |
| `max_exams` | `INT` | YES | NULL | CHECK (max_exams > 0 OR max_exams IS NULL) | NULL = unlimited |
| `max_students_per_class` | `INT` | YES | NULL | — | NULL = follow system_settings |
| `is_active` | `BOOLEAN` | NO | `true` | — | Inactive = không bán nữa (existing subscriptions unaffected) |
| `display_order` | `INT` | NO | `0` | — | Thứ tự hiển thị trên pricing page |
| `features` | `JSONB` | YES | NULL | — | List feature names cho display |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |

### Seed Data

```
-- Free plan (mặc định cho tất cả Teacher)
id=1, name='Free', billing_cycle=NULL, price_vnd=0,
max_classes=10, max_questions=500, max_exams=50, is_active=true

-- Pro Monthly
id=2, name='Pro', billing_cycle='monthly', price_vnd=99000,
max_classes=NULL, max_questions=NULL, max_exams=NULL, is_active=true

-- Pro Annual
id=3, name='Pro', billing_cycle='annual', price_vnd=990000,
max_classes=NULL, max_questions=NULL, max_exams=NULL, is_active=true
```

### Notes
- Thay đổi `price_vnd` chỉ ảnh hưởng subscription mới — existing subscriptions dùng giá tại thời điểm thanh toán (locked trong `payments.amount_vnd`)
- `max_*` NULL = unlimited
- Plan giới hạn được enforce ở app layer, không DB constraint

---

## Table: `subscriptions`

**Purpose:** Theo dõi trạng thái gói của Teacher. Mỗi Teacher tối đa 1 Active subscription tại 1 thời điểm.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `teacher_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | |
| `plan_id` | `BIGINT` | NO | — | FK → plans(id) RESTRICT | |
| `status` | `TEXT` | NO | `'Active'` | CHECK (status IN ('Active', 'PastDue', 'Cancelled', 'Expired')) | |
| `started_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Ngày bắt đầu subscription |
| `expires_at` | `TIMESTAMPTZ` | YES | NULL | — | NULL cho Free plan (không expire); NOT NULL cho Pro |
| `cancelled_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi user cancel |
| `grace_period_ends_at` | `TIMESTAMPTZ` | YES | NULL | — | PastDue → Expired sau ngày này |
| `payment_type` | `TEXT` | NO | `'auto'` | CHECK (payment_type IN ('auto', 'manual')) | 'manual' = Admin set |
| `admin_note` | `TEXT` | YES | NULL | CHECK (length <= 500) | Note khi Admin set manual |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Auto-update trigger |

### Unique Constraints
```sql
-- Chỉ 1 Active subscription per Teacher
CREATE UNIQUE INDEX uq_subscriptions_teacher_active
  ON subscriptions (teacher_id)
  WHERE status = 'Active';
```

### Foreign Keys
| Column | References | On Delete |
|---|---|---|
| `teacher_id` | `users(id)` | RESTRICT |
| `plan_id` | `plans(id)` | RESTRICT |

### Indexes
```
pk_subscriptions                    PRIMARY KEY (id)
uq_subscriptions_public_id          UNIQUE (public_id)
uq_subscriptions_teacher_active     UNIQUE (teacher_id) WHERE status = 'Active'
idx_subscriptions_teacher           (teacher_id, status)                 -- Teacher xem lịch sử
idx_subscriptions_expires           (expires_at) WHERE status = 'Active' -- Background job renewal check
idx_subscriptions_past_due          (grace_period_ends_at) WHERE status = 'PastDue'  -- BG: PastDue → Expired
```

### State Machine
```
[Thanh toán thành công / Admin set manual]
    │
    ▼
  Active ──[expires_at reached, payment fails]──► PastDue
         ──[User cancels]───────────────────────► Cancelled (Active until expires_at)
  Active: (if expires_at IS NULL → Free plan, never expires automatically)

  PastDue ──[payment succeeds in grace period]──► Active
          ──[grace_period_ends_at reached]──────► Expired

  Cancelled ──[expires_at reached]──────────────► Expired (background job)
  Expired → (terminal, Teacher về Free plan)
```

### Notes
- **Free plan subscription**: Mỗi Teacher có 1 Free subscription mặc định. Không có `expires_at`.
- Khi subscription Expired: Teacher tự động quay về Free plan limits. `system_settings` cung cấp default limits.
- `grace_period_ends_at` = `expires_at + INTERVAL '3 days'` (hoặc theo config)

---

## Table: `payments`

**Purpose:** Ghi nhận mọi giao dịch thanh toán. Idempotency key đảm bảo webhook deduplication.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `subscription_id` | `BIGINT` | YES | NULL | FK → subscriptions(id) SET NULL | NULL trước khi subscription được tạo |
| `teacher_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | |
| `provider` | `TEXT` | NO | — | CHECK (provider IN ('momo', 'vnpay')) | |
| `amount_vnd` | `BIGINT` | NO | — | CHECK (amount_vnd > 0) | Số tiền thanh toán (VND) |
| `plan_id` | `BIGINT` | NO | — | FK → plans(id) RESTRICT | Plan đang mua (snapshot giá tại thời điểm này) |
| `billing_cycle` | `TEXT` | NO | — | CHECK (billing_cycle IN ('monthly', 'annual')) | |
| `status` | `TEXT` | NO | `'Pending'` | CHECK (status IN ('Pending', 'Completed', 'Failed', 'Expired')) | |
| `idempotency_key` | `TEXT` | NO | — | UNIQUE NOT NULL | **Critical**: dedup webhook. Format: `{provider}_{order_id}` |
| `provider_order_id` | `TEXT` | YES | NULL | — | Order ID từ payment provider |
| `provider_transaction_id` | `TEXT` | YES | NULL | — | Transaction ID sau khi completed |
| `provider_metadata` | `JSONB` | YES | NULL | — | Raw response từ provider (encrypted nếu cần) |
| `expires_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi payment order hết hạn (30 phút) |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `completed_at` | `TIMESTAMPTZ` | YES | NULL | — | Khi payment completed |

### Foreign Keys
| Column | References | On Delete |
|---|---|---|
| `subscription_id` | `subscriptions(id)` | SET NULL |
| `teacher_id` | `users(id)` | RESTRICT |
| `plan_id` | `plans(id)` | RESTRICT |

### Indexes
```
pk_payments                         PRIMARY KEY (id)
uq_payments_public_id               UNIQUE (public_id)
uq_payments_idempotency_key         UNIQUE (idempotency_key)            -- CRITICAL: dedup webhook
idx_payments_teacher                (teacher_id, created_at DESC)       -- Teacher xem history
idx_payments_status                 (status, created_at DESC)           -- Admin dashboard
idx_payments_provider_order         (provider, provider_order_id)       -- Webhook lookup
idx_payments_pending_expires        (expires_at) WHERE status = 'Pending'  -- Background: expire stale
```

### Idempotency Flow
```
Provider gọi webhook với order confirmation
    │
    ▼
Server tìm: SELECT * FROM payments WHERE idempotency_key = $1

[Key đã tồn tại VÀ status = 'Completed']
    └──► Return 200 OK (already processed, skip)

[Key đã tồn tại VÀ status = 'Pending']
    └──► BEGIN TRANSACTION
         UPDATE payments SET status = 'Completed', completed_at = NOW(), provider_transaction_id = $tx_id
         INSERT INTO subscriptions ...  (hoặc UPDATE status → Active)
         INSERT INTO invoices ...
         INSERT INTO audit_logs ...
         COMMIT
         └──► Return 200 OK

[Key không tồn tại]
    └──► Log error (unexpected) → Return 404
```

---

## Table: `invoices`

**Purpose:** Hóa đơn tự động tạo sau payment thành công. Immutable sau khi tạo.

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | |
| `payment_id` | `BIGINT` | NO | — | FK → payments(id) RESTRICT, UNIQUE | 1-1 với payment |
| `subscription_id` | `BIGINT` | YES | NULL | FK → subscriptions(id) SET NULL | |
| `teacher_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | |
| `invoice_number` | `TEXT` | NO | — | UNIQUE | Format: `INV-YYYYMM-{sequence}` |
| `amount_vnd` | `BIGINT` | NO | — | CHECK (amount_vnd > 0) | = payment.amount_vnd tại thời điểm |
| `plan_name` | `TEXT` | NO | — | — | Snapshot tên plan (không FK → plans, để immutable) |
| `billing_cycle` | `TEXT` | NO | — | — | Snapshot billing cycle |
| `issued_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | **Không có updated_at** — immutable |
| `pdf_url` | `TEXT` | YES | NULL | — | URL file PDF (generated async) |

### Unique Constraints
```sql
-- 1-1 với payment
CREATE UNIQUE INDEX uq_invoices_payment
  ON invoices (payment_id);
```

### Indexes
```
pk_invoices                     PRIMARY KEY (id)
uq_invoices_public_id           UNIQUE (public_id)
uq_invoices_payment             UNIQUE (payment_id)
uq_invoices_number              UNIQUE (invoice_number)
idx_invoices_teacher            (teacher_id, issued_at DESC)    -- Teacher xem history
```

### Notes
- **Immutable**: không có `updated_at`, không soft delete, không UPDATE sau khi tạo
- `plan_name` và `billing_cycle` được snapshot tại thời điểm phát hóa đơn — không reference plan gốc
- `invoice_number` sequence: `INV-{YYYYMM}-{zero-padded-sequence}` (vd: `INV-202601-0001`)

---

## Resource Limit Enforcement

App-layer logic, không DB constraint:

```
Khi Teacher tạo resource mới (Class / Question / Exam):
  1. Tìm Active subscription của Teacher
  2. Nếu không có → dùng Free plan limits từ system_settings
  3. Đếm current count của resource
  4. Nếu count >= limit → 403: "Đã đạt giới hạn. Nâng cấp Pro để tiếp tục."

Khi Subscription expire → Teacher về Free limits
  - Existing resources KHÔNG bị xóa
  - Chỉ không tạo thêm được

Khi Admin set Pro manual:
  - INSERT subscription với payment_type='manual'
  - Ghi audit_log
```

---

## Relationships Diagram

```
plans (1) ──────────── (*) subscriptions
users/Teacher (1) ──── (*) subscriptions
users/Teacher (1) ──── (*) payments
plans (1) ──────────── (*) payments

subscriptions (1) ──── (*) payments   [SET NULL on delete]
payments (1) ────────── (1) invoices   [1-1, RESTRICT]
subscriptions (1) ───── (*) invoices   [SET NULL on delete]
```

---

## Migration Dependencies

Phụ thuộc:
- `users` (từ 01)

### Migration order
1. `CREATE TABLE plans`
2. Apply `set_updated_at` trigger cho `plans`
3. INSERT seed data (Free, Pro Monthly, Pro Annual)
4. `CREATE TABLE subscriptions`
5. Apply `set_updated_at` trigger cho `subscriptions`
6. `CREATE TABLE payments`
7. `CREATE TABLE invoices`
8. INSERT Free subscription cho tất cả Teacher hiện tại (data migration)

---

## Open Questions

- [ ] **Free subscription creation**: Khi user được set role Teacher, tự động INSERT Free subscription hay lazy-load khi cần? Đề xuất: lazy-load (query finds no active subscription → treat as Free)
- [ ] **Invoice PDF**: Generate ở app layer với iTextSharp/QuestPDF, store ở S3/Cloudflare R2, update `pdf_url`. Cần async job.
- [ ] **Proration**: Khi Teacher nâng từ Monthly→Annual giữa kỳ, tính tiền dư như thế nào? MVP-8 để đơn giản: bắt đầu kỳ mới. Future: credit account.
- [ ] **Webhook security**: Verify signature từ Momo/VNPay trước khi process. Store signing secret trong env var, không trong DB.
