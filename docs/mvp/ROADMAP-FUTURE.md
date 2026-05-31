# ROADMAP-FUTURE — Tính năng mở rộng sau production

> File này tổng hợp các tính năng **sau MVP-8** — tức là sau khi hệ thống đã có người dùng thực, stable, và có monetization. Đây không phải cam kết triển khai ngay; đây là **vision** để thiết kế kiến trúc hiện tại không bị tắc nghẽn.
>
> Mỗi mục được đánh giá sơ bộ theo: **impact** (giá trị với user), **effort** (độ phức tạp kỹ thuật), và **dependency** (cần gì trước đó).

---

## Nhóm 1 — Authentication & Identity

### 1.1 OAuth Social Login (Google / Facebook / Zalo)

**Impact:** Cao — giảm friction đăng ký, đặc biệt với Zalo cho thị trường VN.

**Effort:** Trung bình — cần implement OAuth2 PKCE flow, xử lý account linking (email đã tồn tại), và merge account.

**Dependency:** MVP-1 (auth foundation)

**Ghi chú thiết kế:** Khi email OAuth trùng với email/password đã tồn tại, hiển thị flow "link account" thay vì tạo account mới. Tránh tạo duplicate accounts.

---

### 1.2 Two-Factor Authentication (2FA)

**Impact:** Trung bình — cần thiết cho Admin và Teacher.

**Effort:** Trung bình — TOTP (Google Authenticator), backup codes.

**Dependency:** MVP-1

---

## Nhóm 2 — Mobile App

### 2.1 Mobile App (React Native / Flutter)

**Impact:** Cao — học sinh làm bài trên di động là use case rất phổ biến ở VN.

**Effort:** Cao — cần thiết kế lại UX cho mobile, testing, app store review.

**Dependency:** MVP-5 (API ổn định cho Assignment & Attempt), MVP-7 (Notification)

**Ghi chú thiết kế:**
- Backend API phải RESTful và không có web-specific dependency.
- Notification chuyển sang Push Notification (Firebase FCM) thay vì chỉ in-app.
- Auto-save API đã có từ MVP-5 — chỉ cần consume ở mobile.
- Offline mode: cache Assignment data; sync khi có mạng trở lại.

---

## Nhóm 3 — AI Features

### 3.1 AI Question Generation

**Impact:** Cao — giúp Teacher tạo câu hỏi nhanh từ topic/content.

**Effort:** Trung bình — tích hợp LLM API (OpenAI, Gemini, hoặc self-hosted), prompt engineering, review flow.

**Dependency:** MVP-3 (Question Bank)

**Ghi chú thiết kế:**
- AI suggest → Teacher review → Teacher approve/edit → save vào bank.
- Không tự động save mà không có human review.
- Track token usage để kiểm soát chi phí.

---

### 3.2 AI Exam Assembly

**Impact:** Trung bình — AI gợi ý câu hỏi phù hợp theo topic, difficulty distribution.

**Effort:** Trung bình — cần embedding câu hỏi, semantic search.

**Dependency:** MVP-3, MVP-4, AI Question Generation (có thể độc lập)

---

### 3.3 AI-Assisted Grading (Short/Long Writing)

**Impact:** Cao — giảm tải giáo viên khi có nhiều bài tự luận.

**Effort:** Cao — LLM + rubric + human-in-the-loop flow, đảm bảo Teacher vẫn là người chấm cuối.

**Dependency:** MVP-6 (Manual Grading)

**Ghi chú thiết kế:**
- AI đề xuất điểm + lý do → Teacher xem xét → Teacher confirm hoặc override.
- Không cho AI tự động publish grade mà không có Teacher approval.

---

### 3.4 Personalized Learning Recommendations

**Impact:** Trung bình-Cao — gợi ý bài tập dựa trên pattern sai của Student.

**Effort:** Cao — cần data warehouse, ML pipeline.

**Dependency:** MVP-5, MVP-6 (đủ dữ liệu Attempt history)

---

## Nhóm 4 — Gamification

### 4.1 XP & Badges

**Impact:** Trung bình — tăng engagement cho Student.

**Effort:** Trung bình — định nghĩa event triggers, badge design, display trong profile.

**Dependency:** MVP-5, MVP-6

**Ghi chú thiết kế:**
- XP earned từ: nộp bài đúng hạn, điểm cao, streak, complete profile.
- Badge: "First submission", "Perfect score", "7-day streak", v.v.
- Không block core flows — gamification là overlay, không phải gate.

---

### 4.2 Class Leaderboard

**Impact:** Trung bình — tăng cạnh tranh lành mạnh trong lớp.

**Effort:** Thấp-Trung bình.

**Dependency:** MVP-5, MVP-6

**Ghi chú thiết kế:**
- Teacher có thể bật/tắt leaderboard per-Assignment hoặc per-Class.
- Privacy: chỉ hiển thị trong lớp, không public toàn hệ thống.

---

### 4.3 Daily Streak

**Impact:** Trung bình — tăng daily active users.

**Effort:** Thấp.

**Dependency:** MVP-5

---

## Nhóm 5 — Contest Module

### 5.1 Open Contest

**Impact:** Cao — mở rộng ngoài lớp học, thu hút user mới.

**Effort:** Cao — Contest là tính năng riêng biệt với Assignment: leaderboard public, registration flow khác, time-limited events.

**Dependency:** MVP-4 (Exam), MVP-5 (Attempt engine)

