# MVP-4 — Exam Builder

## 1. Context & Mục tiêu

MVP-4 xây dựng **Exam Builder** — cho phép Teacher tổng hợp các Question từ ngân hàng thành một Exam (đề mẫu). Exam chưa được giao cho học sinh; đây là template tái sử dụng được nhiều lần qua nhiều Assignment khác nhau.

Điểm cốt lõi của MVP-4 là **versioning**: mỗi khi Exam được publish thành Assignment (MVP-5), hệ thống snapshot nội dung Exam tại thời điểm đó. Teacher sửa Exam sau không ảnh hưởng đến các Assignment đã được tạo.

**Kết quả cụ thể:** Teacher có thể tạo đề thi, sắp xếp câu hỏi, gán điểm, xem tổng điểm, và chuẩn bị Exam sẵn sàng để giao ở MVP-5.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-1 hoàn thành**: auth, role Teacher.
- **MVP-3 hoàn thành**: Question Bank đã có câu hỏi để đưa vào Exam.

---

## 3. Trong scope

- Teacher CRUD Exam của mình
- Thêm Question từ bank của mình vào Exam (chọn thủ công)
- Xem và thêm Question Public của Teacher khác vào Exam
- Sắp xếp thứ tự câu hỏi trong Exam (drag-and-drop hoặc up/down buttons)
- Gán điểm thực tế cho từng Question trong Exam (override suggested_point)
- Xem tổng điểm Exam (tổng điểm tất cả câu)
- Visibility: `Private` (chỉ mình thấy) / `Public` (Teacher khác có thể xem và dùng làm tham khảo)
- Teacher xem danh sách Exam của mình (phân trang, lọc theo Subject)
- Teacher xem Exam Public của Teacher khác (read-only)
- Teacher duplicate Exam Public về bank của mình
- Preview Exam như Student sẽ thấy
- **Exam versioning**: mỗi khi Exam được dùng để tạo Assignment, lưu `current_version` vào Assignment; Exam gốc có thể tiếp tục được sửa

---

## 4. Ngoài scope (Out of Scope)

- AI gợi ý câu hỏi theo topic/difficulty → ROADMAP-FUTURE
- Random question pool (chọn ngẫu nhiên N câu từ pool) → phase sau MVP-4 (có thể thêm vào MVP-5 hoặc sau)
- Import Exam từ file Word/PDF → ROADMAP-FUTURE
- Rubric chấm điểm chi tiết cho câu tự luận → MVP-6 và sau đó
- Exam có phần (section/part) riêng biệt → phase sau
- Thời gian giới hạn đặt ở Exam (time limit đặt ở Assignment khi giao, không phải ở Exam template)

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T4-01 | Là Teacher, tôi muốn tạo Exam mới với tên, mô tả, và Subject, để có template đề kiểm tra. |
| T4-02 | Là Teacher, tôi muốn thêm câu hỏi từ bank của mình vào Exam, để ghép đề theo ý muốn. |
| T4-03 | Là Teacher, tôi muốn tìm kiếm câu hỏi khi đang soạn Exam (lọc theo Subject, type, difficulty, tag), để tìm câu hỏi phù hợp nhanh. |
| T4-04 | Là Teacher, tôi muốn thêm câu hỏi Public của Teacher khác vào Exam của mình, để tái sử dụng nội dung tốt. |
| T4-05 | Là Teacher, tôi muốn sắp xếp thứ tự câu hỏi trong Exam, để kiểm soát flow bài thi. |
| T4-06 | Là Teacher, tôi muốn gán điểm cụ thể cho từng câu hỏi trong Exam, để điều chỉnh trọng số. |
| T4-07 | Là Teacher, tôi muốn xem tổng điểm Exam ngay khi soạn, để đảm bảo đúng thang điểm. |
| T4-08 | Là Teacher, tôi muốn preview Exam như Student sẽ thấy, để kiểm tra trước khi giao. |
| T4-09 | Là Teacher, tôi muốn set Exam là Public, để chia sẻ với Teacher khác. |
| T4-10 | Là Teacher, tôi muốn duplicate Exam Public về bank của mình, để tuỳ chỉnh theo nhu cầu. |
| T4-11 | Là Teacher, tôi muốn sửa Exam đã tạo, để cập nhật nội dung khi cần. |

