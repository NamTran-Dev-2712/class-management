# 12 — Migration Strategy

> Cross-cutting: hướng dẫn quản lý database migrations xuyên suốt dự án.

---

## 1. Tool Choice

### EF Core Migrations (Primary)

**Lý do chọn**: Stack là .NET, EF Core Code-First là standard approach. Raw SQL có thể nhúng qua `migrationBuilder.Sql()`.

```bash
# Tạo migration mới
dotnet ef migrations add <MigrationName> --project <Infrastructure> --startup-project <API>

# Apply lên database
dotnet ef database update --project <Infrastructure> --startup-project <API>

# Generate SQL script (không apply)
dotnet ef migrations script --idempotent -o migrations.sql
```

### Escape Hatch cho Raw SQL

Khi EF Core không support (complex triggers, GIN indexes, stored functions):

```csharp
// Trong migration file
protected override void Up(MigrationBuilder migrationBuilder)
{
    // EF Core normal migration
    migrationBuilder.CreateTable(...);

    // Raw SQL escape hatch
    migrationBuilder.Sql(@"
        CREATE INDEX CONCURRENTLY idx_questions_content_search
        ON questions USING GIN (content_search);
    ");

    migrationBuilder.Sql(@"
        CREATE OR REPLACE FUNCTION set_updated_at()
        RETURNS TRIGGER AS $$
        BEGIN NEW.updated_at = NOW(); RETURN NEW; END;
        $$ LANGUAGE plpgsql;
    ");
}
```

---

## 2. Naming Convention

### Migration File Names

```
Format: {YYYYMMDDHHmm}_{verb}_{noun_or_description}

Examples:
  202601150900_create_extensions_and_functions
  202601150910_create_users_roles_tables
  202601150920_create_subjects_table
  202601160900_create_classes_table
  202601160910_create_class_memberships_table
  202601170900_create_questions_tables
  ...
  202602010900_add_email_verification_to_users    -- Feature addition
  202602010910_add_idx_questions_content_search   -- Index addition
  202602050900_fix_snapshot_questions_fk          -- Bug fix
```

### Verb Guide

| Verb | Dùng khi |
|---|---|
| `create` | Tạo table/index mới |
| `add` | Thêm column, index, constraint vào table hiện có |
| `alter` | Đổi type, rename column (cẩn thận) |
| `drop` | Xóa column, table, index |
| `fix` | Bug fix trong schema |
| `seed` | Data migration |
| `backfill` | Backfill data cho column mới |

---

## 3. Migration Order (theo MVP)

### MVP-1: Core Foundation

```
202601150900_create_extensions              -- citext, pgcrypto, pg_stat_statements
202601150910_create_updated_at_function     -- Shared trigger function
202601150920_create_roles_table             -- Seed roles
202601150930_create_users_table             -- + triggers + indexes
202601150940_create_user_roles_table
202601150950_create_refresh_tokens_table
202601151000_create_password_reset_tokens_table
202601151010_create_subjects_table          -- + seed data
202606051600_add_attempt_count_to_password_reset_tokens  -- OTP brute-force cap (forgot/reset password)
202606111824_add_admin_users_view           -- vw_admin_users read model for Admin user management
```

> Lưu ý thực tế: `classes` và `class_memberships` (MVP-2) được tạo ngay trong `InitDatabase` (scaffold ban đầu), không phải migration riêng.

> Hangfire tự tạo schema `hangfire` lúc app start — **không** dùng EF migration. Xem [14-background-jobs.md](./14-background-jobs.md).

### MVP-2: Classroom

`classes` + `class_memberships` đã có sẵn từ `InitDatabase`. MVP-2 chỉ bổ sung:

```
202606141757_create_classroom_views_and_subject_name
  -- Thêm cột classes.subject_name (snapshot tên môn / free-text)
  -- Tạo view vw_classes (class + owner + member counts)
  -- Tạo view vw_class_members (membership + student + class + owner)
```

### MVP-3: Question Bank

**Đã triển khai** — gộp trong **một** migration (khác với kế hoạch 4 migration ban đầu):

