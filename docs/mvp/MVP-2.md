# MVP-2 — Classroom Management

## 1. Context & Mục tiêu

MVP-2 xây dựng **hạt nhân của mô hình lớp học**: Teacher tạo và quản lý Class, Student tham gia qua mã invite, Teacher duyệt/từ chối. Đây là nền tảng để mọi Assignment, Attempt, và Report sau này đều gắn vào một Class cụ thể.

**Kết quả cụ thể:** Teacher có thể tạo lớp và quản lý danh sách học sinh; Student có thể tham gia lớp bằng mã. Cả hai đều thấy danh sách lớp và thành viên rõ ràng.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-1 hoàn thành**: auth JWT, phân role Student/Teacher/Admin, danh sách Subject.
- Teacher phải có role `Teacher` (Admin set ở MVP-1).

---

## 3. Trong scope

- Teacher CRUD Class (tên, mô tả, Subject, ảnh bìa tùy chọn)
- Mỗi Class thuộc 1 Subject
- Teacher sinh mã invite (6–8 ký tự, regenerate khi cần)
- Student tham gia Class bằng mã invite → trạng thái `Pending`
- Teacher duyệt (`approve`) hoặc từ chối (`reject`) từng Student
- Teacher kick Student đã được duyệt ra khỏi Class
- Danh sách thành viên Class (phân trang, tìm kiếm)
- Student xem danh sách Class mình đã được duyệt
- Student rời Class tự nguyện
- Class có trạng thái: `Active` / `Archived`
- Teacher archive/unarchive Class của mình
- Admin xem toàn bộ danh sách Class (read-only, không quản lý sâu — phần admin sâu hơn ở MVP-7)

---

## 4. Ngoài scope (Out of Scope)

- Co-teacher (nhiều giáo viên cùng quản lý 1 lớp) → dời sang sau MVP-2 (có thể làm trong sprint riêng sau khi MVP-2 stable)
- Real-time chat / messaging trong lớp → ROADMAP-FUTURE
- Thông báo realtime khi được duyệt → Notification đơn giản làm ở MVP-7; MVP-2 chỉ cần polling hoặc redirect
- Import danh sách học sinh từ Excel → ROADMAP-FUTURE
- Google Classroom sync → ROADMAP-FUTURE
- Class có nhiều Teacher ngay từ đầu → Out of scope

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T2-01 | Là Teacher, tôi muốn tạo một Class mới với tên, mô tả, và môn học, để bắt đầu quản lý lớp. |
| T2-02 | Là Teacher, tôi muốn chỉnh sửa thông tin Class của mình, để cập nhật khi cần thiết. |
| T2-03 | Là Teacher, tôi muốn sinh mã invite và chia sẻ với học sinh, để họ có thể tham gia lớp. |
| T2-04 | Là Teacher, tôi muốn regenerate mã invite khi cần, để thu hồi quyền tham gia của mã cũ. |
| T2-05 | Là Teacher, tôi muốn xem danh sách học sinh đang chờ duyệt, để xử lý yêu cầu tham gia. |
| T2-06 | Là Teacher, tôi muốn duyệt hoặc từ chối từng học sinh, để kiểm soát thành phần lớp. |
| T2-07 | Là Teacher, tôi muốn xem danh sách tất cả thành viên đã được duyệt, để quản lý lớp học. |
| T2-08 | Là Teacher, tôi muốn kick một học sinh ra khỏi lớp, để xử lý trường hợp đặc biệt. |
| T2-09 | Là Teacher, tôi muốn archive lớp học khi kết thúc kỳ học, để giữ gọn danh sách lớp đang hoạt động. |
| T2-10 | Là Teacher, tôi muốn xem danh sách tất cả lớp của mình (active + archived), để quản lý tổng thể. |

### Student

| ID | Story |
|---|---|
| S2-01 | Là Student, tôi muốn nhập mã invite để yêu cầu tham gia lớp, để học online với giáo viên. |
| S2-02 | Là Student, tôi muốn xem trạng thái yêu cầu tham gia (Pending / Approved / Rejected), để biết kết quả. |
| S2-03 | Là Student, tôi muốn xem danh sách các lớp mình đã được duyệt, để điều hướng vào đúng lớp. |
| S2-04 | Là Student, tôi muốn xem danh sách thành viên trong lớp mình đang học, để biết ai học chung. |
| S2-05 | Là Student, tôi muốn rời lớp tự nguyện, để không còn nhận Assignment từ lớp đó. |