### Student

*Student không tương tác trực tiếp với Exam. Student chỉ thấy nội dung đề qua Attempt (MVP-5).*

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-4-01 | Exam phải gắn với đúng 1 Subject. |
| BR-4-02 | Exam phải có ít nhất 1 câu hỏi mới có thể publish thành Assignment. |
| BR-4-03 | Điểm của từng câu hỏi trong Exam phải > 0. |
| BR-4-04 | Teacher chỉ sửa/xoá Exam do mình tạo. |
| BR-4-05 | Không thể xoá Exam đang được dùng trong ít nhất 1 Assignment. Báo lỗi và liệt kê Assignment đang dùng. |
| BR-4-06 | Sửa nội dung Exam sau khi đã có Assignment KHÔNG ảnh hưởng đến Attempt cũ, vì MVP-5 sẽ snapshot nội dung Exam khi tạo Assignment. |
| BR-4-07 | Khi Teacher thêm Question của Teacher khác (Public) vào Exam, lưu tham chiếu đến `question_id` gốc. Khi snapshot ở MVP-5, copy toàn bộ content câu hỏi vào snapshot, không tham chiếu nguyên câu hỏi gốc nữa. |
| BR-4-08 | Duplicate Exam Public: bản copy là Exam mới độc lập, không liên kết ngược lại original. |
| BR-4-09 | Giới hạn số Exam Teacher có thể tạo theo config (chuẩn bị cho premium MVP-8). Mặc định unlimited ở MVP-4. |

---

## 7. States & Workflows

### Exam Visibility

```
Private (default) ──[Teacher set public]──► Public
Public ──[Teacher set private]──► Private
```

### Exam trong lifecycle Assignment (chuẩn bị cho MVP-5)

```
Exam (template) ──[Teacher giao bài ở MVP-5]──►
    Assignment được tạo
        └── Snapshot được tạo (copy nội dung Exam + tất cả Question tại thời điểm này)
            └── Attempt của Student tham chiếu snapshot, không phải Exam gốc

Exam gốc vẫn có thể tiếp tục sửa → không ảnh hưởng Attempt cũ
Exam gốc có thể được giao thành Assignment mới (snapshot mới được tạo)
```

### Exam deletion check