```
20260615180014_create_questions_and_views
  -- Tạo bảng questions, question_options, question_tags (check constraints + indexes theo doc 03)
  -- CREATE EXTENSION pg_trgm + index GIN idx_questions_content_trgm trên lower(content)
  -- Tạo view vw_questions (question + teacher + subject + option_count + tags[])
```

> Khác kế hoạch gốc: `content` là Markdown (không tsvector), `updated_at` qua interceptor (không
> trigger), search dùng ILIKE + pg_trgm GIN. Chi tiết: [03-schema-question-bank.md](./03-schema-question-bank.md).

### MVP-4: Exam Builder

**Đã triển khai** — gộp trong **một** migration (khác với kế hoạch 2 migration ban đầu):

```
20260617150412_create_exams_and_views
  -- Tạo bảng exams, exam_questions (check constraints + indexes theo doc 04)
  -- Tạo view vw_exams (exam + teacher display_name + live subject name/public_id)
20260618150808_add_exam_tags_and_update_view
  -- Tạo bảng exam_tags (CHECK '^[a-z0-9-]{1,50}$', unique (exam_id, tag), idx tag)
  -- DROP + tạo lại vw_exams kèm cột tags text[] (array_agg từ exam_tags) — giống question_tags
```

> Khác kế hoạch gốc: **không dùng DB trigger** để sync `total_point`/`total_questions`/`version`.
> Các giá trị denormalized này được tính lại + tăng `version` ngay trong **application write handler**
> (`UpdateExamQuestionsCommandHandler`) cùng một `SaveChangesAsync` — dễ debug, đúng chuẩn UoW của
> repo, và `version` tăng đúng +1 mỗi lần lưu danh sách câu hỏi. `updated_at` qua interceptor.
> Chi tiết: [04-schema-exam.md](./04-schema-exam.md).

### MVP-5: Assignment & Testing

```
20260617173525_create_assignments_and_views
  -- Tạo bảng assignments, assignment_snapshots, snapshot_questions, snapshot_options,
  --   attempts, attempt_answers (check constraints + indexes theo doc 05 & 06)
  -- question_order / selected_option_ids = JSONB (mảng id), value-converter sang List<long>
  -- snapshot_questions/snapshot_options có public_id (uuid) để API tham chiếu
  -- Tạo 3 view: vw_assignments, vw_student_assignments, vw_attempts (raw SQL)
```

> Gộp toàn bộ MVP-5 vào **một migration** (giống cách Exams/Questions làm). Enum lưu PascalCase.
> **Không dùng DB trigger**: `total_point`/`total_questions` của snapshot tính trong publish handler;
> auto-grade + auto-submit do **Hangfire recurring job** `assignment-lifecycle` + app-layer xử lý
> (xem 05/06 "as built" và [14-background-jobs.md](./14-background-jobs.md)). `updated_at` qua interceptor.

### MVP-6: Grading (as built)

```
20260618213719_create_manual_grades
  -- Tạo bảng manual_grades (score numeric(8,2) CHECK >= 0, feedback CHECK length <= 5000,
  --   unique (attempt_id, snapshot_question_id), idx attempt_id + created_by)
  -- FK: attempt_id → attempts CASCADE, snapshot_question_id → snapshot_questions RESTRICT,
  --   created_by/updated_by → users SET NULL (cột auditable chuẩn = graded_by/graded_at semantics)
```

> Khác kế hoạch gốc trong [07-schema-grading.md](./07-schema-grading.md): **không dùng DB trigger**
> `check_attempt_grading_complete`. Việc chuyển attempt sang `Graded` + tính lại `total_manual_score`/
> `total_score` được làm ở **application layer** (`ManualGradeFinalizer`, dùng chung bởi GradeAttempt
> handler) cùng một `SaveChangesAsync` — đồng bộ với cách auto-grade (`AttemptGrading`) đã làm ở MVP-5,
> dễ debug hơn. **Không tạo bảng `assignment_grade_releases`**: việc công bố điểm (Manual policy) chỉ
> set cột `assignments.grades_released_at` đã có sẵn từ MVP-5 (released_by luôn là teacher chủ sở hữu;
> audit đầy đủ ai/khi nào để dành cho MVP-7 `audit_logs`). Không có view mới — tổng điểm nằm trên
> bảng `attempts`, đã được `vw_attempts`/`vw_student_assignments` đọc.

