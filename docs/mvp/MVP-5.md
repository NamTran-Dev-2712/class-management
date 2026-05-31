# MVP-5 — Assignment & Online Testing

## 1. Context & Mục tiêu

MVP-5 là **trung tâm của toàn bộ hệ thống** — nơi mọi thứ từ MVP-1 đến MVP-4 hội tụ và tạo ra giá trị thực sự cho người dùng. Teacher giao Exam cho Class dưới dạng Assignment; Student thấy bài, bắt đầu làm, và nộp. Hệ thống auto-grade câu khách quan và chuẩn bị cho việc chấm thủ công ở MVP-6.

**Điểm sống còn của MVP-5:**
1. **Snapshot bất biến** — nội dung đề thi không thể bị thay đổi sau khi giao.
2. **Auto-save** — học sinh không mất bài do reload hay mất mạng.
3. **Auto-submit** — hệ thống tự nộp bài khi hết giờ, không có kẽ hở.
4. **State machine rõ ràng** — Assignment và Attempt đều có trạng thái tường minh.

**Kết quả cụ thể:** Teacher giao bài cho lớp; học sinh làm bài online trong thời hạn; câu trắc nghiệm được chấm tự động; Teacher và Student thấy kết quả (pending manual grading cho câu tự luận).

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-1 hoàn thành**: auth, role Student/Teacher.
- **MVP-2 hoàn thành**: Class và ClassMembership đã có.
- **MVP-3 hoàn thành**: Question Bank với 5 loại câu hỏi.
- **MVP-4 hoàn thành**: Exam Builder với versioning.

---

## 3. Trong scope

### Assignment Management
- Teacher tạo Assignment từ Exam của mình, giao cho 1 hoặc nhiều Class
- Cấu hình Assignment: tên, mô tả, thời gian bắt đầu, thời hạn nộp, thời gian làm bài (time limit)
- Cấu hình `max_attempts` (1 hoặc unlimited)
- Cấu hình `score_policy` (highest / latest) khi `max_attempts > 1`
- Cấu hình `allow_late_submission` (có cho nộp trễ không)
- Cấu hình `grade_publish_policy` (immediate / after_deadline / manual) — logic hiển thị điểm ở MVP-6
- Cấu hình `shuffle_questions` (random thứ tự câu hỏi)
- Cấu hình `shuffle_options` (random thứ tự lựa chọn cho SingleChoice/MultipleChoice)
- Assignment states: `Draft` → `Scheduled` / `Open` → `Closed` → `Archived`
- Teacher publish Assignment (Draft → Scheduled nếu chưa đến giờ, hoặc Open nếu đã đến giờ)
- Teacher đóng sớm Assignment (`Open` → `Closed`)
- Teacher archive Assignment
- **Snapshot**: khi publish Assignment, copy toàn bộ nội dung Exam + Question vào snapshot tables

### Online Testing (Student side)
- Student xem danh sách Assignment của các Class mình tham gia
- Student xem chi tiết Assignment: thời hạn, time limit, số câu, điểm tối đa, số lần làm còn lại
- Student bắt đầu Attempt (Start) khi Assignment đang `Open`
- **Auto-save** đáp án Student đang làm theo interval (mỗi 30 giây hoặc khi đổi câu)
- Student nộp bài thủ công (Submit)
- **Auto-submit** khi hết `time_limit` tính từ lúc Start Attempt
- Attempt states: `InProgress` → `Submitted` → `AutoGraded` / `NeedManualGrading` / `Graded`

### Auto-grading
- Sau khi Attempt được Submit, auto-grade ngay các câu `SingleChoice`, `MultipleChoice`, `TrueFalse`
- `MultipleChoice`: chỉ tính điểm khi Student chọn **đúng tất cả** đáp án correct (không partial score ở MVP-5)
- Câu `ShortWriting` / `LongWriting`: tạo record chờ chấm thủ công (MVP-6)
- Nếu Attempt chỉ có câu khách quan: state → `AutoGraded`
- Nếu có câu tự luận: state → `NeedManualGrading`

### Anti-cheating cơ bản
- `shuffle_questions` và `shuffle_options` theo cấu hình Assignment
- Ghi nhận `started_at` và `submitted_at` của mỗi Attempt
- Ghi nhận `ip_address` và `user_agent` khi tạo Attempt (audit only)
- Không cho phép Start Attempt mới khi đã đạt `max_attempts`

---

## 4. Ngoài scope (Out of Scope)

