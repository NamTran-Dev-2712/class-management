# 15 — Schema: Media & Rich Content Library

> MVP liên quan: [MVP-9 — Media & Rich Content Library](../mvp/MVP-9.md)
> Conventions: [00-conventions.md](./00-conventions.md)
> Phụ thuộc: [01-schema-auth.md](./01-schema-auth.md) (`users`), [03-schema-question-bank.md](./03-schema-question-bank.md) (`question_media`, `question_options.media_id`), [05-schema-assignment-snapshot.md](./05-schema-assignment-snapshot.md) (`snapshot_media`), [09-schema-payment.md](./09-schema-payment.md) (`plans.max_storage_bytes` — quota theo plan)

---

## Tables Overview

| Table | Mô tả |
|---|---|
| `media_assets` | Media (ảnh/audio/video) do Teacher upload; lưu ở object storage (R2/Local), DB chỉ giữ metadata |
| `question_media` | (ở [03](./03-schema-question-bank.md)) bảng nối media ↔ question (attachment cấp Question) |
| `snapshot_media` | (ở [05](./05-schema-assignment-snapshot.md)) bản đóng băng media khi publish Assignment + guard chống cleanup |

Bytes **không bao giờ** đi qua API .NET: client upload thẳng lên storage qua presigned URL (BR-9-02). API chỉ giữ metadata + cấp presign + confirm.

---

## Table: `media_assets`

**Purpose:** Một file media đã upload. Tạo ở trạng thái `Pending` lúc presign, chuyển `Confirmed` sau khi object thực sự tồn tại trên storage (BR-9-03). `storage_key` là key nội bộ, **không bao giờ** trả qua API — client chỉ thấy `public_id` + `url` (BR-9-01).

### Columns

| Column | Type | Nullable | Default | Constraints | Description |
|---|---|---|---|---|---|
| `id` | `BIGINT` | NO | identity | PK | Nội bộ, không expose |
| `public_id` | `UUID` | NO | `gen_random_uuid()` | UNIQUE | ID đối ngoại |
| `owner_id` | `BIGINT` | NO | — | FK → users(id) RESTRICT | Teacher tạo media |
| `provider` | `TEXT` | NO | — | CHECK (provider IN ('Local', 'R2')) | Backend storage đang dùng |
| `storage_key` | `VARCHAR(500)` | NO | — | — | Key object nội bộ (`media/{ownerPublicId}/{mediaPublicId}.ext`). **Không expose** |
| `url` | `VARCHAR(1000)` | NO | — | — | URL CDN công khai (`PublicBaseUrl + storage_key`), đóng băng vào snapshot |
| `kind` | `TEXT` | NO | — | CHECK (kind IN ('Image', 'Audio', 'Video')) | Loại media |
| `content_type` | `VARCHAR(100)` | NO | — | — | MIME (validate theo allowlist) |
| `byte_size` | `BIGINT` | NO | — | CHECK (byte_size > 0) | Kích thước file |
| `width` | `INT` | YES | NULL | CHECK (>0) | Chiều rộng (ảnh/video) |
| `height` | `INT` | YES | NULL | CHECK (>0) | Chiều cao (ảnh/video) |
| `duration_seconds` | `INT` | YES | NULL | CHECK (>=0) | Thời lượng (audio/video) |
| `status` | `TEXT` | NO | `'Pending'` | CHECK (status IN ('Pending', 'Confirmed')) | Lifecycle |
| `deleted_at` | `TIMESTAMPTZ` | YES | NULL | — | Soft delete |
| `created_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | |
| `updated_at` | `TIMESTAMPTZ` | NO | `NOW()` | — | Trigger `set_updated_at` |
| `created_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | = owner_id |
| `updated_by` | `BIGINT` | YES | NULL | FK → users(id) SET NULL | |

### Foreign Keys
| Column | References | On Delete | Lý do |
|---|---|---|---|
| `owner_id` | `users(id)` | RESTRICT | Không xóa Teacher khi còn media |

### Indexes
```
pk_media_assets                 PRIMARY KEY (id)
uq_media_assets_public_id       UNIQUE (public_id)
idx_media_assets_owner          (owner_id) WHERE deleted_at IS NULL        -- thư viện + quota sum
idx_media_assets_status_created (status, created_at)                       -- cleanup: Pending-expired sweep
idx_media_assets_deleted        (deleted_at) WHERE deleted_at IS NOT NULL  -- cleanup: object chờ xóa vật lý
```

### Notes
- **Quota (BR-9-04):** tổng `byte_size` các asset (Pending + Confirmed) của owner ≤ cap theo plan (`plans.max_storage_bytes`; `NULL/0 = unlimited`), resolve qua `IResourceLimitService`, kiểm tại bước **presign**.
- **Per-file cap (BR-9-05):** cap theo kind (image/audio/video) đọc live từ `system_settings` (`max_image_bytes`/`max_audio_bytes`/`max_video_bytes`) qua `IMediaPolicy`.
- **Cleanup (BR-9-08):** Hangfire recurring `media-cleanup` xóa object dưới storage cho (a) asset `Pending` quá `PendingConfirmTtlMinutes`, và (b) asset đã soft-delete **và không còn** được `snapshot_media` pin. Xem [14-background-jobs.md](./14-background-jobs.md).
- **Không lộ `storage_key`:** view `vw_media` và mọi DTO chỉ trả `public_id` + `url`.

---

## Read model — view `vw_media`

Teacher library list + admin storage list dùng read-model view để tái sử dụng `BaseGetQueryHandler` mà không JOIN Identity `ApplicationUser`. **Không** project `storage_key` (BR-9-01).

```sql
CREATE VIEW vw_media AS
SELECT m.id, m.public_id, m.provider, m.url, m.kind, m.content_type, m.byte_size,
       m.width, m.height, m.duration_seconds, m.status, m.created_at, m.updated_at,
       m.owner_id, u.public_id AS owner_public_id, u.display_name AS owner_name
FROM media_assets m
JOIN users u ON u.id = m.owner_id
WHERE m.deleted_at IS NULL;
```

---

## Storage provider abstraction (không phải DB)

Secrets storage đọc từ config/env, **không bao giờ lưu DB** (BR-9-10). `IStorageProvider`/`IStorageProviderResolver` (Application) + adapter `LocalStorageProvider` (dev/test, `Storage:UseFakeProvider=true`) / `R2StorageProvider` (prod, AWS S3 SDK). Đổi provider không chạm Application — mirror `IPaymentProvider` (MVP-8).

---

## Relationships Diagram

```
users/Teacher (1) ──────────── (*) media_assets

media_assets (1) ───────────── (*) question_media          [RESTRICT]  (attachment cấp question)
media_assets (0..1) ────────── (*) question_options.media_id [SET NULL] (ảnh cho option)

publish → snapshot_media (đóng băng public_id + frozen_url; guard chống cleanup)
```

---

## Migration

- `20260702145021_create_media_and_alter_questions_snapshots` — tạo `media_assets`, `question_media`, `snapshot_media`; thêm `question_options.media_id`, `plans.max_storage_bytes`; DROP+CREATE `vw_questions` (thêm `media_count`) + CREATE `vw_media`; trigger `trg_media_assets_updated_at`.
