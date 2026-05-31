# MVP-7 — Admin & Moderation

## 1. Context & Mục tiêu

MVP-7 là **milestone production-ready** — sau khi hoàn thành MVP-7, hệ thống có thể deploy ra production cho người dùng thực tế sử dụng. MVP-7 bổ sung 4 nhóm tính năng quan trọng:

1. **Admin dashboard & governance**: Admin thấy được toàn bộ hệ thống và xử lý vi phạm.
2. **Audit log**: Mọi hành động nhạy cảm đều được ghi lại, không thể xoá.
3. **Notification system**: Người dùng nhận thông báo trong app cho các sự kiện quan trọng.
4. **Production hardening**: Rate limiting, error tracking, input validation chặt, security headers.

**Kết quả cụ thể:** Hệ thống đủ tin cậy để vận hành với người dùng thực, Admin có đủ công cụ quản trị, và có nền tảng để mở rộng tính năng sau này.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-1 → MVP-6 hoàn thành**: Toàn bộ core features đã stable.
- Môi trường production: HTTPS, domain, infrastructure monitoring.

---

## 3. Trong scope

### Admin Dashboard & Governance
- Admin dashboard: thống kê tổng quan (tổng user, user mới 7 ngày, lớp đang active, report chờ xử lý)
- Admin xem danh sách tất cả User (mở rộng từ MVP-1), lọc theo role, trạng thái
- Admin xem danh sách tất cả Class (có thể xem thành viên, xem Assignment count)
- Admin xem tài nguyên Public (Question, Exam) toàn hệ thống
- Admin xem toàn bộ Assignment và Attempt (read-only, không sửa)
- **System settings**: cấu hình giới hạn tài nguyên (max_classes_per_teacher, max_questions_per_teacher, max_exams_per_teacher) — chuẩn bị cho premium MVP-8
- Admin force-close Assignment (trường hợp khẩn cấp)
- Admin xem Audit Log

### Report & Moderation Flow
- Bất kỳ user nào có thể report một trong các target sau:
  - `Question` — nội dung không phù hợp
  - `Exam` — nội dung không phù hợp
  - `Assignment` — cấu hình sai, gian lận
  - `Class` — tên/mô tả không phù hợp
  - `User` — hành vi vi phạm
- Report states: `Pending` → `Reviewing` → `Resolved` / `Rejected`
- Admin xem danh sách Report, lọc theo target type và status
- Admin thực hiện action: `Dismiss` (bỏ qua) / `WarnUser` (cảnh báo) / `HideContent` (ẩn nội dung) / `DeleteContent` (xoá) / `BanUser` (khoá tài khoản)
- Reporter nhận thông báo khi Report được xử lý

### Audit Log
Ghi log tất cả hành động nhạy cảm sau (không thể xoá / sửa bởi bất kỳ ai):

| Hành động | Ai có thể làm |
|---|---|
| Đăng nhập thành công / thất bại | User |
| Đăng xuất | User |
| Thay đổi role user | Admin |
| Khoá/mở tài khoản | Admin |
| Tạo/sửa/xoá Question | Teacher |
| Sửa đáp án đúng trong Question | Teacher |
| Tạo/sửa/xoá Exam | Teacher |
| Publish Assignment | Teacher |
| Đóng Assignment | Teacher |
| Student Start Attempt | Student |
| Student Submit Attempt | Student |
| Teacher nhập điểm tự luận | Teacher |
| Teacher sửa điểm đã chấm | Teacher |
| Teacher công bố điểm | Teacher |
| Admin xử lý Report | Admin |
| Thay đổi System Settings | Admin |
| Subscription thay đổi (MVP-8 chuẩn bị) | System |

### Notification System
Notification in-app (không phải real-time WebSocket ở MVP-7 — polling hoặc SSE đơn giản):

