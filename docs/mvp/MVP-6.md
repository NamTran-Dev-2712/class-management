# MVP-6 — Manual Grading & Reports

## 1. Context & Mục tiêu

MVP-6 hoàn chỉnh vòng đời chấm điểm bằng cách thêm **chấm thủ công câu tự luận**, **kiểm soát thời điểm công bố điểm**, và **báo cáo tiến độ học tập**. Sau MVP-6, hệ thống đã có đầy đủ vòng lặp: giao bài → làm bài → chấm điểm → phản hồi → xem kết quả.

**Kết quả cụ thể:** Teacher chấm được câu tự luận với feedback; Student xem điểm khi được phép; Teacher xem báo cáo tổng hợp và export danh sách điểm.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-5 hoàn thành**: Attempt state machine, auto-grade, `grade_publish_policy` đã cấu hình.
- Có ít nhất 1 Attempt ở trạng thái `NeedManualGrading` hoặc `AutoGraded` để test.

---

## 3. Trong scope

### Manual Grading
- Teacher xem danh sách Attempt của Assignment, lọc theo trạng thái
- Teacher mở từng Attempt để chấm câu `ShortWriting` / `LongWriting`
- Teacher nhập điểm (số) và feedback text cho từng câu tự luận
- Sau khi Teacher chấm tất cả câu tự luận → Attempt state tự động → `Graded`
- Teacher có thể sửa điểm câu tự luận sau khi đã chấm (audit log ghi lại — MVP-7)
- Teacher xem câu trả lời tự luận của Student kèm context câu hỏi

### Grade Publishing
- Áp dụng `grade_publish_policy` đã cấu hình từ Assignment (MVP-5):
  - `immediate`: Student thấy điểm ngay sau khi Attempt → `AutoGraded` hoặc `Graded`
  - `after_deadline`: Student thấy điểm sau khi `closes_at` của Assignment đã qua
  - `manual`: Teacher bấm nút "Công bố điểm" cho Assignment → Student mới thấy
- Khi điểm chưa được công bố: Student thấy trạng thái "Đang chờ công bố điểm"
- Teacher luôn thấy điểm của tất cả Attempt bất kể policy

### Student View
- Student xem điểm Attempt của mình (nếu đã được phép theo policy)
- Student xem feedback Teacher cho từng câu tự luận
- Student xem đáp án đúng của câu khách quan (nếu Assignment có `show_answers_after_grade = true`)
- Student xem lịch sử tất cả Attempt của mình trong Assignment

### Reports & Analytics
- Teacher xem **báo cáo Assignment**: tổng số Student, số đã nộp, số chưa nộp, điểm trung bình, điểm max/min
- Teacher xem **danh sách điểm theo lớp**: bảng Student × điểm (hoặc "Chưa nộp")
- Teacher xem **biểu đồ phân phối điểm** (histogram — số học sinh theo từng khoảng điểm)
- Teacher xem tiến độ Student: danh sách Student chưa nộp bài (theo Assignment)
- Export bảng điểm ra CSV/Excel

---

## 4. Ngoài scope (Out of Scope)

- Rubric chấm điểm phức tạp (nhiều tiêu chí, sub-scores) → phase sau
- AI hỗ trợ chấm tự luận → ROADMAP-FUTURE
- Báo cáo tiến độ học kỳ (tổng hợp nhiều Assignment) → có thể thêm ở MVP-7 hoặc sau
- Phân tích câu hỏi (độ khó thực tế, tỷ lệ chọn đúng từng option) → phase sau
- Comment thread giữa Teacher và Student → ROADMAP-FUTURE
- Peer review (Student chấm chéo) → ROADMAP-FUTURE

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T6-01 | Là Teacher, tôi muốn xem danh sách tất cả Attempt của Assignment với trạng thái rõ ràng, để biết bài nào cần chấm. |
| T6-02 | Là Teacher, tôi muốn lọc Attempt theo trạng thái (NeedManualGrading, Graded, AutoGraded), để chỉ thấy bài cần chấm. |
| T6-03 | Là Teacher, tôi muốn mở Attempt của từng Student và thấy câu trả lời tự luận rõ ràng, để chấm điểm chính xác. |
| T6-04 | Là Teacher, tôi muốn nhập điểm số và comment phản hồi cho từng câu tự luận, để Student biết điểm mạnh/yếu. |
| T6-05 | Là Teacher, tôi muốn sửa điểm đã chấm trước đó, để điều chỉnh khi phát hiện sai sót. |
| T6-06 | Là Teacher, tôi muốn xem điểm tổng và trạng thái tự động cập nhật sau khi chấm xong tất cả câu, để biết bài đã được chấm hoàn chỉnh. |
| T6-07 | Là Teacher, tôi muốn công bố điểm cho toàn bộ lớp khi cần (manual policy), để kiểm soát thời điểm học sinh xem kết quả. |
| T6-08 | Là Teacher, tôi muốn xem báo cáo tổng hợp Assignment (điểm trung bình, min/max, số chưa nộp), để đánh giá kết quả lớp. |
| T6-09 | Là Teacher, tôi muốn xuất bảng điểm ra Excel/CSV, để dùng trong hệ thống quản lý trường hoặc báo cáo. |

