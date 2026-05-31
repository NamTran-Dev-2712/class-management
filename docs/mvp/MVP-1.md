# MVP-1 — Core Foundation

## 1. Context & Mục tiêu

MVP-1 xây dựng **nền tảng xác thực và phân quyền** cho toàn bộ hệ thống. Không có MVP-1, không có gì khác hoạt động được. Mục tiêu là tạo ra một hệ thống auth vững chắc, phân role rõ ràng, và cung cấp đầy đủ layout shell theo role để các MVP sau có thể gắn vào.

**Kết quả cụ thể:** Người dùng có thể đăng ký, đăng nhập, phân biệt Student / Teacher / Admin, và Admin có thể quản lý user + subject cơ bản.

---

## 2. Tiền điều kiện (Dependencies)

- Không có MVP nào phụ thuộc trước.
- Môi trường: .NET + PostgreSQL + Remix đã setup xong.
- Email service (SMTP/SendGrid) để gửi email đặt lại mật khẩu.

---

## 3. Trong scope

- Đăng ký tài khoản bằng email + password
- Đăng nhập bằng email + password
- JWT access token (ngắn hạn) + refresh token (dài hạn) rotation
- Đăng xuất (revoke refresh token)
- Quên mật khẩu → nhận email → đặt lại mật khẩu (link có TTL)
- 3 role hệ thống: `Student`, `Teacher`, `Admin`
- Đăng ký mặc định là `Student`; Admin set role thủ công
- Xem và chỉnh sửa profile cá nhân (tên hiển thị, avatar, bio)
- Đổi mật khẩu khi đã đăng nhập
- Layout shell riêng biệt cho từng role (sidebar/navbar khác nhau)
- Admin: danh sách user, tìm kiếm/lọc, xem thông tin user, khoá / mở tài khoản
- Admin: CRUD Subject (môn học) — tên, mô tả, trạng thái active/inactive

---

## 4. Ngoài scope (Out of Scope)

- OAuth social login (Google, Facebook, Zalo) → ROADMAP-FUTURE
- Xác minh email khi đăng ký (email verification) → có thể làm ở MVP-7 hardening
- 2FA / MFA → ROADMAP-FUTURE
- Admin phân role Teacher qua flow approval phức tạp — ở MVP này Admin set role trực tiếp
- Premium / subscription → MVP-8
- Notification → MVP-7
- Mobile app → ROADMAP-FUTURE

---

## 5. User Stories

### Student

| ID | Story |
|---|---|
| S1-01 | Là Student, tôi muốn đăng ký tài khoản bằng email + password, để có thể sử dụng hệ thống. |
| S1-02 | Là Student, tôi muốn đăng nhập, để truy cập các tính năng dành cho mình. |
| S1-03 | Là Student, tôi muốn đặt lại mật khẩu qua email khi quên, để không bị mất quyền truy cập. |
| S1-04 | Là Student, tôi muốn xem và chỉnh sửa profile của mình, để cập nhật thông tin cá nhân. |
| S1-05 | Là Student, tôi muốn đổi mật khẩu khi đã đăng nhập, để bảo mật tài khoản. |

### Teacher

| ID | Story |
|---|---|
| T1-01 | Là Teacher, tôi muốn đăng ký và được Admin nâng role, để có thể sử dụng các tính năng giảng dạy. |
| T1-02 | Là Teacher, tôi muốn có layout riêng biệt phù hợp với workflow giảng dạy, để điều hướng nhanh. |

### Admin