| Event | Nhận thông báo |
|---|---|
| `ClassJoinApproved` | Student |
| `ClassJoinRejected` | Student |
| `AssignmentCreated` | Student (tất cả member lớp) |
| `AssignmentDueSoon` (24h trước deadline) | Student chưa nộp |
| `GradePublished` | Student |
| `PendingGradingReminder` | Teacher (có bài chưa chấm > 24h) |
| `ReportResolved` | Reporter |
| `ReportReceived` | Admin |
| `SystemAnnouncement` | Tất cả hoặc role cụ thể |

Notification states: `Unread` → `Read` → `Archived`

### Production Hardening
- **Rate limiting**: giới hạn request per IP và per user (đặc biệt auth endpoints)
- **Input validation**: validate chặt tất cả input ở API layer, không tin tưởng client
- **Security headers**: HTTPS only, HSTS, Content-Security-Policy, X-Frame-Options
- **Error handling**: centralized error handler, không expose stack trace cho client
- **Health check endpoint**: `/health` cho load balancer / monitoring
- **Structured logging**: mọi request/error log theo format chuẩn (JSON)

---

## 4. Ngoài scope (Out of Scope)

- Real-time WebSocket notifications → có thể upgrade sau (polling đủ cho MVP-7)
- Email notifications → có thể thêm ở phase sau (chỉ in-app ở MVP-7)
- Advanced analytics / BI dashboard → ROADMAP-FUTURE
- Automated content moderation (AI) → ROADMAP-FUTURE
- GDPR compliance / data export cho user → phase sau nếu cần
- Multi-tenant / organization-level admin → ROADMAP-FUTURE
- Dark mode admin panel → không ưu tiên

---

## 5. User Stories

### Admin

| ID | Story |
|---|---|
| A7-01 | Là Admin, tôi muốn xem dashboard tổng quan hệ thống, để nắm tình trạng hoạt động. |
| A7-02 | Là Admin, tôi muốn xem danh sách tất cả Report theo trạng thái và loại, để xử lý vi phạm kịp thời. |
| A7-03 | Là Admin, tôi muốn xem chi tiết Report cùng nội dung bị báo cáo, để đánh giá chính xác. |
| A7-04 | Là Admin, tôi muốn thực hiện action với Report (Warn/Hide/Delete/Ban), để xử lý vi phạm. |
| A7-05 | Là Admin, tôi muốn xem Audit Log với filter theo hành động/user/thời gian, để điều tra sự cố. |
| A7-06 | Là Admin, tôi muốn cấu hình giới hạn tài nguyên (max class, max question…), để chuẩn bị cho plan premium. |
| A7-07 | Là Admin, tôi muốn force-close Assignment trong trường hợp khẩn cấp (phát hiện gian lận), để kiểm soát tình huống. |
| A7-08 | Là Admin, tôi muốn gửi SystemAnnouncement đến tất cả user hoặc role cụ thể, để thông báo bảo trì/cập nhật. |

### Teacher

| ID | Story |
|---|---|
| T7-01 | Là Teacher, tôi muốn nhận thông báo khi học sinh tham gia yêu cầu vào lớp, để duyệt kịp thời. |
| T7-02 | Là Teacher, tôi muốn nhận nhắc nhở khi có bài tự luận chưa chấm quá 24h, để không bỏ sót. |
| T7-03 | Là Teacher, tôi muốn report nội dung vi phạm (câu hỏi public, exam public của người khác), để bảo vệ chất lượng hệ thống. |

### Student