### Student

| ID | Story |
|---|---|
| S6-01 | Là Student, tôi muốn xem điểm của mình sau khi được công bố, để biết kết quả học tập. |
| S6-02 | Là Student, tôi muốn xem feedback của Teacher cho từng câu tự luận, để hiểu chỗ sai và cải thiện. |
| S6-03 | Là Student, tôi muốn biết bài của mình đang ở trạng thái nào (chờ chấm / đã có điểm / chưa được công bố), để không bị lo lắng. |
| S6-04 | Là Student, tôi muốn xem đáp án đúng của câu khách quan sau khi điểm được công bố (nếu Teacher cho phép), để học từ sai lầm. |
| S6-05 | Là Student, tôi muốn xem lịch sử điểm tất cả Attempt của mình cho mỗi Assignment, để theo dõi tiến độ cải thiện. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-6-01 | Điểm câu tự luận phải trong khoảng `[0, point]` với `point` là điểm được gán trong Snapshot. |
| BR-6-02 | Attempt chuyển sang `Graded` khi VÀ CHỈ KHI tất cả câu tự luận đã có điểm (không còn câu nào `null`). |
| BR-6-03 | Teacher sửa điểm câu tự luận sau khi đã `Graded` → Attempt tính lại tổng điểm ngay; ghi audit log (MVP-7). |
| BR-6-04 | `grade_publish_policy = manual` → Teacher phải bấm "Công bố điểm" cho Assignment; khi đó tất cả Attempt `Graded` + `AutoGraded` của Assignment đó đều hiển thị với Student. |
| BR-6-05 | Student chỉ xem điểm Attempt của chính mình; không xem được Attempt của Student khác. |
| BR-6-06 | `show_answers_after_grade` (cấu hình ở Assignment MVP-5): nếu `true`, Student xem đáp án đúng câu khách quan sau khi điểm được công bố. Mặc định `false`. |
| BR-6-07 | Export CSV/Excel: chỉ bao gồm điểm đã được phép hiển thị theo `grade_publish_policy`. |
| BR-6-08 | Với Assignment `score_policy = highest`: báo cáo và export dùng điểm cao nhất trong tất cả Attempt đã `Graded`/`AutoGraded` của Student. |

---

## 7. States & Workflows

### Grade publishing flow

```
Assignment tạo với grade_publish_policy

── immediate ──► Student thấy điểm ngay khi Attempt = AutoGraded hoặc Graded

── after_deadline ──► Student thấy điểm sau closes_at (background job kiểm tra)

── manual ──► Teacher bấm "Công bố điểm"
                    │
                    ▼
             grades_published_at được set
                    │
                    ▼
             Student thấy điểm của tất cả Attempt đã Graded/AutoGraded
```

### Manual grading flow

