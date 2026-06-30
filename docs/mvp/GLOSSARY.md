# GLOSSARY — Bảng thuật ngữ chuẩn

> File này là **single source of truth** cho tất cả thuật ngữ trong dự án.
> Mọi document, code, PR description đều phải dùng nhất quán theo bảng này.
> Khi cần thêm thuật ngữ mới, cập nhật file này trước.

---

## Thuật ngữ nghiệp vụ cốt lõi

| Thuật ngữ (EN) | Không dùng | Định nghĩa | Ví dụ |
|---|---|---|---|
| **Question** | câu hỏi đề thi, item | Một câu hỏi đơn lẻ trong ngân hàng câu hỏi của giáo viên. | "Tính đạo hàm của f(x) = x²" |
| **Exam** | đề thi, bài thi, đề, bài kiểm tra | Một bộ câu hỏi đóng vai trò **template/đề mẫu**. Exam chưa được giao cho học sinh. | "Đề ôn tập Chương 1 – Đại số" |
| **Assignment** | bài tập, bài giao, nhiệm vụ | Một lần Teacher giao **Exam** cho **Class** cụ thể, có thời hạn và cấu hình riêng. | "Giao Đề Chương 1 cho lớp 10A1, mở 20/12 – 25/12" |
| **Attempt** | bài làm, lần nộp | Một lần Student thực hiện (bắt đầu → nộp) một Assignment. | "Lần làm của Nam, 24/12 lúc 19:00, score = 8.5" |
| **Grade** | điểm, kết quả | Kết quả chấm điểm cuối cùng của một Attempt sau khi auto-grade + manual grade. | "8.5 / 10" |
| **Snapshot** | bản sao, copy | Bản sao **bất biến** (immutable) của Exam được tạo ra tại thời điểm Assignment được publish. Attempt luôn tham chiếu snapshot, không phải Exam gốc. | — |
| **Class** | lớp học, room | Một lớp học ảo do Teacher tạo, gắn với một Subject. Student tham gia qua mã invite. | "Lớp Toán 10A1 – HK1 2025" |
| **Subject** | môn học | Môn học (Toán, Văn, Anh…). Admin quản lý danh sách Subject. | "Toán", "Vật lý" |
| **Contest** | cuộc thi | Tính năng thi đua mở rộng với leaderboard, **khác với Assignment**. Không triển khai ở MVP-1 đến MVP-7. | — |

---

## Role & Permission

| Thuật ngữ | Định nghĩa |
|---|---|
| **Student** | Người dùng đã đăng ký với role học sinh. Tham gia Class, làm Attempt. |
| **Teacher** | Người dùng với role giáo viên. Tạo Class, Question, Exam, Assignment; chấm điểm. |
| **Admin** | Quản trị viên hệ thống. Quản lý User, Subject, lớp public, báo cáo vi phạm. |
| **ClassOwner** | Teacher đã tạo ra Class. Có toàn quyền với Class đó. |
| **ClassMember** | Student đã được duyệt vào Class. |
| **PendingMember** | Student đã gửi yêu cầu tham gia Class, chờ Teacher duyệt. |
| **Permission** | Quyền hạn cụ thể theo resource (vd: `class:update`, `question:delete`). Luôn check theo **resource ownership**, không chỉ check role. |

---

## Assignment & Attempt States

### Assignment Status

| State | Ý nghĩa |
|---|---|
| `Draft` | Teacher đang soạn, chưa giao. Student không thấy. |
| `Scheduled` | Đã lên lịch, chưa đến giờ mở. Student thấy nhưng chưa làm được. |
| `Open` | Đang mở, Student có thể bắt đầu làm. |
| `Closed` | Đã hết hạn. Student không thể bắt đầu mới; bài đang `InProgress` bị auto-submit. |
| `Archived` | Đã lưu trữ. Không hiện ở danh sách chính nhưng vẫn truy cập được điểm cũ. |

### Attempt Status

| State | Ý nghĩa |
|---|---|
| `NotStarted` | Assignment đang `Open` nhưng Student chưa bấm Start. |
| `InProgress` | Student đã bấm Start, đang làm, chưa Submit. |
| `Submitted` | Student đã Submit thủ công hoặc hệ thống auto-submit khi hết giờ. |
| `AutoGraded` | Toàn bộ câu hỏi là loại tự động (single/multi-choice, true/false), đã chấm xong, có Grade. |
| `NeedManualGrading` | Có ít nhất một câu Short/Long writing chưa được Teacher chấm. |
| `Graded` | Tất cả câu hỏi đã có điểm (auto + manual). Grade hoàn chỉnh. |
| `LateSubmitted` | Nộp sau thời hạn (áp dụng nếu Assignment có cấu hình `allow_late = true`). |
| `Expired` | Hết giờ nhưng Student chưa submit — hệ thống auto-submit và chuyển `Submitted`. |

---

## Loại câu hỏi (Question Types)

