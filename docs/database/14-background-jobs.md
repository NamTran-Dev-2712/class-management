# 14 — Background Jobs (Hangfire)

> Cross-cutting: hàng đợi job nền cho các tác vụ bất đồng bộ (gửi email, …).

---

## 1. Tổng quan

Dự án dùng **Hangfire** với storage **PostgreSQL** (`Hangfire.PostgreSql`) để chạy job nền một cách
**bền vững** — job sống sót qua restart, có retry tự động. Use case đầu tiên: gửi email đặt lại mật khẩu
(OTP + link) qua **Resend** mà không chặn HTTP request.

```
HTTP request ──▶ IEmailQueueService.EnqueuePasswordResetEmail(...)
                      │  (HangfireEmailQueueService → IBackgroundJobClient.Enqueue)
                      ▼
              Hangfire job (PasswordResetEmailJob)
                      │  IEmailService (ResendEmailService)
                      ▼
                  Resend API
```

Abstraction giữ Application **không** phụ thuộc Hangfire/Resend:
- `IEmailQueueService` (Application) → `HangfireEmailQueueService` (Infrastructure) enqueue job.
- `IEmailService` (Application) → `ResendEmailService` (Infrastructure) gọi Resend.

---

## 2. Schema `hangfire`

> ⚠️ **Hangfire tự quản lý schema của nó** — **KHÔNG** quản lý bằng EF Core migration.

`Hangfire.PostgreSql` tự tạo schema riêng tên `hangfire` (mặc định) khi storage khởi tạo lúc app start,
gồm các bảng nội bộ: `job`, `state`, `jobparameter`, `jobqueue`, `server`, `list`, `set`, `hash`, `counter`,
`aggregatedcounter`, `schema`, `lock`. Các bảng này:

- **Tách biệt** hoàn toàn khỏi domain schema (`public`) — không xuất hiện trong ERD nghiệp vụ.
- Không có trong `ApplicationDbContextModelSnapshot`, không sinh EF migration.
- Được Hangfire migrate idempotent theo version của package.

Khi backup/restore: schema `hangfire` chứa state job tạm thời, có thể tái tạo — không phải dữ liệu nghiệp vụ.

---

## 3. Cấu hình (`HangfireOptions` — section `Hangfire`)

| Key | Default | Mô tả |
|---|---|---|
| `Enabled` | `true` | Bật/tắt toàn bộ Hangfire (storage + server). Tests đặt `false`. |
| `EnableServer` | `true` | Có chạy background server (worker xử lý job) trong process này không. |
| `DashboardPath` | `/hangfire` | Đường dẫn dashboard. |
| `WorkerCount` | `0` | 0 = mặc định Hangfire (`ProcessorCount * 5`). |

> **Lưu ý môi trường:** `Enabled` được đọc lúc **đăng ký service** (gate `AddHangfire`/`AddHangfireServer`),
> nên override phải đến từ nguồn config có sẵn sớm (env var / appsettings), không phải nguồn nạp trễ.
> Integration tests tắt Hangfire bằng env var `Hangfire__Enabled=false` và fake `IEmailQueueService`.

---

## 4. Dashboard

`GET {DashboardPath}` (mặc định `/hangfire`) — chỉ **Admin** truy cập, bảo vệ bằng
`HangfireAdminAuthorizationFilter` (kiểm tra `User.IsInRole("Admin")`). Authentication (cookie/JWT) chạy
trong pipeline trước filter nên `HttpContext.User` đã có sẵn.

## 5. Recurring job — Assignment lifecycle (MVP-5)

Job định kỳ `assignment-lifecycle` (đăng ký trong `Program.cs` qua `IRecurringJobManager`, gated bởi
`Hangfire:Enabled` + `EnableServer`; cron lấy từ `Assignment:LifecycleSweepCron`, mặc định mỗi phút).

- Class mỏng `AssignmentLifecycleJob.ExecuteAsync()` chỉ gửi `RunAssignmentLifecycleCommand` qua MediatR
  (toàn bộ logic ở Application — dùng chung đường finalize + auto-grade với luồng học sinh nộp bài).
- Mỗi lần chạy (idempotent, có giới hạn `Assignment:LifecycleBatchSize`):
  1. `Scheduled → Open` khi `opens_at ≤ now`.
  2. `Open → Closed` khi `closes_at ≤ now` (+ auto-submit mọi attempt `InProgress` của assignment đó).
  3. Auto-submit các attempt `InProgress` đã qua `deadline_at`.
- **Phòng thủ nhiều lớp**: ngoài job, mỗi request làm bài còn kiểm tra thời gian phía server (lazy
  check) nên deadline vẫn được tôn trọng kể cả khi Hangfire tắt (vd: trong test).

## 6. Recurring job — Media cleanup (MVP-9)

Job định kỳ `media-cleanup` (đăng ký trong `Program.cs`, gated bởi `Hangfire:Enabled` + `EnableServer`;
cron từ `Media:CleanupCron`, mặc định mỗi 30 phút).

- Class mỏng `MediaCleanupJob.ExecuteAsync()` đọc tunable từ `MediaOptions` (appsettings) rồi gửi
  `RunMediaCleanupCommand(PendingConfirmTtlMinutes, BatchSize)` qua MediatR — toàn bộ logic ở Application,
  không phụ thuộc Options.
- Mỗi lần chạy (idempotent, giới hạn `Media:CleanupBatchSize`) quét 2 nhóm asset mồ côi:
  1. **Pending quá hạn**: `status = 'Pending'` và `created_at < now - PendingConfirmTtlMinutes` (presign
     nhưng không confirm).
  2. **Đã soft-delete và không còn được pin**: `deleted_at IS NOT NULL` và `public_id` **không** xuất hiện
     trong `snapshot_media` — tức không assignment đã publish nào còn tham chiếu (BR-9-06).
- Với mỗi asset: xóa object dưới storage (`IStorageProvider.DeleteAsync`) rồi **hard-delete** row
  (`ExecuteDeleteAsync`, bỏ qua interceptor soft-delete). Nếu xóa storage lỗi → log + **giữ row** để thử
  lại lần sau (không bao giờ mồ côi object).
- Asset soft-delete **vẫn được snapshot pin** thì không bị xóa vật lý → Attempt cũ/đang làm vẫn xem được
  media qua `frozen_url` (BR-9-08).