### MVP-7: Admin (as built)

**Đã triển khai** — gộp 4 bảng vào **một** migration (giống Questions/Exams/Assignments):

```
20260620063139_create_admin_moderation_tables
  -- audit_logs    (append-only; KHÔNG public_id, KHÔNG updated_at; actor/target đa hình KHÔNG FK;
  --                metadata jsonb; CHECK action/actor_role/target_type; 4 index theo doc 08)
  -- reports       (public_id; FK reporter_id RESTRICT, admin_id SET NULL; uq_reports_idempotent
  --                partial unique (reporter_id,target_type,target_id) WHERE status IN Pending/Reviewing)
  -- notifications (public_id; FK user_id CASCADE; reference_id + uq_notifications_idempotent partial
  --                unique (user_id,event_type,reference_id) WHERE reference_id IS NOT NULL)
  -- system_settings (value jsonb; uq_system_settings_key; FK updated_by SET NULL)
20260620064853_add_audit_logs_view    -- vw_audit_logs (audit ⨝ actor; metadata::text)
20260621064905_add_reports_view       -- vw_reports (report ⨝ reporter ⨝ reviewing admin)
```

> Notifications đọc trực tiếp từ bảng `notifications` (scope theo current user) qua `BaseGetQueryHandler`
> — không cần view riêng.