### Admin

| ID | Story |
|---|---|
| A2-01 | Là Admin, tôi muốn xem danh sách tất cả Class trong hệ thống, để có cái nhìn tổng quan. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-2-01 | Một Class phải gắn với đúng 1 Subject. Subject phải có trạng thái `active`. |
| BR-2-02 | Mã invite phải unique trong hệ thống tại thời điểm sinh. Khi regenerate, mã cũ ngay lập tức mất hiệu lực. |
| BR-2-03 | Student chỉ được gửi 1 yêu cầu tham gia 1 Class (không gửi lại khi đang `Pending` hoặc đã `Approved`). Nếu bị `Rejected`, có thể thử lại. |
| BR-2-04 | Teacher chỉ quản lý được Class do mình tạo (owner). Không thể sửa/xoá Class của giáo viên khác. |
| BR-2-05 | Không thể xoá Class vĩnh viễn — chỉ được Archive. Lý do: Assignment và Attempt đã tạo trong Class cần được giữ lại. |
| BR-2-06 | Student bị kick ra khỏi Class vẫn xem được kết quả Attempt cũ của mình (đọc only). |
| BR-2-07 | Student rời Class tự nguyện tương đương bị kick — không xoá data, vẫn giữ Attempt cũ. |
| BR-2-08 | Class `Archived` vẫn hiển thị ở danh sách với filter riêng; không nhận Assignment mới. |
| BR-2-09 | Giới hạn số Class Teacher có thể tạo theo config hệ thống (chuẩn bị cho premium MVP-8). Mặc định unlimited trong MVP-2. |
| BR-2-10 | Student đã bị `Rejected` không được tự động re-join; phải nhập mã invite lại. |

---

## 7. States & Workflows

### Class State

```
Draft (không dùng trong MVP-2, để dành nếu cần)
    │
Active ──[Teacher archives]──► Archived ──[Teacher unarchives]──► Active
```

### Class Membership State (Student)

```
[Nhập mã invite]
    │
    ▼
Pending ──[Teacher approves]──► Approved (ClassMember)
    │
    └──[Teacher rejects]──► Rejected ──[Student nhập lại mã]──► Pending (lần mới)

Approved ──[Teacher kicks]──► Removed (vẫn giữ data cũ, không hiện trong member list)
Approved ──[Student leaves]──► Left (tương đương Removed)
```

### Mã invite lifecycle

