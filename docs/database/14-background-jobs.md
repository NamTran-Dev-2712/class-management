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
