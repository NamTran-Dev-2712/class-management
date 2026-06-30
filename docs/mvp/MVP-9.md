# MVP-9 — Media & Rich Content Library

## 1. Context & Mục tiêu

Đến hết MVP-8, nội dung `Question` (và `QuestionOption`, explanation) chỉ là **Markdown text thuần** — không có bất kỳ hạ tầng upload/lưu trữ file nào trong hệ thống. Điều này khiến ngân hàng câu hỏi đơn điệu: không thể ra đề có hình minh hoạ (sơ đồ, biểu đồ, ảnh hình học), câu hỏi nghe (audio tiếng Anh), hay câu hỏi xem video.

MVP-9 bổ sung **năng lực lưu trữ media đầu tiên** của dự án dưới dạng một abstraction sạch, dễ thay nhà cung cấp — mô phỏng đúng pattern `IPaymentProvider`/`IPaymentProviderResolver` đã có ở MVP-8. Teacher có thể upload **ảnh / audio / video** và gắn vào Question, Option, explanation; media được phục vụ qua CDN; và khi Assignment được publish, media được **đóng băng vào snapshot** để bài đã/đang làm không bao giờ đổi.

Nhà cung cấp production mặc định là **Cloudflare R2** (truy cập qua AWS S3 SDK vì R2 tương thích S3) — chọn vì **egress miễn phí**, chi phí lưu trữ thấp (~$0.015/GB), phù hợp khi nhiều Student cùng tải media. Vì dùng S3 SDK nên có thể đổi sang AWS S3 / Backblaze B2 / MinIO mà không sửa code nghiệp vụ.

**Kết quả cụ thể:** Teacher upload ảnh/audio/video trực tiếp từ trình duyệt và chèn vào Question/Option/explanation; Student xem được media khi làm bài; media trong Assignment đã publish là bất biến; dung lượng lưu trữ bị giới hạn theo plan (Free/Pro).

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-3 (Question Bank)** hoàn thành: có `Question`/`QuestionOption` với content Markdown và editor `MarkdownEditor`.
- **MVP-5 (Assignment & Snapshot)** hoàn thành: cơ chế snapshot bất biến (`snapshot_questions`/`snapshot_options`) để gắn media frozen.
- **MVP-8 (Premium & Resource limit)** hoàn thành: `IResourceLimitService` + plan-based limits để áp quota dung lượng.
- Tài khoản Cloudflare R2 (hoặc một object storage tương thích S3) đã tạo bucket + API token; HTTPS + domain CDN cho media đã có.
- Renderer Markdown hiện tại đã cho phép thẻ `<img>` qua rehype-sanitize allowlist (cần mở rộng cho `<audio>`/`<video>`).

---

## 3. Trong scope

### Storage Provider Abstraction
- `IStorageProvider` (interface ở Application) + DTO nhỏ (`PresignUploadRequest`, `PresignUploadResult`, `StorageObjectRef`), `IStorageProviderResolver`, enum `StorageProvider` — mirror đúng `IPaymentProvider`/`PaymentProviderResolver`.
- Adapter `LocalStorageProvider` (dev/test): lưu file vào thư mục local + phục vụ static; bật khi `Storage:UseFakeProvider = true`.
- Adapter `R2StorageProvider` (prod): dùng AWS S3 SDK trỏ tới endpoint R2; secrets đọc từ config/env (`Storage:R2:*`), **không bao giờ lưu trong DB**.
- Đổi provider không chạm Application — y hệt cách swap Momo/VNPay.

### Upload trực tiếp browser → storage (presigned URL)
- Client gọi `POST /api/media/presign` → server validate (kind/MIME/size + quota) → trả presigned PUT URL + `storage_key` dự kiến.
- Client upload bytes **thẳng lên R2** (không proxy qua API .NET).
- Client gọi `POST /api/media` để confirm → server kiểm tra object đã tồn tại → tạo `MediaAsset` trạng thái `Confirmed`.
- Asset chỉ ở trạng thái `Pending` cho tới khi confirm thành công.