- Camera proctoring, screen recording → ROADMAP-FUTURE
- AI phát hiện chuyển tab / copy-paste → ROADMAP-FUTURE
- Partial score cho MultipleChoice (chọn đúng 1/3 đáp án được điểm một phần) → phase sau
- Random question pool (chọn ngẫu nhiên N câu từ pool lớn hơn) → phase sau
- Assignment giao cho Student cụ thể (không phải cả Class) → phase sau
- Cấu hình IP whitelist (chỉ làm bài trong mạng trường) → ROADMAP-FUTURE
- Cho phép Teacher mở lại Attempt cụ thể cho Student → có thể thêm ở MVP-6/7

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T5-01 | Là Teacher, tôi muốn tạo Assignment từ Exam của mình và giao cho lớp với thời gian cụ thể, để học sinh biết khi nào cần làm bài. |
| T5-02 | Là Teacher, tôi muốn cấu hình thời gian làm bài (time limit), để kiểm soát thời gian thi. |
| T5-03 | Là Teacher, tôi muốn bật shuffle câu hỏi và đáp án, để giảm thiểu sao chép giữa học sinh. |
| T5-04 | Là Teacher, tôi muốn xem danh sách tất cả Attempt của Assignment, để theo dõi tiến độ nộp bài. |
| T5-05 | Là Teacher, tôi muốn đóng sớm Assignment khi cần, để kết thúc bài kiểm tra trước thời hạn ban đầu. |
| T5-06 | Là Teacher, tôi muốn xem Assignment đang Draft trước khi publish, để kiểm tra cấu hình. |
| T5-07 | Là Teacher, tôi muốn giao cùng 1 Exam cho nhiều Class khác nhau (tạo Assignment riêng cho mỗi lớp), để linh hoạt về thời gian. |

### Student

| ID | Story |
|---|---|
| S5-01 | Là Student, tôi muốn xem danh sách Assignment từ tất cả lớp mình tham gia, để biết bài nào cần làm. |
| S5-02 | Là Student, tôi muốn xem thông tin chi tiết Assignment (thời hạn, số câu, time limit, số lần làm còn lại) trước khi bắt đầu, để chuẩn bị tốt. |
| S5-03 | Là Student, tôi muốn bắt đầu làm bài và thấy câu hỏi hiển thị đúng, để hoàn thành Assignment. |
| S5-04 | Là Student, tôi muốn bài làm được auto-save trong khi tôi đang làm, để không mất bài nếu sự cố xảy ra. |
| S5-05 | Là Student, tôi muốn biết còn bao nhiêu thời gian làm bài (countdown timer), để quản lý thời gian. |
| S5-06 | Là Student, tôi muốn nộp bài bất kỳ lúc nào trước khi hết giờ, để chủ động kết thúc sớm. |
| S5-07 | Là Student, tôi muốn bài được tự động nộp khi hết giờ, để không bị mất bài vì quên nộp. |
| S5-08 | Là Student, tôi muốn xem lịch sử các Attempt của mình, để biết đã làm bao nhiêu lần. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-5-01 | Assignment phải gắn với đúng 1 Class và đúng 1 Exam. Exam phải có ít nhất 1 câu hỏi. |
| BR-5-02 | **Snapshot bất biến**: ngay khi Teacher publish Assignment, hệ thống copy toàn bộ nội dung `exam_questions` + `question` + `question_options` vào `assignment_snapshots`. Sau đó Attempt LUÔN đọc từ snapshot. |
| BR-5-03 | Thời gian bắt đầu (`opens_at`) phải ≤ thời hạn nộp (`closes_at`). |
| BR-5-04 | `time_limit` (số phút làm bài) phải ≤ khoảng thời gian `opens_at` → `closes_at`. |
| BR-5-05 | Student chỉ Start Attempt khi Assignment đang `Open` VÀ chưa đạt `max_attempts`. |
| BR-5-06 | Student chỉ có **1 Attempt `InProgress`** tại 1 thời điểm. Không thể Start mới khi đang có Attempt `InProgress`. |
| BR-5-07 | Auto-submit xảy ra khi `time_limit` kể từ `started_at` của Attempt đã trôi qua. Lưu `submitted_at` = thời điểm auto-submit, đánh dấu là `auto_submitted = true`. |
| BR-5-08 | Khi Assignment `Closed`: tất cả Attempt đang `InProgress` bị auto-submit ngay lập tức. |
| BR-5-09 | Student đã bị kick khỏi Class hoặc tự rời Class không thể Start Attempt mới; Attempt đang `InProgress` vẫn có thể Submit trong `time_limit`. |
| BR-5-10 | `shuffle_questions` và `shuffle_options` áp dụng per-Attempt: mỗi Attempt có thứ tự riêng, được lưu lúc Start. |
| BR-5-11 | Auto-save không tính là Submit. Student phải Submit (thủ công hoặc auto) để kết thúc Attempt. |
| BR-5-12 | `MultipleChoice` auto-grade: đúng tất cả đáp án correct VÀ không chọn thêm đáp án sai → đủ điểm. Nếu sai bất kỳ (thiếu hoặc thừa) → 0 điểm (không partial score). |