**Ghi chú thiết kế:**
- **Contest ≠ Assignment** — thiết kế riêng biệt, không tái dùng Assignment flow.
- Contest có: public registration, anonymous leaderboard (hoặc không anonymous), multiple rounds, prizes.
- Cần bảo mật chặt hơn vì contest public có thể bị abuse.

---

## Nhóm 6 — Advanced Question Types

### 6.1 Fill-in-the-Blank (Điền khuyết)

**Impact:** Trung bình.

**Effort:** Trung bình — validation đáp án phức tạp (case-insensitive, trim whitespace, multiple accepted answers).

**Dependency:** MVP-3

---

### 6.2 Drag-and-Drop / Ordering

**Impact:** Thấp.

**Effort:** Cao — phức tạp cả UI và grading logic.

**Dependency:** MVP-3

---

### 6.3 Matching (Nối đôi)

**Impact:** Thấp-Trung bình.

**Effort:** Trung bình.

**Dependency:** MVP-3

---

## Nhóm 7 — Advanced Grading

### 7.1 Partial Score cho MultipleChoice

**Impact:** Trung bình — fairness khi học sinh chọn đúng một phần.

**Effort:** Thấp-Trung bình — cần quyết định formula (vd: điểm × (correct_selected - incorrect_selected) / total_correct).

**Dependency:** MVP-5

---

### 7.2 Rubric-based Grading

**Impact:** Cao — giảng viên đại học và trường chuyên cần.

**Effort:** Trung bình-Cao — UI cho định nghĩa rubric, mapping câu hỏi → rubric criteria.

**Dependency:** MVP-6

---

### 7.3 Peer Review (Student chấm chéo)

**Impact:** Trung bình — cho bài tập nhóm hoặc kỹ năng đánh giá.

**Effort:** Cao — cần anonymous assignment, consensus scoring.

**Dependency:** MVP-6

---

## Nhóm 8 — Collaboration & Communication

### 8.1 In-Class Messaging

**Impact:** Trung bình — giáo viên và học sinh cần trao đổi về bài.

**Effort:** Cao — real-time WebSocket, message history, moderation.

**Dependency:** MVP-2 (Class), MVP-7 (Moderation)

---

### 8.2 Announcement / Bulletin Board

**Impact:** Trung bình — thay thế nhẹ hơn cho messaging.

**Effort:** Thấp — Teacher post announcement, Student xem, không cần real-time.

**Dependency:** MVP-2

---

### 8.3 Video Meeting Integration (Zoom / Google Meet)

**Impact:** Trung bình — hữu ích cho lớp học hybrid.

**Effort:** Thấp — chỉ cần embed meeting link trong Class; không host video tự.

**Dependency:** MVP-2

---

## Nhóm 9 — LMS Integration

### 9.1 Google Classroom Sync

**Impact:** Cao cho trường đang dùng Google Workspace.

**Effort:** Cao — OAuth với Google, sync Class/Student/Assignment bidirectional.

**Dependency:** MVP-2, MVP-5

---

### 9.2 SCORM / xAPI Support

**Impact:** Trung bình — enterprise/higher-ed use case.

**Effort:** Cao — chuẩn SCORM phức tạp.

**Dependency:** MVP-4, MVP-5

---

## Nhóm 10 — Advanced Payment & Business

### 10.1 Stripe (Credit Card Quốc tế)

**Impact:** Trung bình — dành cho giáo viên người nước ngoài hoặc trường quốc tế.

**Effort:** Trung bình — Stripe SDK, webhook, SCA compliance.

**Dependency:** MVP-8 (payment abstraction layer)

---

### 10.2 Trial Period & Coupon Codes

**Impact:** Cao — tăng conversion free → paid.

**Effort:** Thấp-Trung bình.

**Dependency:** MVP-8

---

### 10.3 B2B Licensing (Trường học mua gói cho cả giáo viên)

**Impact:** Cao — revenue model mạnh hơn.

**Effort:** Cao — organization entity, seat management, billing per organization.

**Dependency:** MVP-8, cần thiết kế lại data model cho multi-tenancy.

---

## Nhóm 11 — Advanced Proctoring

### 11.1 Tab Switch Detection

**Impact:** Trung bình.

**Effort:** Thấp — Page Visibility API; cảnh báo khi chuyển tab, ghi log.

**Dependency:** MVP-5

---

### 11.2 Camera-based Proctoring

**Impact:** Cao cho thi cử nghiêm túc.

**Effort:** Rất cao — WebRTC, AI face detection, privacy concerns, legal compliance.

**Dependency:** MVP-5, MVP-7 (audit log mạnh)

**Ghi chú:** Cần cân nhắc kỹ về quyền riêng tư và legal (GDPR, data locality VN).

---

## Ưu tiên gợi ý sau MVP-8

Nếu cần chọn next 3 items, gợi ý theo impact/effort ratio:

| # | Tính năng | Impact | Effort | Lý do ưu tiên |
|---|---|---|---|---|
| 1 | Mobile App | Cao | Cao | Use case chính của học sinh VN |
| 2 | OAuth Google | Cao | Trung bình | Giảm friction đăng ký rõ rệt |
| 3 | AI Question Generation | Cao | Trung bình | Differentiator mạnh, giảm workload teacher |

Sau đó tuỳ theo feedback từ người dùng thực để chọn hướng tiếp theo.