| ID | Story |
|---|---|
| S7-01 | Là Student, tôi muốn nhận thông báo khi được duyệt vào lớp, để biết có thể bắt đầu học. |
| S7-02 | Là Student, tôi muốn nhận thông báo khi có Assignment mới, để không bỏ lỡ bài. |
| S7-03 | Là Student, tôi muốn nhận thông báo khi bài được chấm và điểm được công bố, để xem kết quả. |
| S7-04 | Là Student, tôi muốn nhận thông báo nhắc nhở 24h trước deadline nếu chưa nộp, để không bỏ lỡ. |
| S7-05 | Là Student/Teacher, tôi muốn report nội dung vi phạm trong hệ thống, để góp phần giữ môi trường học tập lành mạnh. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-7-01 | Audit log là **append-only**: không cho phép update hoặc delete. Admin xem được nhưng không sửa được. |
| BR-7-02 | Một user chỉ có thể submit 1 Report cho cùng 1 target (idempotent report). |
| BR-7-03 | `BanUser` action từ Admin → revoke tất cả refresh token của user đó ngay lập tức. |
| BR-7-04 | `HideContent` action → nội dung bị ẩn khỏi tất cả user nhưng vẫn tồn tại trong DB (soft hide). |
| BR-7-05 | `DeleteContent` action → xoá nội dung; nếu là Question đang trong Exam active → không cho xoá, chỉ hide. |
| BR-7-06 | System Settings chỉ có 1 record active. Thay đổi Settings tạo bản ghi mới trong audit log. |
| BR-7-07 | Notification `AssignmentDueSoon` chỉ gửi cho Student chưa có Attempt hoặc Attempt chưa Submit. Không gửi cho Student đã nộp. |
| BR-7-08 | Rate limit auth endpoints: max 5 lần đăng nhập thất bại / 5 phút / IP. Sau đó lock 15 phút. |
| BR-7-09 | API endpoints không được trả về stack trace hoặc internal error message trong production. |

---

## 7. States & Workflows

### Report State Machine

```
User submit Report
    │
    ▼
Pending ──[Admin nhận xét]──► Reviewing
    │                              │
    │                    ┌─────────┴──────────┐
    │                    │                    │
    │              [Admin xử lý]        [Admin bỏ qua]
    │                    │                    │
    │                    ▼                    ▼
    │                Resolved             Rejected
    │
    └──[không có Admin xử lý]──► Vẫn Pending (có Notification cho Admin)
```

### Notification lifecycle