---

## 7. States & Workflows

### Assignment State Machine

```
Draft
  │
  ├──[publish, opens_at > now]──────► Scheduled
  │                                      │
  ├──[publish, opens_at ≤ now]──────► Open ──[closes_at qua hoặc Teacher đóng sớm]──► Closed
  │                                                                                         │
  └──────────────────────────────────────────────────────[Teacher archive]──────────► Archived
```

### Attempt State Machine

```
[Student Start]
    │
    ▼
InProgress ──[Student Submit hoặc time_limit hết hoặc Assignment Closed]──► Submitted
                                                                                │
                        ┌───────────────────────────────────────────────────────┤
                        │                                                        │
                [Chỉ câu khách quan]                               [Có câu tự luận]
                        │                                                        │
                        ▼                                                        ▼
                  AutoGraded                                        NeedManualGrading
                                                                          │
                                                               [MVP-6: Teacher chấm xong]
                                                                          │
                                                                          ▼
                                                                       Graded
```

### Auto-save flow

```
Student đang làm bài
    │
    ├──[mỗi 30 giây]──► API auto-save đáp án hiện tại (PATCH /attempts/{id}/answers)
    │
    ├──[Student chuyển câu]──► Optional: save ngay câu vừa trả lời
    │
    └──[Student Submit]──► Final save + submit
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `assignments` | id, exam_id, exam_version, class_id, teacher_id, title, description, opens_at, closes_at, time_limit_minutes, max_attempts, score_policy, allow_late, grade_publish_policy, shuffle_questions, shuffle_options, status, created_at |
| `assignment_snapshots` | id, assignment_id — container cho snapshot |
| `snapshot_questions` | id, snapshot_id, original_question_id, type, content, point, display_order — bản copy câu hỏi tại thời điểm publish |
| `snapshot_options` | id, snapshot_question_id, content, is_correct, display_order — bản copy options |
| `attempts` | id, assignment_id, student_id, status, started_at, submitted_at, auto_submitted, ip_address, user_agent, question_order (JSON array), created_at |
| `attempt_answers` | id, attempt_id, snapshot_question_id, selected_option_ids (array), text_answer, auto_score, saved_at |

> **Ghi chú thiết kế:** `question_order` trong `attempts` là JSON array lưu thứ tự câu hỏi của Attempt này (kết quả shuffle). Đảm bảo mỗi Attempt có thứ tự nhất quán xuyên suốt.

---

## 9. Permission Matrix

| Action | Student (member) | Student (non-member) | Teacher (owner) | Teacher (khác) | Admin |
|---|---|---|---|---|---|
| Xem danh sách Assignment của lớp | Có | Không | Có | Không | Có |
| Xem chi tiết Assignment | Có | Không | Có | Không | Có |
| Start Attempt | Có (nếu đủ điều kiện) | Không | Không | Không | Không |
| Auto-save đáp án | Có (InProgress) | Không | Không | Không | Không |
| Submit Attempt | Có (InProgress) | Không | Không | Không | Không |
| Tạo/sửa/publish Assignment | Không | Không | Có | Không | Không |
| Đóng sớm Assignment | Không | Không | Có | Không | Không |
| Xem danh sách Attempt | Không | Không | Có | Không | Có |
| Xem Attempt của mình | Có | Không | Có | Không | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Student Start Assignment sau `closes_at` | Từ chối: "Bài kiểm tra đã kết thúc" |
| Student Start khi đã có Attempt `InProgress` | Chuyển hướng về Attempt đang `InProgress` thay vì tạo mới |
| Student Start khi đã đạt `max_attempts` | Từ chối: "Bạn đã làm hết số lần cho phép" |
| Mạng mất giữa lúc làm bài | Auto-save đã lưu đáp án gần nhất; Student reload → load lại draft answers từ `attempt_answers` |
| Student nộp bài khi `time_limit` đã hết ở phía server | Server kiểm tra thời gian; nếu quá `time_limit` → đánh dấu `auto_submitted = true`, không nhận submit mới |
| Teacher sửa Exam sau khi Assignment đã published | Không ảnh hưởng Snapshot. Teacher thấy cảnh báo: "Assignment X đang dùng Exam này" |
| Teacher publish Assignment khi Exam không có câu hỏi | Validate: báo lỗi, không cho publish |
| Assignment `opens_at` = `closes_at` | Validate: `closes_at` phải sau `opens_at` ít nhất `time_limit` phút |
| Student rời lớp khi có Attempt `InProgress` | Attempt vẫn tồn tại; Student vẫn có thể Submit; sau khi rời, không tạo Attempt mới được |
| Duplicate Attempt submit (race condition) | Idempotency check: server check `status != InProgress` trước khi accept Submit |

---

## 11. Acceptance Criteria

- [ ] Teacher tạo Assignment từ Exam, cấu hình thời gian và time limit → publish thành công.
- [ ] Snapshot được tạo khi publish: sửa Exam gốc sau đó không thay đổi nội dung Assignment.
- [ ] Student thấy Assignment trong danh sách khi Class được duyệt và Assignment đang `Open`.
- [ ] Student Start → thấy câu hỏi đúng với thứ tự đã shuffle (nếu bật).
- [ ] Auto-save: reload trang → đáp án cũ vẫn còn.
- [ ] Countdown timer hiển thị đúng; khi hết giờ → auto-submit.
- [ ] Sau Submit: câu `SingleChoice`, `TrueFalse` được chấm điểm ngay.
- [ ] `MultipleChoice`: chọn đúng tất cả → có điểm; thiếu hoặc thừa → 0 điểm.
- [ ] Câu `ShortWriting`/`LongWriting`: Attempt state → `NeedManualGrading`.
- [ ] Student không thể Start khi Assignment `Closed` hoặc đã đạt `max_attempts`.
- [ ] `shuffle_questions`: 2 Student khác nhau thấy thứ tự câu hỏi khác nhau.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Auto-submit race condition (client submit và server auto-submit cùng lúc) | Cao | Idempotency: check `status = InProgress` trước khi process submit; database transaction |
| Auto-save tạo quá nhiều request → overload server | Trung bình | Rate limit auto-save (max 1 request/30s per attempt); debounce ở client |
| Snapshot tables phình to theo thời gian | Trung bình | Archive Attempt + Snapshot cũ sau N tháng (cấu hình); index tốt |
| Clock skew giữa client và server → time limit sai | Trung bình | Luôn dùng server time để tính time limit; client chỉ hiển thị countdown |
| Học sinh share câu hỏi với nhau (shuffle không đủ) | Thấp | Shuffle là biện pháp cơ bản; proctoring nâng cao ở ROADMAP-FUTURE |

---

## 13. Verification Plan

**Scenario 1 — Assignment lifecycle:**
1. Teacher tạo Assignment (Draft) → xem preview → publish → status = Scheduled (nếu chưa đến giờ).
2. Đến giờ `opens_at` → status tự chuyển sang `Open` (cần background job hoặc check khi load).
3. Teacher đóng sớm → status = `Closed`.

**Scenario 2 — Student làm bài:**
1. Student Start → countdown timer chạy.
2. Trả lời một số câu → reload trang → đáp án vẫn còn (auto-save).
3. Submit thủ công → xem kết quả auto-grade ngay.

**Scenario 3 — Auto-submit:**
1. Student Start nhưng không submit → đợi hết `time_limit` → hệ thống auto-submit.
2. Kiểm tra Attempt: `auto_submitted = true`, `status = Submitted`, điểm câu khách quan đã tính.

**Scenario 4 — Snapshot bất biến:**
1. Teacher publish Assignment → ghi nhận nội dung câu hỏi.
2. Teacher sửa Exam gốc (đổi đáp án) → Student load Assignment → thấy nội dung cũ (snapshot).

**Scenario 5 — Max attempts:**
1. `max_attempts = 1` → Student Submit → không thể Start lại.
2. `max_attempts = 2` → Student Submit lần 1 → Start lần 2 thành công → Submit lần 2 → không thể Start lần 3.

---

## 14. Hook sang MVP-6

MVP-6 (Manual Grading & Reports) sẽ:
- Xử lý các Attempt có state `NeedManualGrading`
- Teacher mở Attempt cụ thể → thấy câu tự luận của Student → nhập điểm + feedback
- Sau khi chấm tất cả câu tự luận → Attempt state → `Graded`
- Áp dụng `grade_publish_policy` (được cấu hình ở MVP-5) để quyết định khi nào Student xem được điểm
- Báo cáo tổng hợp điểm của cả lớp dựa trên `attempt_answers` + `auto_score` + manual score