### MediaAsset & gắn vào câu hỏi
- Entity `MediaAsset` (module mới `Modules/Media`): owner, `media_kind` (Image/Audio/Video), content_type, byte_size, các thuộc tính tuỳ chọn (width/height/duration), `status`, soft-delete + audit. API chỉ expose qua `public_id`, không lộ `storage_key`.
- Hai cách gắn media (bổ trợ nhau):
  - **Inline Markdown**: `![alt](url)`, `<img>`, `<audio>`, `<video>` trong content/explanation/option — mở rộng allowlist rehype-sanitize cho `<audio>`/`<video>` an toàn (chỉ src/controls/type…).
  - **Attachment list**: danh sách media gắn ở cấp Question (cho media không chèn inline).
- `MarkdownEditor` thêm nút **Upload** (hiện chỉ có nút link) → mở dialog upload → chèn URL/Markdown vào vị trí con trỏ.

### Snapshot bất biến
- Khi `PublishAssignment`, mọi tham chiếu media được **đóng băng vào snapshot** (snapshot question/option lưu `public_id`/URL media tại thời điểm publish).
- Xoá/đổi media sau đó **không** ảnh hưởng Assignment đang/đã làm — giữ đúng nguyên tắc "Snapshot bất biến".

### Quota & giới hạn theo plan
- MIME allowlist + cap dung lượng mỗi file theo kind (image/audio/video) — là tunable (Options + `system_settings` live, theo pattern MVP-7.5).
- Tổng dung lượng lưu trữ mỗi Teacher resolve qua `IResourceLimitService` theo plan (Free vs Pro); `0/null = unlimited`. Quota check **tại bước presign**.

### Dọn dẹp (lifecycle)
- Hangfire recurring job (mirror các lifecycle job sẵn có) quét: asset `Pending` quá hạn (presign nhưng không confirm) và asset đã soft-delete → xoá object dưới storage.

### Admin
- Admin xem tổng quan dung lượng theo Teacher (read-only) và có thể gỡ media vi phạm (tái dùng moderation MVP-7).

---

## 4. Ngoài scope (Out of Scope)

- Transcoding / adaptive streaming video (HLS/DASH) — phục vụ file gốc qua HTML5 + CDN → ROADMAP-FUTURE
- Pipeline tạo thumbnail / biến đổi ảnh (resize, crop, watermark) → ROADMAP-FUTURE
- AI alt-text / auto-caption → ROADMAP-FUTURE
- DRM / chống tải media → ROADMAP-FUTURE
- Upload media trong bài làm của Student (đính kèm câu trả lời) → phase sau
- Thư viện media dùng chung giữa nhiều Teacher → phase sau

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T9-01 | Là Teacher, tôi muốn upload ảnh và chèn vào nội dung Question, để minh hoạ câu hỏi rõ hơn. |
| T9-02 | Là Teacher, tôi muốn upload file audio cho câu hỏi nghe, để ra đề tiếng Anh. |
| T9-03 | Là Teacher, tôi muốn upload video ngắn cho câu hỏi, để hỏi về nội dung video. |
| T9-04 | Là Teacher, tôi muốn gắn ảnh vào từng Option, để các lựa chọn có hình. |
| T9-05 | Là Teacher, tôi muốn biết mình đã dùng bao nhiêu dung lượng so với giới hạn plan, để quản lý chi phí. |
| T9-06 | Là Teacher, tôi muốn xoá media không dùng nữa, để dọn thư viện. |

### Student

| ID | Story |
|---|---|
| S9-01 | Là Student, tôi muốn xem ảnh/nghe audio/xem video trong câu hỏi khi làm bài, để hiểu đúng đề. |
| S9-02 | Là Student, tôi muốn media tải nhanh và ổn định, để không bị gián đoạn khi làm bài. |

### Admin