| ID | Story |
|---|---|
| A1-01 | Là Admin, tôi muốn xem danh sách tất cả user có phân trang và tìm kiếm, để quản lý tài khoản hiệu quả. |
| A1-02 | Là Admin, tôi muốn khoá/mở tài khoản user, để kiểm soát quyền truy cập. |
| A1-03 | Là Admin, tôi muốn thay đổi role của user (Student ↔ Teacher), để cấp quyền giảng dạy. |
| A1-04 | Là Admin, tôi muốn tạo, sửa, xoá Subject (môn học), để Teacher có thể gắn lớp học với môn học. |
| A1-05 | Là Admin, tôi muốn tìm kiếm Subject và bật/tắt trạng thái active, để quản lý danh mục môn học. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-1-01 | Email phải unique trong hệ thống. Đăng ký email đã tồn tại → trả lỗi rõ ràng, không để lộ "email đã tồn tại" hay không (chống enumeration: trả về thông báo chung "nếu email đúng, bạn sẽ nhận được mail"). |
| BR-1-02 | Password phải tối thiểu 8 ký tự, có ít nhất 1 chữ hoa, 1 chữ thường, 1 số. |
| BR-1-03 | Refresh token có TTL cố định (vd: 7 ngày). Khi dùng refresh token thì rotate (invalidate token cũ, cấp token mới). |
| BR-1-04 | Link đặt lại mật khẩu có TTL tối đa 15 phút và chỉ dùng được 1 lần. |
| BR-1-05 | Tài khoản bị khoá không thể đăng nhập; refresh token hiện tại bị revoke ngay khi Admin khoá. |
| BR-1-06 | Đăng ký mặc định là role `Student`. Admin không thể tự thay đổi role của chính mình. |
| BR-1-07 | Subject phải có tên unique. Subject `inactive` vẫn tồn tại trong DB nhưng không hiển thị ở các dropdown tạo Class (MVP-2). |
| BR-1-08 | Không được xoá Subject nếu đang có Class đang dùng Subject đó. Phải set `inactive` thay vào đó. |

---

## 7. States & Workflows

### User Account State

```
Registered
    │
    ├──[Admin locks]──► Locked ──[Admin unlocks]──► Active
    │
    └──► Active (default sau đăng ký)
```

### Password Reset Flow

```
User nhập email
    │
    ▼
Hệ thống tạo reset token (TTL 15 phút) → gửi email
    │
    ▼
User click link → kiểm tra token hợp lệ & chưa hết hạn
    │
    ├──[hết hạn / đã dùng]──► Báo lỗi, yêu cầu thử lại
    │
    └──[hợp lệ]──► User nhập password mới → lưu hash → token bị invalidate
```

### Refresh Token Rotation