```
Generated ──[regenerate]──► Invalidated (mã cũ)
                              Và một Generated mới được tạo
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `classes` | id, name, description, subject_id, owner_id (teacher), invite_code, status (Active/Archived), cover_image_url, created_at |
| `class_memberships` | id, class_id, student_id, status (Pending/Approved/Rejected/Removed/Left), joined_at, processed_at |

---

## 9. Permission Matrix

| Action | Student | Teacher (owner) | Teacher (khác) | Admin |
|---|---|---|---|---|
| Tạo Class | Không | Có | N/A | Không |
| Sửa thông tin Class | Không | Có | Không | Không |
| Archive/Unarchive Class | Không | Có | Không | Không |
| Xem Class details | Chỉ lớp đã được duyệt | Có | Không | Có |
| Xem danh sách member | Chỉ lớp đã được duyệt | Có | Không | Có |
| Duyệt/từ chối Student | Không | Có | Không | Không |
| Kick Student | Không | Có | Không | Không |
| Xem mã invite | Không | Có | Không | Không |
| Regenerate mã invite | Không | Có | Không | Không |
| Join Class (nhập mã) | Có | Không | Không | Không |
| Rời Class | Có (lớp mình đang học) | N/A | N/A | Không |
| Xem danh sách tất cả Class | Không | Không | Không | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Nhập sai mã invite | Trả lỗi: "Mã lớp không tồn tại hoặc đã hết hiệu lực" |
| Student nhập mã của Class đã Archived | Từ chối: "Lớp học đã kết thúc, không nhận thêm thành viên mới" |
| Student cố join lớp đang Pending | Báo: "Yêu cầu của bạn đang chờ được duyệt" |
| Teacher xoá lớp (action không hỗ trợ) | UI không có nút xoá, chỉ có Archive |
| Student rời lớp khi có Assignment đang `Open` và Attempt đang `InProgress` | Attempt vẫn tồn tại và có thể submit; sau khi rời, Student không nhận Assignment mới của lớp đó |
| Teacher kick Student đang có Attempt `InProgress` | Kick thành công; Attempt vẫn tồn tại và Student vẫn nộp được nếu deadline chưa qua |
| Regenerate mã invite khi có Student đang hold mã cũ | Mã cũ mất hiệu lực; Student dùng mã cũ nhận lỗi rõ ràng |
| Teacher vô tình archive lớp đang có Assignment `Open` | Cảnh báo: "Lớp đang có X bài đang mở, bạn có chắc muốn archive?" — Teacher confirm mới thực hiện |
| Subject của Class bị set inactive sau khi Class tạo | Class vẫn hoạt động bình thường; chỉ ảnh hưởng khi tạo Class mới |

---

## 11. Acceptance Criteria

- [ ] Teacher tạo Class thành công → xuất hiện trong danh sách lớp của Teacher.
- [ ] Mã invite sinh ra là unique và có thể chia sẻ.
- [ ] Student nhập mã đúng → trạng thái `Pending` xuất hiện trong danh sách chờ duyệt của Teacher.
- [ ] Teacher duyệt Student → Student thấy Class trong danh sách lớp của mình.
- [ ] Teacher từ chối Student → Student nhận thông báo (hoặc thấy status `Rejected`).
- [ ] Regenerate mã → mã cũ không còn hoạt động.
- [ ] Teacher kick Student → Student không còn trong danh sách member, nhưng data cũ vẫn giữ.
- [ ] Teacher archive Class → Class vẫn hiện trong tab "Archived", Student không thấy ở danh sách lớp đang học.
- [ ] Student không thể truy cập Class chưa được duyệt (kể cả biết ID của Class đó).
- [ ] Admin xem được danh sách tất cả Class.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Mã invite bị brute-force | Trung bình | Mã đủ dài (8 ký tự alphanumeric = 2.8 tỷ combo); rate limit ở MVP-7; có thể thêm CAPTCHA sau |
| Mã invite bị share rộng ngoài ý muốn | Thấp | Teacher có thể regenerate bất kỳ lúc nào; cân nhắc cho phép Teacher set thời hạn mã (future) |
| Teacher archive lớp khi có Attempt đang diễn ra | Trung bình | Warning dialog rõ ràng trước khi archive |

---

## 13. Verification Plan

**Scenario 1 — Class lifecycle:**
1. Teacher tạo Class "Toán 10A1" gắn Subject "Toán" → Class xuất hiện ở danh sách active.
2. Teacher archive Class → chuyển sang tab "Archived".
3. Teacher unarchive → trở về Active.

**Scenario 2 — Student join flow:**
1. Student nhập mã invite đúng → status Pending.
2. Teacher thấy Student trong danh sách chờ duyệt → approve.
3. Student thấy Class trong danh sách lớp của mình.
4. Teacher reject Student khác → Student đó thấy status Rejected.

**Scenario 3 — Invite code management:**
1. Teacher regenerate mã → mã mới được sinh.
2. Student dùng mã cũ → nhận lỗi.
3. Student dùng mã mới → join thành công.

**Scenario 4 — Permission check:**
1. Student cố truy cập Class chưa được duyệt qua URL → 403.
2. Teacher khác cố sửa Class không thuộc mình → 403.

---

## 14. Hook sang MVP-3

MVP-3 (Question Bank) xây dựng độc lập với MVP-2 (không phụ thuộc Class). Tuy nhiên, từ MVP-5 trở đi, mọi Assignment sẽ gắn với một `class_id` được thiết lập ở MVP-2. MVP-4 (Exam) cũng độc lập với Class. Cả MVP-3 và MVP-4 có thể làm song song với MVP-2 ở các sprint riêng.