| ID | Story |
|---|---|
| A9-01 | Là Admin, tôi muốn xem dung lượng media theo Teacher, để theo dõi chi phí lưu trữ. |
| A9-02 | Là Admin, tôi muốn gỡ media vi phạm, để giữ nội dung sạch. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-9-01 | Mọi truy cập media đối ngoại chỉ qua `public_id` (hoặc URL CDN), không bao giờ lộ `storage_key` nội bộ. |
| BR-9-02 | Upload đi thẳng client → storage qua presigned URL; API không proxy bytes file. |
| BR-9-03 | Server validate kind/MIME/size **trước** khi cấp presign; `MediaAsset` chỉ `Confirmed` sau khi object thực sự tồn tại trên storage. |
| BR-9-04 | Tổng dung lượng media của Teacher không vượt quota theo plan (`0/null = unlimited`); kiểm tra tại bước presign. |
| BR-9-05 | Mỗi file không vượt cap theo kind (image/audio/video) cấu hình trong `system_settings`. |
| BR-9-06 | Khi Assignment publish, mọi media tham chiếu được đóng băng vào snapshot; sửa/xoá media sau đó không ảnh hưởng snapshot. |
| BR-9-07 | Chỉ owner (Teacher tạo media) hoặc Admin được xoá media. Student không bao giờ upload/xoá. |
| BR-9-08 | Asset `Pending` quá thời hạn xác nhận và asset đã soft-delete sẽ bị job dọn dẹp xoá object dưới storage. |
| BR-9-09 | Renderer media phải đi qua allowlist sanitize nghiêm ngặt (`<img>/<audio>/<video>` + thuộc tính an toàn); không bao giờ bypass sanitizer. |
| BR-9-10 | Secrets storage đọc từ config/env, không lưu DB; provider có thể swap không sửa Application. |

---

## 7. States & Workflows

### MediaAsset Lifecycle

```
[Teacher chọn file để upload]
    │
    ▼
[POST /api/media/presign]  ── validate kind/MIME/size + quota ──► (fail → 403/400)
    │ (ok)
    ▼
MediaAsset = Pending  +  presigned PUT URL
    │
    ▼
[Client PUT bytes thẳng lên R2]
    │
    ├──[POST /api/media confirm: object tồn tại]──► MediaAsset = Confirmed ──► chèn vào Question
    │
    └──[Không confirm trong N phút]──► Pending hết hạn ──[cleanup job]──► xoá object + asset
```

### Publish → Snapshot media freeze