```
Event xảy ra (vd: Assignment được tạo)
    │
    ▼
System tạo Notification record cho từng recipient
    │
    ▼
Unread ──[User xem]──► Read ──[User archive hoặc hệ thống expire]──► Archived
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `audit_logs` | id, action, actor_id, actor_role, target_type, target_id, metadata (JSON), ip_address, created_at — append-only |
| `reports` | id, reporter_id, target_type, target_id, reason, description, status, admin_id, admin_action, admin_note, resolved_at, created_at |
| `notifications` | id, user_id, event_type, title, body, link, status (Unread/Read/Archived), created_at, read_at |
| `system_settings` | id, key, value, updated_by, updated_at — bảng key-value cho config hệ thống |

---

## 9. Permission Matrix

| Action | Student | Teacher | Admin |
|---|---|---|---|
| Submit Report | Có | Có | Có |
| Xem Report của mình | Có | Có | Có (tất cả) |
| Xử lý Report | Không | Không | Có |
| Xem Audit Log | Không | Không | Có |
| Xem Notification của mình | Có | Có | Có |
| Gửi SystemAnnouncement | Không | Không | Có |
| Xem System Settings | Không | Không | Có |
| Thay đổi System Settings | Không | Không | Có |
| Force-close Assignment | Không | Không | Có |
| Xem Admin Dashboard | Không | Không | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| User report nội dung của chính mình | Cho phép (edge case ít gặp; Admin xử lý theo ngữ cảnh) |
| User report cùng 1 target 2 lần | Idempotent: trả về report đã tồn tại, không tạo mới |
| Admin xoá Question đang trong Assignment active | Từ chối: "Câu hỏi đang trong Assignment đang mở" |
| Admin Ban user đang có Attempt `InProgress` | Revoke token ngay; Attempt sẽ được auto-submit khi hết giờ hoặc bị bỏ |
| Notification flood: Assignment tạo cho lớp 500 học sinh | Batch insert notifications; không block request của Teacher |
| `AssignmentDueSoon` background job chạy khi server restart | Idempotent: check `sent_at` trước khi gửi để không gửi 2 lần |
| Audit log query với filter rộng → slow | Index theo `actor_id`, `target_type`, `created_at`; paginate bắt buộc |

---

## 11. Acceptance Criteria

- [ ] Admin dashboard hiển thị đúng các số liệu tổng quan.
- [ ] User submit Report → Admin thấy trong danh sách Pending.
- [ ] Admin xử lý Report (Warn/Hide/Delete/Ban) → Reporter nhận Notification.
- [ ] Audit log ghi lại: Teacher sửa điểm → Admin xem thấy entry với actor, time, old value, new value.
- [ ] Rate limit: sau 5 lần đăng nhập sai → IP bị lock 15 phút.
- [ ] Notification `AssignmentCreated` gửi đến tất cả Student của lớp.
- [ ] Notification `AssignmentDueSoon` không gửi cho Student đã Submit.
- [ ] `GradePublished` notification → Student nhận → xem điểm thành công.
- [ ] API trả về lỗi chuẩn (không có stack trace) trong production mode.
- [ ] Health check endpoint `/health` → 200 OK.
- [ ] System Settings thay đổi → audit log ghi lại.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Audit log phình to → query chậm | Trung bình | Index tốt; archive logs cũ > 1 năm sang cold storage |
| Notification queue lag → student nhận thông báo trễ | Trung bình | Background job; trong MVP-7 chấp nhận độ trễ vài giây |
| Admin vô tình Ban nhầm user | Thấp | Confirm dialog; unban action có thể thực hiện lại |
| Report spam từ user độc hại | Thấp | Rate limit report (max 10 reports/ngày/user); idempotent check |
| Rate limit quá chặt chặn user hợp lệ | Trung bình | Config rate limit theo môi trường; whitelist IP nội bộ |

---

## 13. Verification Plan

**Scenario 1 — Admin governance:**
1. Tạo 5 User mới, 3 Class mới → Admin dashboard hiển thị đúng.
2. Admin force-close Assignment → status = Closed; tất cả Attempt InProgress bị auto-submit.

**Scenario 2 — Report flow:**
1. Student report 1 Question → Admin thấy trong Pending.
2. Admin chọn HideContent → Question bị ẩn; Reporter nhận Notification.
3. Student report lại Question đó → nhận response "đã report".

**Scenario 3 — Audit log:**
1. Teacher sửa điểm Attempt: từ 7 → 8 → audit log ghi entry với actor, target, old=7, new=8.
2. Admin xem Audit Log → filter theo teacher_id → thấy entry đó.

**Scenario 4 — Notification:**
1. Teacher tạo Assignment → 3 Student của lớp nhận Notification "Assignment mới".
2. 24h trước deadline: 2 Student chưa nộp nhận nhắc nhở; 1 Student đã nộp không nhận.
3. Teacher công bố điểm → 3 Student nhận Notification → xem điểm.

**Scenario 5 — Production hardening:**
1. Gửi 6 request login sai trong 5 phút từ 1 IP → lần 6 nhận 429 Too Many Requests.
2. Gọi API với input XSS (`<script>alert(1)</script>`) → sanitized hoặc rejected, không bị stored.
3. `/health` endpoint → 200 OK.

---

## 14. Hook sang MVP-8

MVP-7 là **production-ready milestone**. Sau MVP-7:
- Hệ thống sẵn sàng deploy và có người dùng thực sử dụng.
- `system_settings` với `max_classes_per_teacher`, `max_questions_per_teacher`... đã có — MVP-8 sẽ dùng để enforce limits theo plan.
- Audit log cho payment events đã được chuẩn bị trong schema.
- User-facing infrastructure (rate limit, auth, notification) đã stable → MVP-8 chỉ thêm payment layer lên trên.