```
Access token hết hạn
    │
    ▼
Client gửi refresh token
    │
    ├──[token hợp lệ]──► Cấp access token mới + refresh token mới (rotate)
    │                       Token cũ bị invalidate ngay
    │
    └──[token không hợp lệ/hết hạn]──► 401, yêu cầu đăng nhập lại
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `users` | Tài khoản người dùng: id, email, password_hash, display_name, avatar_url, bio, is_locked, created_at |
| `roles` | Bảng role: Student, Teacher, Admin |
| `user_roles` | Quan hệ nhiều-nhiều user ↔ role (1 user có thể có nhiều role trong tương lai) |
| `refresh_tokens` | Lưu refresh token hợp lệ: token_hash, user_id, expires_at, revoked_at |
| `password_reset_tokens` | Token đặt lại mật khẩu: token_hash, user_id, expires_at, used_at |
| `subjects` | Môn học: id, name, description, is_active |

---

## 9. Permission Matrix

| Action | Student | Teacher | Admin |
|---|---|---|---|
| Đăng ký / đăng nhập | Có | Có | Có |
| Xem profile của mình | Có | Có | Có |
| Sửa profile của mình | Có | Có | Có |
| Xem profile người khác | Không | Không | Có |
| Khoá/mở tài khoản | Không | Không | Có |
| Thay đổi role user | Không | Không | Có |
| CRUD Subject | Không | Không | Có |
| Xem danh sách Subject | Có (active only) | Có (active only) | Có (tất cả) |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Đăng ký email đã tồn tại | Trả về thông báo chung (không confirm email tồn tại — chống enumeration) |
| Đặt lại mật khẩu với link hết hạn | Báo lỗi rõ ràng, hướng dẫn yêu cầu link mới |
| Đặt lại mật khẩu với link đã dùng | Báo lỗi: "Link đã được sử dụng" |
| Admin khoá tài khoản đang đăng nhập | Revoke refresh token ngay; access token hết hạn tự nhiên sau TTL |
| Admin tự thay đổi role của mình | Chặn ở business logic — Admin không thể tự thay role |
| User đổi password mới trùng password cũ | Cho phép (không cần cấm) |
| Xoá Subject đang có Class dùng | Chặn, trả lỗi: "Môn học đang được sử dụng, không thể xoá" |
| Nhiều refresh token cùng tồn tại cho 1 user | Rotation invalidate token cũ. Nếu phát hiện dùng token đã bị invalidate → revoke TẤT CẢ token của user (phát hiện reuse attack) |

---

## 11. Acceptance Criteria

- [ ] User đăng ký thành công với email + password hợp lệ, nhận về JWT access token và refresh token.
- [ ] User đăng nhập sai password 3 lần liên tiếp → không bị khoá (rate limit ở MVP-7; ở MVP-1 chỉ cần trả lỗi bình thường).
- [ ] Link đặt lại mật khẩu hết hiệu lực sau 15 phút hoặc sau khi dùng 1 lần.
- [ ] Refresh token rotation: sau khi dùng, token cũ không thể dùng lại.
- [ ] Tài khoản bị Admin khoá không thể đăng nhập; refresh token bị revoke.
- [ ] Admin có thể CRUD Subject; Subject có Class đang dùng thì không xoá được.
- [ ] Layout khác nhau giữa Student, Teacher, Admin (route, sidebar, navbar).
- [ ] User không đăng nhập không thể truy cập bất kỳ route protected nào.
- [ ] User đăng nhập với role Student không thể truy cập route của Teacher/Admin.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Email service down → không gửi được link reset password | Trung bình | Queue email, retry logic; thông báo user "email sẽ đến trong vài phút" |
| Token leaking qua log | Cao | Không log raw token; chỉ log token_hash hoặc id |
| Refresh token reuse attack | Cao | Phát hiện reuse → revoke all tokens của user + notify (MVP-7) |
| Admin vô tình khoá tài khoản quan trọng | Thấp | Có confirm dialog; audit log ghi lại ai khoá lúc nào |

---

## 13. Verification Plan

**Scenario 1 — Đăng ký & đăng nhập cơ bản:**
1. Đăng ký tài khoản với email mới → nhận token → gọi API `/auth/me` thành công.
2. Đăng nhập với email/password sai → nhận 401.
3. Đăng nhập đúng → nhận access + refresh token.

**Scenario 2 — Quên mật khẩu:**
1. Gửi yêu cầu đặt lại → nhận email (kiểm tra inbox test).
2. Click link trong 15 phút → đặt mật khẩu mới thành công.
3. Click lại link cũ → nhận lỗi "link đã hết hạn / đã sử dụng".

**Scenario 3 — Admin quản lý user:**
1. Admin khoá tài khoản → user đó không đăng nhập được.
2. Admin mở khoá → user đăng nhập lại được.
3. Admin set role Teacher cho user → user thấy layout Teacher.

**Scenario 4 — Subject management:**
1. Admin tạo Subject "Toán" → xuất hiện trong danh sách.
2. Admin set Subject "inactive" → không xuất hiện ở Teacher dropdown tạo Class.
3. Admin xoá Subject đang có Class → nhận lỗi.

---

## 14. Hook sang MVP-2

MVP-2 (Classroom Management) sẽ dùng:
- `users` với role `Teacher` để tạo Class
- `users` với role `Student` để join Class
- `subjects` để gắn môn học với Class
- JWT auth middleware được thiết lập ở MVP-1 để bảo vệ mọi Class API