```
Teacher muốn xoá Exam
    │
    ├──[Exam đang dùng trong Assignment]──► Từ chối, liệt kê Assignment
    │
    └──[Exam không dùng ở đâu]──► Xoá thành công
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `exams` | id, teacher_id, subject_id, title, description, visibility, version (integer tăng dần mỗi lần save), is_deleted, created_at, updated_at |
| `exam_questions` | id, exam_id, question_id, display_order, point — quan hệ Exam ↔ Question với điểm và thứ tự |

> **Ghi chú về `version`:** Mỗi lần Teacher save Exam (thêm/bỏ câu hỏi, sửa điểm, đổi thứ tự), `version` tăng 1. Khi tạo Assignment (MVP-5), lưu `exam_version` vào Assignment để biết bản nào được dùng khi snapshot.

---

## 9. Permission Matrix

| Action | Student | Teacher (owner) | Teacher (khác) | Admin |
|---|---|---|---|---|
| Tạo Exam | Không | Có | Không | Không |
| Sửa Exam | Không | Có (của mình) | Không | Không |
| Xoá Exam | Không | Có (của mình, nếu không dùng) | Không | Không |
| Xem Exam của mình | Không | Có | Không | Không |
| Xem Exam Public | Không | Có (read-only) | Có (read-only) | Có |
| Duplicate Exam Public | Không | Có | Có | Không |
| Set visibility Public/Private | Không | Có (của mình) | Không | Không |
| Preview Exam | Không | Có | Không (chỉ xem Public) | Có |
| Xem tất cả Exam | Không | Không | Không | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Thêm Question đã có trong Exam (trùng lặp) | Ngăn chặn: báo "Câu hỏi này đã có trong đề" |
| Tạo Assignment từ Exam không có câu hỏi | Validate: Exam phải có ít nhất 1 câu hỏi |
| Teacher xoá Exam đang trong Assignment | Từ chối, trả lỗi với danh sách Assignment |
| Teacher sửa điểm câu hỏi trong Exam sau khi Assignment đã tạo | Cho phép sửa Exam; Snapshot của Assignment không thay đổi |
| Question trong Exam bị Teacher gốc xoá (Question Public) | Exam vẫn tham chiếu question_id; khi tạo Assignment mới cần snapshot, nếu Question không còn tồn tại → cảnh báo Teacher để gỡ/thay thế trước khi giao. Attempt cũ không bị ảnh hưởng (đã snapshot). |
| Tổng điểm Exam = 0 (tất cả câu có điểm = 0) | Validate: ít nhất 1 câu có điểm > 0 hoặc tổng điểm > 0 |
| Duplicate Exam Public có nhiều câu hỏi Private của Teacher gốc | Bản copy vẫn tham chiếu question_id gốc; nếu question_id đó là Private của Teacher gốc, Teacher copy sẽ thấy "câu hỏi không còn truy cập được" và cần thay thế |

---

## 11. Acceptance Criteria

- [ ] Teacher tạo Exam với tên, Subject, thêm ít nhất 1 câu hỏi → save thành công.
- [ ] Teacher thêm câu hỏi từ bank của mình và từ Question Public vào cùng 1 Exam.
- [ ] Thứ tự câu hỏi được lưu và hiển thị đúng.
- [ ] Điểm từng câu được gán riêng; tổng điểm hiển thị đúng.
- [ ] Teacher xoá Exam đang có Assignment → nhận lỗi rõ ràng.
- [ ] Teacher xoá Exam không có Assignment → xoá thành công.
- [ ] Exam duplicate hoạt động đúng: bản copy độc lập, sửa 1 bản không ảnh hưởng bản kia.
- [ ] Preview Exam hiển thị đúng như Student sẽ thấy (câu hỏi đúng thứ tự, không hiện đáp án đúng).
- [ ] Exam version tăng mỗi khi Teacher save thay đổi.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Teacher sửa Exam sau khi Assignment đã chạy và làm sai dữ liệu | Cao | Snapshot ở MVP-5 đảm bảo isolation; Exam gốc và Snapshot là 2 bản độc lập |
| Question Public bị Teacher gốc xoá trong khi Exam khác đang dùng | Trung bình | Soft delete Question; cảnh báo Teacher khi mở Exam có câu hỏi "unavailable" |
| Exam có quá nhiều câu hỏi → performance khi load | Thấp | Giới hạn mềm (vd: cảnh báo khi > 100 câu); pagination trong exam builder |

---

## 13. Verification Plan

**Scenario 1 — Tạo và soạn Exam:**
1. Teacher tạo Exam "Kiểm tra 15 phút Chương 1" → save thành công.
2. Thêm 5 câu từ Question Bank → xuất hiện đúng thứ tự.
3. Kéo câu hỏi đổi thứ tự → thứ tự lưu đúng.
4. Gán điểm: câu 1 = 2đ, câu 2 = 3đ → tổng = 5đ hiển thị đúng.

**Scenario 2 — Versioning:**
1. Tạo Assignment từ Exam (thực hiện ở MVP-5).
2. Sửa Exam: thêm câu hỏi, đổi điểm.
3. Kiểm tra Assignment cũ → nội dung không thay đổi (dùng snapshot).

**Scenario 3 — Public Exam:**
1. Teacher A set Exam Public → Teacher B thấy ở danh sách Exam Public.
2. Teacher B duplicate về bank → bản copy xuất hiện trong bank của Teacher B.

**Scenario 4 — Xoá có ràng buộc:**
1. Exam có Assignment (MVP-5) → xoá → nhận lỗi.
2. Exam không có Assignment → xoá thành công.

---

## 14. Hook sang MVP-5

MVP-5 (Assignment & Online Testing) sẽ:
- Cho Teacher chọn Exam từ bank của mình để tạo Assignment
- Tạo Snapshot ngay khi Assignment được publish: copy toàn bộ `exam_questions` + nội dung từng `question` + `question_options` vào bảng snapshot riêng
- Attempt của Student tham chiếu snapshot (không tham chiếu `exams` hoặc `questions` gốc)
- Assignment lưu `exam_id` + `exam_version` để biết Snapshot này được tạo từ version nào của Exam