```
Teacher mở danh sách Attempt (filter: NeedManualGrading)
    │
    ▼
Teacher chọn 1 Attempt
    │
    ▼
Thấy: câu khách quan (điểm auto) + câu tự luận (chờ chấm)
    │
    ▼
Teacher nhập điểm + feedback cho từng câu tự luận
    │
    ▼
[Tất cả câu tự luận đã có điểm?]
    ├──[Có]──► Attempt → Graded; tổng điểm được tính
    └──[Chưa]──► Attempt vẫn NeedManualGrading
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `manual_grades` | id, attempt_id, snapshot_question_id, score, feedback, graded_by (teacher_id), graded_at, updated_at |
| `assignment_grade_releases` | id, assignment_id, released_by (teacher_id), released_at — ghi nhận khi Teacher công bố điểm (manual policy) |

> **Tổng điểm Attempt** = `SUM(attempt_answers.auto_score)` + `SUM(manual_grades.score)`. Tính động, không lưu riêng (hoặc cache nếu cần performance).

---

## 9. Permission Matrix

| Action | Student (owner) | Student (khác) | Teacher (class owner) | Admin |
|---|---|---|---|---|
| Xem danh sách Attempt của Assignment | Chỉ của mình | Không | Tất cả | Có |
| Xem chi tiết Attempt (câu trả lời) | Chỉ của mình (nếu đủ điều kiện) | Không | Có | Có |
| Nhập/sửa điểm tự luận | Không | Không | Có | Không |
| Công bố điểm (manual policy) | Không | Không | Có | Không |
| Xem điểm (khi đã công bố) | Có (của mình) | Không | Có | Có |
| Xem feedback | Có (của mình, khi đã công bố) | Không | Có | Có |
| Xem đáp án đúng | Có (nếu `show_answers_after_grade = true` và đã công bố) | Không | Có | Có |
| Xem báo cáo Assignment | Không | Không | Có | Có |
| Export CSV/Excel | Không | Không | Có | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Teacher nhập điểm vượt quá `point` của câu hỏi | Validate: điểm phải ≤ điểm tối đa của câu |
| Teacher nhập điểm âm | Validate: điểm phải ≥ 0 |
| Teacher công bố điểm khi còn Attempt ở `NeedManualGrading` | Cảnh báo: "Còn X bài chưa được chấm xong. Bạn có chắc muốn công bố?" — Teacher confirm mới thực hiện. Attempt chưa Graded sẽ hiển thị "Chưa có điểm" với Student |
| Student xem điểm của Attempt không phải của mình | 403 |
| `grade_publish_policy = after_deadline` nhưng `closes_at` chưa qua | Student thấy "Điểm sẽ được công bố sau khi hết thời gian làm bài" |
| Assignment bị archive trước khi Teacher chấm xong | Attempt vẫn có thể chấm; chỉ ảnh hưởng giao bài mới |
| `score_policy = highest` và Student làm 2 lần (lần 1 = 7, lần 2 = 5) | Báo cáo và export dùng 7; Student xem cả 2 Attempt |

---

## 11. Acceptance Criteria

- [ ] Teacher mở Attempt `NeedManualGrading` → thấy câu hỏi, câu trả lời auto-graded và câu tự luận riêng biệt.
- [ ] Teacher nhập điểm + feedback cho câu tự luận → lưu thành công.
- [ ] Sau khi Teacher chấm xong tất cả câu tự luận → Attempt state = `Graded`; tổng điểm hiển thị đúng.
- [ ] `grade_publish_policy = immediate`: Student Submit → thấy điểm ngay.
- [ ] `grade_publish_policy = manual`: Student không thấy điểm cho đến khi Teacher bấm "Công bố".
- [ ] Student không thể xem điểm của Student khác (403).
- [ ] Báo cáo Assignment hiển thị đúng: số đã nộp, chưa nộp, điểm trung bình, min/max.
- [ ] Export CSV: tải về file với đúng danh sách Student + điểm.
- [ ] Teacher sửa điểm đã chấm → tổng điểm cập nhật ngay.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Teacher quên chấm → Student không bao giờ có điểm | Trung bình | Notification nhắc nhở (MVP-7): "Bạn có X bài chưa chấm" |
| Teacher công bố điểm khi còn bài chưa chấm | Trung bình | Warning dialog rõ ràng trước khi publish |
| Sửa điểm dẫn đến tranh chấp | Thấp | Audit log ghi lại mọi lần sửa điểm (MVP-7) |
| Export file lớn → timeout | Thấp | Giới hạn mềm (vd: max 500 rows); async export nếu cần |

---

## 13. Verification Plan

**Scenario 1 — Manual grading flow:**
1. Student Submit Assignment có câu tự luận → Attempt = `NeedManualGrading`.
2. Teacher mở Attempt → thấy câu tự luận của Student.
3. Teacher nhập điểm 4/5 và feedback "Cần phân tích sâu hơn".
4. Attempt → `Graded`; tổng điểm = auto_score + 4.

**Scenario 2 — Grade publishing:**
1. Assignment `policy = manual` → Student Submit → không thấy điểm.
2. Teacher bấm "Công bố điểm" → Student reload → thấy điểm và feedback.

**Scenario 3 — Reports:**
1. 5 Student nộp bài, 2 chưa nộp → báo cáo hiển thị 5 submitted, 2 pending, điểm trung bình đúng.
2. Export CSV → file có 7 rows (5 có điểm, 2 "Chưa nộp").

**Scenario 4 — Score policy:**
1. Assignment `max_attempts = 2`, `score_policy = highest`.
2. Student làm lần 1 = 6đ, lần 2 = 8đ.
3. Báo cáo hiển thị 8đ cho Student đó.

---

## 14. Hook sang MVP-7

MVP-7 (Admin & Moderation) sẽ:
- Thêm **audit log** cho hành động sửa điểm (BR-6-03) — ai sửa, bao giờ, từ bao nhiêu sang bao nhiêu
- Thêm **notification** nhắc Teacher có bài chưa chấm (event: `AssignmentPendingGrading`)
- Thêm **notification** cho Student khi điểm được công bố (event: `GradePublished`)
- Thêm **report flow** để Student/Teacher report vấn đề về điểm hoặc nội dung câu hỏi