```
Teacher publish Assignment
    │
    ▼
Copy exam_questions → snapshot_questions / snapshot_options
    │
    ▼
Mỗi media reference (inline + attachment) ghi public_id/URL vào snapshot
    │
    ▼
[Teacher xoá/đổi media gốc về sau]  ──►  Snapshot KHÔNG đổi  ──►  Attempt cũ/đang làm an toàn
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `media_assets` | id, public_id, owner_id (teacher), provider, storage_key, media_kind (Image/Audio/Video), content_type, byte_size, width?, height?, duration_seconds?, status (Pending/Confirmed), created_at/by, updated_at/by, deleted_at |
| `question_media` | (bảng nối, optional) id, question_id, media_id, role (inline/attachment), display_order — gắn media cấp Question/Option |
| `snapshot_question_media` | bản đóng băng media của snapshot: snapshot_question_id, media_public_id, frozen_url, media_kind, display_order |

> Ghi chú: nếu media chỉ dùng inline trong Markdown, có thể bỏ `question_media` và chỉ lưu URL trong content; bảng nối phục vụ attachment cấp-một (first-class). Quyết định cuối khi implement.

---

## 9. Permission Matrix

| Action | Student | Teacher | Admin |
|---|---|---|---|
| Xem media trong câu hỏi/bài làm | Có | Có | Có |
| Upload media (presign + confirm) | Không | Có | Có |
| Gắn media vào Question/Option | Không | Có (của mình) | Không |
| Xoá media | Không | Có (của mình) | Có (tất cả) |
| Xem dung lượng đã dùng | Không | Có (của mình) | Có (tất cả) |
| Gỡ media vi phạm | Không | Không | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Upload file vượt cap dung lượng/file | Từ chối ngay tại presign: 400 "File vượt giới hạn X MB cho loại Y" |
| Teacher vượt tổng quota plan | 403 "Bạn đã đạt giới hạn dung lượng. Nâng cấp Pro để thêm." + link upgrade |
| MIME không nằm trong allowlist | 400 "Định dạng không hỗ trợ"; chỉ chấp loại đã whitelist |
| Client presign rồi không upload/confirm | Asset giữ `Pending`; cleanup job xoá sau N phút |
| Confirm nhưng object chưa có trên storage | Từ chối confirm; giữ `Pending` để retry |
| Teacher xoá media đang dùng trong Question chưa publish | Cho phép; câu hỏi mất media (hoặc cảnh báo trước khi xoá) |
| Teacher xoá media đã nằm trong Assignment đã publish | Object gốc xoá nhưng snapshot giữ bản frozen → Attempt vẫn xem được (theo BR-9-06) |
| Content-type khai báo khác với file thực tế | Validate lại tại confirm (best-effort); từ chối nếu lệch nghiêm trọng |
| Video quá lớn gây tải chậm | Chỉ phục vụ file gốc + CDN; transcoding ngoài scope; cap kích thước file |

---

## 11. Acceptance Criteria

- [ ] Teacher upload ảnh qua presigned URL → chèn vào content Question → Student thấy ảnh khi làm bài.
- [ ] Teacher upload audio và video → render bằng `<audio>`/`<video>` an toàn (qua sanitizer).
- [ ] Upload đi thẳng client → R2, API không nhận bytes file.
- [ ] Asset chỉ `Confirmed` khi object tồn tại; asset `Pending` mồ côi bị cleanup job xoá.
- [ ] Vượt cap/file hoặc vượt quota plan → lỗi rõ ràng với link upgrade.
- [ ] Publish Assignment → xoá media gốc → Attempt cũ vẫn xem được media (snapshot frozen).
- [ ] Đổi provider sang Local (dev) hoạt động full flow offline mà không sửa Application.
- [ ] API không bao giờ trả `storage_key`; chỉ `public_id`/URL.
- [ ] Teacher xem được "đã dùng X / Y dung lượng".

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Lạm dụng upload (spam file lớn) làm phình chi phí | Cao | Quota theo plan + cap/file + rate-limit presign per-user; cleanup `Pending` |
| Upload nội dung độc hại / không phù hợp | Trung bình | MIME allowlist, sanitizer chặt, moderation MVP-7 cho gỡ media |
| Media gốc bị xoá làm hỏng đề cũ | Cao | Snapshot freeze media (BR-9-06) — đề đã publish không phụ thuộc media gốc |
| Egress phí cao nếu chọn nhầm provider | Trung bình | Mặc định R2 (egress miễn phí) + CDN; abstraction để đổi provider dễ |
| Presigned URL bị lộ/lạm dụng | Trung bình | TTL ngắn cho presign, scope đúng key, kiểm tra owner trước khi cấp |
| File giả mạo content-type | Thấp | Validate MIME tại presign + re-check tại confirm |

---

## 13. Verification Plan

**Scenario 1 — Upload ảnh happy path:**
1. Teacher mở editor → nút Upload → chọn ảnh.
2. Client gọi presign → upload thẳng R2 → confirm.
3. `MediaAsset = Confirmed`; ảnh chèn vào content.
4. Student làm bài → thấy ảnh tải từ CDN.

**Scenario 2 — Audio & video:**
1. Teacher upload audio + video → chèn `<audio>`/`<video>`.
2. Kiểm tra renderer chỉ cho thuộc tính an toàn (sanitizer).

**Scenario 3 — Quota & cap:**
1. Teacher Free upload tới hết quota → upload tiếp → 403 + link upgrade.
2. Upload 1 file vượt cap kind → 400.

**Scenario 4 — Snapshot freeze:**
1. Teacher tạo Question có ảnh → publish Assignment.
2. Student bắt đầu Attempt → xoá media gốc.
3. Student vẫn xem được ảnh (snapshot frozen); Attempt cũ không đổi.

**Scenario 5 — Cleanup mồ côi:**
1. Gọi presign nhưng không confirm.
2. Chạy cleanup job → object + asset `Pending` bị xoá.

**Scenario 6 — Provider swap:**
1. Bật `Storage:UseFakeProvider = true` (Local).
2. Chạy full flow offline → vẫn upload/confirm/render được, không sửa Application.

---

## 14. Hook sang MVP-10

MVP-9 làm câu hỏi đa dạng hơn (ảnh/audio/video) và đặt nền hạ tầng storage có thể tái dùng. MVP-10 tận dụng nền tảng Attempt sẵn có để **chống gian lận khi làm bài** (browser lockdown + log vi phạm). Hạ tầng storage của MVP-9 cũng là tiền đề cho các tính năng proctoring nâng cao bằng ảnh/video ở ROADMAP-FUTURE (camera-based proctoring 11.2).