| Type | Chấm tự động | Mô tả |
|---|---|---|
| `SingleChoice` | Có | 1 đáp án đúng trong nhiều lựa chọn |
| `MultipleChoice` | Có | Nhiều đáp án đúng; chọn **đúng tất cả** mới tính điểm |
| `TrueFalse` | Có | Đúng / Sai |
| `ShortWriting` | Không | Tự luận ngắn — Teacher chấm thủ công |
| `LongWriting` | Không | Tự luận dài — Teacher chấm thủ công |
| `FillInBlank` | Có | Điền khuyết — content chứa chỗ trống `{{n}}`; mỗi blank có nhiều đáp án chấp nhận; chấm case-insensitive + trim. (MVP-11) |
| `Matching` | Có | Nối đôi — ghép phần tử vế trái ↔ vế phải; mặc định đúng-tất-cả mới tính điểm. (MVP-11) |
| `Ordering` | Có | Sắp xếp thứ tự các phần tử; mặc định đúng-toàn-bộ-thứ-tự mới tính điểm. (MVP-11) |

> **Lưu ý:** `FillInBlank` / `Matching` / `Ordering` được triển khai ở MVP-11 (chấm tự động), không nhầm với `ShortWriting`/`LongWriting` (chấm tay).

---

## Grading

| Thuật ngữ | Định nghĩa |
|---|---|
| **Auto-grade** | Hệ thống tự động chấm điểm các câu `SingleChoice`, `MultipleChoice`, `TrueFalse` ngay khi Attempt được Submit. |
| **Manual grade** | Teacher nhập điểm + feedback cho câu `ShortWriting` / `LongWriting`. |
| **Grade publishing** | Chính sách khi nào Student được xem điểm: `immediate` (ngay khi nộp) / `after_deadline` / `manual_release` (Teacher mở). |
| **Score policy** | Với Assignment có `max_attempts > 1`: `highest` (lấy điểm cao nhất), `latest` (lấy lần cuối). |
| **Retake** | Làm lại Assignment. Chỉ khi `max_attempts > 1`. Mỗi lần tạo ra Attempt mới độc lập. |

---

## Hệ thống & Technical

| Thuật ngữ | Định nghĩa |
|---|---|
| **Audit Log** | Bản ghi lịch sử mọi hành động nhạy cảm (sửa điểm, sửa đáp án, khoá tài khoản…). Không thể xoá bởi user thường. |
| **Notification** | Thông báo trong hệ thống (in-app). Có trạng thái: `Unread`, `Read`, `Archived`. |
| **Report** | Báo cáo vi phạm do user gửi cho admin xử lý. Có trạng thái: `Pending` → `Reviewing` → `Resolved` / `Rejected`. |
| **Subscription** | Gói dịch vụ trả phí của Teacher. Có trạng thái: `Active`, `PastDue`, `Cancelled`, `Expired`. (MVP-8) |
| **Premium plan** | Gói dịch vụ nâng cao (Free / Pro). Quy định giới hạn tài nguyên. (MVP-8) |
| **Idempotency** | Đảm bảo webhook thanh toán xử lý đúng dù gọi nhiều lần cùng 1 transaction. (MVP-8) |
| **Rate limit** | Giới hạn số lượng request/giây để bảo vệ API. |
| **JWT** | JSON Web Token — access token ngắn hạn + refresh token dài hạn cho auth. |
| **Auto-save** | Hệ thống tự động lưu đáp án đang làm của Student theo interval, tránh mất bài. |
| **MediaAsset** | Một file media (ảnh/audio/video) đã upload, expose qua `public_id`/URL, không lộ `storage_key`. (MVP-9) |
| **Storage provider** | Lớp trừu tượng lưu trữ file (`IStorageProvider`); production = Cloudflare R2 (S3-compatible), dev = Local. Đổi provider không sửa Application. (MVP-9) |
| **Presigned URL** | URL có chữ ký, TTL ngắn, cho client upload file thẳng lên storage không qua API. (MVP-9) |
| **Proctoring** | Giám sát hành vi làm bài bằng trình duyệt (lockdown) — best-effort, không tuyệt đối. (MVP-10) |
| **Browser lockdown** | Tập biện pháp phía trình duyệt: bắt buộc fullscreen, chặn copy/paste/right-click, phát hiện chuyển tab/mất focus. (MVP-10) |
| **ProctorEvent / Violation** | Một sự kiện liêm chính ghi server (`attempt_events`): TabSwitch/FullscreenExit/CopyAttempt… Server đếm `violation_count`; vượt ngưỡng → auto-submit/khoá. (MVP-10) |

---

## Những gì KHÔNG nên dùng

| Không dùng | Dùng thay |
|---|---|
| "bài thi" | **Assignment** (khi đã giao) hoặc **Exam** (khi là template) |
| "bài tập" | **Assignment** |
| "đề thi" | **Exam** |
| "đề" | **Exam** |
| "câu hỏi đề thi" | **Question** |
| "lần nộp" | **Attempt** |
| "điểm số" | **Grade** |
| "bản copy đề" | **Snapshot** |
| "cuộc thi / kỳ thi" | **Contest** (feature riêng, không nhầm với Assignment) |
