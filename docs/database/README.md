# Database Design — Class Management System

## Tổng quan

| Attribute | Value |
|---|---|
| Database | PostgreSQL 16+ |
| Architecture | Monolithic (single schema `public`) |
| ORM | EF Core (Code-First, raw SQL escape hatch khi cần) |
| Charset | UTF-8 (`en_US.UTF-8`) |
| Timezone | Store tất cả timestamp dạng `TIMESTAMPTZ` (UTC) |

Bộ tài liệu này là **nền tảng kỹ thuật** để triển khai thực tế. Mọi quyết định schema ở đây đều bám sát `docs/mvp/` và có thể mở rộng theo `docs/mvp/ROADMAP-FUTURE.md`.

---

## Index tài liệu

| File | Mô tả | MVP liên quan |
|---|---|---|
| [00-conventions.md](./00-conventions.md) | Naming, IDs, timestamps, soft delete, audit, enum | Cross-cutting |
| [01-schema-auth.md](./01-schema-auth.md) | users, roles, tokens, subjects | MVP-1 |
| [02-schema-classroom.md](./02-schema-classroom.md) | classes, class_memberships | MVP-2 |
| [03-schema-question-bank.md](./03-schema-question-bank.md) | questions, question_options, question_tags | MVP-3 |
| [04-schema-exam.md](./04-schema-exam.md) | exams, exam_questions | MVP-4 |
| [05-schema-assignment-snapshot.md](./05-schema-assignment-snapshot.md) | assignments, assignment_snapshots, snapshot_questions/options | MVP-5 |
| [06-schema-attempt.md](./06-schema-attempt.md) | attempts, attempt_answers | MVP-5 |
| [07-schema-grading.md](./07-schema-grading.md) | manual_grades, assignment_grade_releases | MVP-6 |
| [08-schema-admin.md](./08-schema-admin.md) | audit_logs, reports, notifications, system_settings | MVP-7 |
| [09-schema-payment.md](./09-schema-payment.md) | plans, subscriptions, payments, invoices | MVP-8 |
| [10-indexing-and-performance.md](./10-indexing-and-performance.md) | Index design, hot queries, partitioning | Cross-cutting |
| [11-security.md](./11-security.md) | Password hashing, PII, RLS, backup | Cross-cutting |
| [12-migrations-strategy.md](./12-migrations-strategy.md) | Migration order, rollback, zero-downtime | Cross-cutting |
| [13-extensibility.md](./13-extensibility.md) | Database extension points cho ROADMAP-FUTURE | Future |

---

## Tổng số bảng theo MVP

| MVP | Tables mới | Cumulative |
|---|---|---|
| MVP-1 | users, roles, user_roles, refresh_tokens, password_reset_tokens, subjects | 6 |
| MVP-2 | classes, class_memberships | 8 |
| MVP-3 | questions, question_options, question_tags | 11 |
| MVP-4 | exams, exam_questions | 13 |
| MVP-5 | assignments, assignment_snapshots, snapshot_questions, snapshot_options, attempts, attempt_answers | 19 |
| MVP-6 | manual_grades, assignment_grade_releases | 21 |
| MVP-7 | audit_logs, reports, notifications, system_settings | 25 |
| MVP-8 | plans, subscriptions, payments, invoices | 29 |

---

## Design Principles

### 1. Snapshot Immutability
Khi Assignment được publish, toàn bộ nội dung Exam + Questions được **copy** vào `assignment_snapshots` / `snapshot_questions` / `snapshot_options`. Attempt chỉ đọc từ snapshot, không bao giờ đọc từ `exams` hay `questions` gốc. Điều này đảm bảo:
- Teacher sửa câu hỏi/đề sau khi giao → không ảnh hưởng bài đã làm
- Lịch sử điểm luôn đúng với nội dung học sinh thực sự làm

### 2. Append-only Audit Log
`audit_logs` **không có** `updated_at`, **không** soft delete, **không** cho phép UPDATE. Chỉ INSERT. Bất kỳ hành động nhạy cảm nào đều tạo row mới.

### 3. Idempotency cho Payment
`payments.idempotency_key` là `UNIQUE NOT NULL`. Webhook từ Momo/VNPay có thể gọi nhiều lần — server kiểm tra key trước khi process để không apply 2 lần.

### 4. No Cascading Deletes cho Business Data
Bảng nghiệp vụ quan trọng dùng **RESTRICT** hoặc **SET NULL** thay vì CASCADE. Tránh mất dữ liệu do xoá nhầm. Business data dùng soft delete (`deleted_at`).

### 5. ID Strategy: BIGINT Internal + UUID Public
- Internal join: `BIGINT GENERATED ALWAYS AS IDENTITY` — nhỏ gọn, fast B-tree
- API exposure: `public_id UUID NOT NULL DEFAULT gen_random_uuid()` — không predictable
- API **chỉ expose `public_id`**, không bao giờ expose `id`

### 6. Subject là nhãn tùy chọn (Optional Label)
`subjects` là bảng lookup do Admin quản lý để phân loại nội dung. `subject_id` trong `classes`, `questions`, `exams` đều **nullable** — Teacher không bắt buộc phải gắn môn học. Phù hợp với lớp học ngoài phạm vi trường chính quy (câu lạc bộ, khoá học tự do...). FK dùng `SET NULL` thay vì `RESTRICT` để xóa Subject không ảnh hưởng data liên quan.

### 7. Multi-tenancy Ready (Không implement ngay)
Schema hiện tại là single-tenant SaaS. Để chuẩn bị B2B sau này, thiết kế unique constraints theo dạng có thể thêm `organization_id` vào composite mà không phá logic hiện tại. Chi tiết ở [13-extensibility.md](./13-extensibility.md).

---

## Dependency graph giữa các bảng

```
subjects ──(nullable)──► classes ◄── class_memberships
subjects ──(nullable)──► questions
subjects ──(nullable)──► exams

                classes ◄── class_memberships
                  │         users ◄── user_roles ──► roles
                  │           │
                  └───────────┘
                          │
                      assignments ──► assignment_snapshots
                        │               │
                    exams               ├── snapshot_questions
                        │               └── snapshot_options
                    exam_questions           │
                        │               attempts ──► attempt_answers
                    questions               │
                        │           manual_grades
                    question_options
                    question_tags

audit_logs (standalone, references actor_id nullable)
reports (standalone, references reporter_id)
notifications (references user_id)
system_settings (standalone)
plans ◄── subscriptions ◄── payments ◄── invoices
```

---

## Đọc tài liệu này như thế nào

1. **Đọc trước:** [00-conventions.md](./00-conventions.md) — đây là "luật" cho mọi bảng
2. **Theo thứ tự MVP:** `01` → `09` khi implement từng phase
3. **Trước khi code:** Đọc [10-indexing-and-performance.md](./10-indexing-and-performance.md) để hiểu index nào cần thêm ngay
4. **Trước khi deploy:** Review [11-security.md](./11-security.md) và [12-migrations-strategy.md](./12-migrations-strategy.md)
5. **Khi cần mở rộng:** [13-extensibility.md](./13-extensibility.md) mô tả cách thêm feature không phá schema hiện tại