> Khác kế hoạch gốc / [08-schema-admin.md](./08-schema-admin.md):
> - **Idempotency notifications** dùng cột `reference_id` (text) thay vì biểu thức `payload->>'reference_id'`
>   trong partial index — `NOW()` không IMMUTABLE nên cửa sổ 24h của doc không thể nằm trong predicate
>   (gotcha #4); job dedup theo thời gian, partial unique là backstop cứng cho event có reference_id.
> - **`reason`** lưu PascalCase (`InappropriateContent`…) cho nhất quán với mọi enum-as-string khác.
> - **`system_settings`** thêm `created_at` (từ `BaseEntity`) — vô hại, không có trong doc gốc.
> - **REVOKE UPDATE/DELETE** trên `audit_logs` ở DB level: **chưa áp dụng** ở MVP-7 (app-layer append-only
>   qua `IAuditLogger`; cân nhắc bật ở hardening sau vì test/seed dùng chung role). `updated_at` của
>   `reports`/`system_settings` qua trigger `set_updated_at` (thêm trong `DatabaseSeeder.ApplyTriggersAsync`).
> - Seed 12 `system_settings` mặc định qua `SystemSettingSeeder` (idempotent, additive).
> - View đọc (`vw_audit_logs`, `vw_reports`, `vw_notifications`) tạo ở các migration sau theo từng phase.

### MVP-8: Payment (as built)

**Đã triển khai** — gộp 4 bảng vào **một** migration (giống Questions/Exams/Assignments), + một
migration view riêng cho admin:

```
20260626153009_create_payment_tables
  -- plans, subscriptions, payments, invoices (check constraints + indexes theo doc 09)
  -- partial unique uq_subscriptions_teacher_active (teacher_id) WHERE status='Active' (BR-8-01)
  -- uq_payments_idempotency_key (dedup webhook), idx_payments_pending_expires (sweep)
  -- uq_invoices_payment (1-1), uq_invoices_number; sequence payment_invoice_number_seq (raw SQL)
  -- mở rộng chk_audit_logs_action thêm các action subscription.*/payment.*/invoice.issued
20260628144059_add_payment_admin_views
  -- vw_subscriptions (subscription ⨝ teacher ⨝ plan) + vw_payments (payment ⨝ teacher ⨝ plan) — raw SQL
```

> Khác kế hoạch gốc:
> - **Enum lưu PascalCase** (`Monthly`/`Annual`, `Momo`/`VnPay`, `Auto`/`Manual`, `Active`/`PastDue`/…)
>   cho nhất quán với mọi enum-as-string khác — CHECK constraints dùng đúng các giá trị này.
> - **Không seed Free subscription cho mỗi teacher** (open question đã chốt: lazy — không có Active
>   subscription ⇒ coi như Free). Free limits đọc từ `system_settings` qua `IResourceLimitService`.
> - **Không có bảng riêng cho invoice number**: dùng Postgres sequence `payment_invoice_number_seq`
>   (race-free) format `INV-YYYYMM-######`.
> - **Plans là nguồn sự thật cho resource limit theo plan**: teacher có Active/PastDue (grace) sub →
>   limit của plan đó (null = unlimited); ngược lại → Free caps live từ `system_settings`.
> - Provider trừu tượng qua `IPaymentProvider` (+ `IPaymentProviderResolver`): `FakePaymentProvider`
>   (dev/test, `Payment:UseFakeProvider=true`) + `MomoPaymentProvider`/`VnPayPaymentProvider` thật.
> - Lifecycle (Active→PastDue/Cancelled, PastDue→Expired, hết hạn Pending order) chạy bằng Hangfire
>   recurring `subscription-lifecycle` → `RunSubscriptionLifecycleCommand` (app-layer, idempotent).


---

## 4. Down Migrations (Rollback)

**Nguyên tắc**: Mọi migration phải có `Down()` viết đầy đủ.

```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // Đảo ngược CHÍNH XÁC những gì Up() đã làm
    // Theo thứ tự ngược lại

    // Xóa triggers trước
    migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_users_updated_at ON users;");

    // Xóa indexes
    migrationBuilder.DropIndex("uq_users_email_active", "users");

    // Xóa tables (theo thứ tự FK)
    migrationBuilder.DropTable("user_roles");
    migrationBuilder.DropTable("users");
}
```

### Production Rollback Policy

> **Quan trọng**: Production dùng **forward-only** mindset. Down migration chỉ dùng trong development/staging.

| Environment | Rollback approach |
|---|---|
| Development | `dotnet ef database update <PreviousMigration>` |
| Staging | Down migration hoặc restore from backup |
| **Production** | **Forward-only**: deploy hotfix migration thay vì rollback |

---

## 5. Zero-Downtime Migrations

Áp dụng khi table có nhiều data hoặc production traffic cao.

### Pattern 1: Add nullable column (safe, zero-downtime)

```
Step 1: Add column nullable
  ALTER TABLE users ADD COLUMN email_verified_at TIMESTAMPTZ NULL;
  → Instant, no downtime

Step 2 (optional): Backfill
  UPDATE users SET email_verified_at = created_at WHERE email_verified_at IS NULL;
  → Batch update, có thể chạy online

Step 3 (optional): Set NOT NULL nếu cần
  ALTER TABLE users ALTER COLUMN email_verified_at SET NOT NULL;
  → Chỉ sau khi backfill hoàn thành
```

### Pattern 2: Add index (zero-downtime)

```sql
-- CONCURRENTLY: build index mà không lock table
CREATE INDEX CONCURRENTLY idx_questions_new ON questions (subject_id, type);

-- EF Core: dùng migrationBuilder.Sql()
-- Không dùng migrationBuilder.CreateIndex() vì không hỗ trợ CONCURRENTLY
```

> **Warning**: `CREATE INDEX CONCURRENTLY` không thể chạy trong transaction. Phải là standalone migration, không wrap trong BEGIN/COMMIT.

### Pattern 3: Rename column (4-step, zero-downtime)

```
Step 1: Add new column (nullable)
  ALTER TABLE users ADD COLUMN display_name_new TEXT;

Step 2: Dual-write in app
  // App writes to BOTH old and new column

Step 3: Backfill
  UPDATE users SET display_name_new = display_name WHERE display_name_new IS NULL;

Step 4: Switch app to read from new column, drop old
  // After deploy: drop old column
  ALTER TABLE users DROP COLUMN display_name;
  ALTER TABLE users RENAME COLUMN display_name_new TO display_name;
```

### Pattern 4: Drop column (safe 2-step)

```
Step 1: Remove from application code (stop reading/writing to column)
  → Deploy app without using the column

Step 2: Drop column from DB
  ALTER TABLE users DROP COLUMN obsolete_column;
  → Now safe to drop, no app code references it
```

---

## 6. Long-Running Migration Safety

### Batch Updates

Tránh `UPDATE table SET ... WHERE 1=1` trên table lớn — lock table quá lâu.

```sql
-- Batch update với loop
DO $$
DECLARE
  batch_size INT := 1000;
  updated INT;
BEGIN
  LOOP
    UPDATE attempt_answers
    SET submitted_at = (SELECT submitted_at FROM attempts WHERE id = attempt_id)
    WHERE submitted_at IS NULL
    LIMIT batch_size;

    GET DIAGNOSTICS updated = ROW_COUNT;
    EXIT WHEN updated < batch_size;

    PERFORM pg_sleep(0.1);  -- Brief pause between batches
  END LOOP;
END $$;
```

### Statement Timeout

```sql
-- Set timeout cho long-running migration (không bị hang forever)
SET statement_timeout = '30s';
ALTER TABLE ... ;
RESET statement_timeout;
```

---

## 7. Migration Safety Checklist

Trước khi apply migration lên production:

**Code Review**
- [ ] Migration có `Down()` method viết đầy đủ
- [ ] Không có `DROP TABLE` hay `DROP COLUMN` mà không có 2-step process
- [ ] Không có UPDATE/DELETE trên large table mà không batch
- [ ] `CREATE INDEX` dùng `CONCURRENTLY` (nếu table có data)

**Testing**
- [ ] Migration chạy thành công trên dev database
- [ ] `Down()` rollback chạy thành công (test trên dev)
- [ ] Migration chạy thành công trên staging environment với data tương tự production

**Pre-deployment**
- [ ] Backup production database xong
- [ ] Migration script được review bởi ít nhất 1 người khác
- [ ] Estimate runtime (test trên staging với production-size data nếu có)
- [ ] Maintenance window allocated nếu migration có downtime risk

**Post-deployment**
- [ ] Verify: query key tables sau migration, data còn đúng
- [ ] Monitor: error rate, slow queries trong 15 phút đầu
- [ ] Rollback plan ready (restore từ backup nếu cần)

---

## 8. Seed Data Management

### Idempotent Seeding

```csharp
// Trong seed migration (hoặc Program.cs startup)
public static async Task SeedAsync(ApplicationDbContext context)
{
    // Idempotent: chỉ insert nếu chưa có
    if (!await context.Roles.AnyAsync())
    {
        context.Roles.AddRange(
            new Role { Name = "Student" },
            new Role { Name = "Teacher" },
            new Role { Name = "Admin" }
        );
        await context.SaveChangesAsync();
    }

    if (!await context.SystemSettings.AnyAsync(s => s.Key == "max_classes_per_teacher"))
    {
        context.SystemSettings.Add(new SystemSetting
        {
            Key = "max_classes_per_teacher",
            Value = JsonDocument.Parse("10"),
            ValueType = "integer",
            Description = "Free plan: max classes per teacher"
        });
        await context.SaveChangesAsync();
    }
}
```

### Seed Data Files

Seed data quan trọng nên có migration file riêng (không trộn với schema migration):

```
202601150920_create_subjects_table.cs     -- Schema
202601151010_seed_roles.cs                -- Seed
202601151020_seed_subjects.cs             -- Seed
202603050900_create_plans_table.cs        -- Schema
202603050910_seed_plans.cs                -- Seed
202603050920_seed_system_settings.cs      -- Seed
```

---

## 9. CI/CD Integration

### GitHub Actions / Azure DevOps Pipeline

```yaml
# migration-check.yml
- name: Validate migrations
  run: |
    # Generate migration script
    dotnet ef migrations script --idempotent -o /tmp/migration.sql
    
    # Syntax check (PostgreSQL client)
    psql $CI_DB_URL --command "\i /tmp/migration.sql" --dry-run

- name: Apply migrations (staging)
  run: |
    dotnet ef database update --connection "$STAGING_DB_CONNECTION"

- name: Run smoke tests
  run: dotnet test --filter Category=Database
```

### Rollback on Failure

```yaml
- name: Apply migrations with rollback on failure
  run: |
    # Save current migration state
    CURRENT=$(dotnet ef migrations list | tail -1)
    
    # Try to apply
    if ! dotnet ef database update; then
      echo "Migration failed, rolling back to $CURRENT"
      dotnet ef database update $CURRENT
      exit 1
    fi
```
