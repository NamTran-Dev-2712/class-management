# MVP-3 — Question Bank

## 1. Context & Mục tiêu

MVP-3 xây dựng **ngân hàng câu hỏi** của hệ thống — nơi Teacher tạo, lưu trữ, và tái sử dụng các câu hỏi để ghép thành đề thi ở MVP-4. Question là đơn vị nhỏ nhất của nội dung học tập, và chất lượng ngân hàng câu hỏi quyết định chất lượng đề thi.

**Kết quả cụ thể:** Teacher có thể tạo câu hỏi với 5 loại khác nhau, gán tag và difficulty, lọc/tìm kiếm câu hỏi của mình, và chia sẻ câu hỏi công khai để Teacher khác tham khảo.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-1 hoàn thành**: auth, role Teacher, Subject đã có.
- Question phải gắn với 1 Subject (lấy từ danh sách Subject của Admin).

---

## 3. Trong scope

- Teacher CRUD Question của mình
- 5 loại Question: `SingleChoice`, `MultipleChoice`, `TrueFalse`, `ShortWriting`, `LongWriting`
- Gán điểm gợi ý (suggested point) cho mỗi Question
- Phân loại độ khó: `Easy`, `Medium`, `Hard`
- Gắn Question với Subject
- Tag tự do (free-form tags): Teacher thêm/bỏ tag khi tạo hoặc sửa câu hỏi
- Visibility: `Private` (chỉ mình thấy) / `Public` (mọi Teacher thấy và dùng tham khảo)
- Teacher xem danh sách Question của mình (phân trang)
- Teacher tìm kiếm/lọc Question theo: Subject, loại câu hỏi, difficulty, tag, visibility, keyword
- Teacher xem danh sách Question public của Teacher khác (read-only, không sửa)
- Teacher duplicate (copy) Question public về bank của mình để tùy chỉnh
- Teacher preview Question trước khi thêm vào Exam

---

## 4. Ngoài scope (Out of Scope)

- `FillInTheBlank` (điền khuyết) → dời sang phase sau để tránh phức tạp validation đáp án
- Import câu hỏi từ Excel/CSV → ROADMAP-FUTURE
- AI gợi ý câu hỏi liên quan → ROADMAP-FUTURE
- Shared question pools theo Subject (Teacher cùng Subject share pool) → phase sau
- Versioning nội dung câu hỏi (Question history) — Versioning được handle ở Exam Snapshot (MVP-4/5), không cần track history từng câu hỏi ở MVP-3
- Review / rating câu hỏi public của Teacher khác → ROADMAP-FUTURE

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T3-01 | Là Teacher, tôi muốn tạo câu hỏi trắc nghiệm 1 đáp án (SingleChoice), để dùng trong đề kiểm tra. |
| T3-02 | Là Teacher, tôi muốn tạo câu hỏi trắc nghiệm nhiều đáp án (MultipleChoice), để đánh giá hiểu biết toàn diện. |
| T3-03 | Là Teacher, tôi muốn tạo câu hỏi Đúng/Sai (TrueFalse), để kiểm tra nhanh kiến thức. |
| T3-04 | Là Teacher, tôi muốn tạo câu hỏi tự luận ngắn (ShortWriting), để yêu cầu học sinh trả lời bằng văn bản. |
| T3-05 | Là Teacher, tôi muốn tạo câu hỏi tự luận dài (LongWriting), để đánh giá năng lực phân tích. |
| T3-06 | Là Teacher, tôi muốn gắn tag và chọn độ khó cho câu hỏi, để dễ tìm kiếm và phân loại sau này. |
| T3-07 | Là Teacher, tôi muốn đặt câu hỏi là Public, để chia sẻ với các Teacher khác. |
| T3-08 | Là Teacher, tôi muốn tìm kiếm và lọc câu hỏi trong bank của mình, để tìm nhanh câu hỏi cần dùng. |
| T3-09 | Là Teacher, tôi muốn xem câu hỏi public của Teacher khác, để tham khảo ý tưởng. |
| T3-10 | Là Teacher, tôi muốn duplicate câu hỏi public về bank của mình, để tùy chỉnh cho phù hợp. |
| T3-11 | Là Teacher, tôi muốn sửa câu hỏi đã tạo, để cập nhật nội dung khi phát hiện sai sót. |
| T3-12 | Là Teacher, tôi muốn xoá câu hỏi không còn dùng, để giữ bank gọn gàng. |
| T3-13 | Là Teacher, tôi muốn preview câu hỏi như Student sẽ thấy, để kiểm tra hiển thị trước khi dùng. |

### Student

*Student không tương tác trực tiếp với Question Bank. Student chỉ thấy câu hỏi trong ngữ cảnh làm Attempt (MVP-5).*

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-3-01 | Question phải gắn với đúng 1 Subject (active). |
| BR-3-02 | `SingleChoice` phải có ít nhất 2 lựa chọn, đúng 1 đáp án được đánh dấu là correct. |
| BR-3-03 | `MultipleChoice` phải có ít nhất 2 lựa chọn, ít nhất 1 đáp án được đánh dấu là correct. |
| BR-3-04 | `TrueFalse` có đúng 2 lựa chọn cố định: True / False; Teacher chọn 1 đáp án đúng. |
| BR-3-05 | `ShortWriting` và `LongWriting` không có đáp án cố định trong DB — Teacher chấm thủ công (MVP-6). |
| BR-3-06 | Suggested point của Question mang tính gợi ý; điểm thực tế được set khi thêm vào Exam (MVP-4). |
| BR-3-07 | Teacher chỉ sửa/xoá Question do mình tạo. |
| BR-3-08 | Không thể xoá Question đang được dùng trong ít nhất 1 Exam. Thay vào đó, báo lỗi và liệt kê các Exam đang dùng. |
| BR-3-09 | Khi duplicate Question public, bản copy thuộc về Teacher thực hiện duplicate; không có liên kết ngược lại original. |
| BR-3-10 | Tag là free-form text, không validate danh sách cố định. Normalize về lowercase khi lưu. |
| BR-3-11 | Giới hạn số Question Teacher có thể tạo theo config hệ thống (chuẩn bị cho premium MVP-8). Mặc định unlimited ở MVP-3. |

---

## 7. States & Workflows

### Question Visibility

```
Private (default) ──[Teacher set public]──► Public
Public ──[Teacher set private]──► Private
```

### Question deletion check

```
Teacher muốn xoá Question
    │
    ├──[Question đang dùng trong Exam]──► Từ chối, liệt kê Exam đang dùng
    │
    └──[Question không dùng ở đâu]──► Xoá thành công (soft delete hoặc hard delete)
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `questions` | id, teacher_id, subject_id, type (enum), content (text/HTML), difficulty, suggested_point, visibility (Private/Public), is_deleted, created_at, updated_at |
| `question_options` | id, question_id, content, is_correct, display_order — chỉ cho SingleChoice / MultipleChoice / TrueFalse |
| `question_tags` | id, question_id, tag (normalized lowercase) |

> **Lưu ý thiết kế:** `content` của Question nên hỗ trợ rich text (HTML hoặc Markdown) để Teacher chèn công thức, hình ảnh. Chọn format ở phase implementation.

---

## 9. Permission Matrix

| Action | Student | Teacher (owner) | Teacher (khác) | Admin |
|---|---|---|---|---|
| Tạo Question | Không | Có | Không | Không |
| Sửa Question | Không | Có (của mình) | Không | Không |
| Xoá Question | Không | Có (của mình, nếu không dùng trong Exam) | Không | Không |
| Xem Question của mình | Không | Có | Không | Không |
| Xem Question Public | Không | Có (read-only) | Có (read-only) | Có |
| Duplicate Question Public | Không | Có | Có | Không |
| Set visibility Public/Private | Không | Có (của mình) | Không | Không |
| Xem tất cả Question (bao gồm Private) | Không | Không | Không | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| SingleChoice: Teacher không chọn đáp án đúng | Validate khi save: bắt buộc có đúng 1 option là correct |
| MultipleChoice: Teacher không chọn đáp án đúng nào | Validate: bắt buộc ít nhất 1 option là correct |
| Xoá Question đang trong Exam | Từ chối, trả về danh sách Exam đang dùng |
| Teacher sửa content/options của Question đang trong Exam | Cho phép sửa; Exam snapshot (MVP-4/5) đảm bảo Attempt cũ không bị ảnh hưởng |
| Duplicate Question Public về Private và sửa | Hoàn toàn cho phép; bản copy độc lập với original |
| Tag có ký tự đặc biệt hoặc khoảng trắng thừa | Trim whitespace, normalize lowercase khi lưu |
| Teacher tìm kiếm Question với Subject đã inactive | Vẫn hiển thị Question cũ đã gắn Subject inactive (không ẩn đi) |
| Teacher xoá Question đang được dùng trong Assignment đang `Open` | Không thể xoá (Question đang có trong Exam đang dùng trong Assignment); phải đợi Assignment kết thúc hoặc gỡ khỏi Exam trước |

---

## 11. Acceptance Criteria

- [ ] Teacher tạo được câu hỏi với đủ 5 loại.
- [ ] `SingleChoice` và `MultipleChoice` validate đáp án đúng khi save.
- [ ] Teacher tìm kiếm câu hỏi theo Subject, loại, difficulty, tag, keyword → trả đúng kết quả.
- [ ] Teacher set Question là Public → Teacher khác thấy trong trang Question Public.
- [ ] Teacher duplicate Question Public → bản copy xuất hiện trong bank của Teacher đó với visibility = Private.
- [ ] Teacher xoá Question đang trong Exam → nhận lỗi rõ ràng.
- [ ] Teacher xoá Question không dùng ở đâu → xoá thành công.
- [ ] Student không thể truy cập Question Bank (API trả 403).

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Content câu hỏi chứa HTML/script độc hại (XSS) | Cao | Sanitize content khi lưu và khi render; dùng DOMPurify hoặc tương đương |
| Question Bank của Teacher quá lớn → tìm kiếm chậm | Trung bình | Index theo subject_id, teacher_id, type, difficulty; full-text search cho keyword |
| Teacher sửa đáp án Question đang dùng trong Exam đang mở | Trung bình | Snapshot ở MVP-4/5 giải quyết; cảnh báo UI khi Teacher sửa câu hỏi đang được dùng |

---

## 13. Verification Plan

**Scenario 1 — CRUD các loại câu hỏi:**
1. Tạo Question SingleChoice với 4 lựa chọn, 1 đáp án đúng → save thành công.
2. Tạo Question MultipleChoice không chọn đáp án đúng → nhận lỗi validation.
3. Tạo Question TrueFalse, ShortWriting, LongWriting → save thành công.

**Scenario 2 — Tìm kiếm và lọc:**
1. Tạo 10 câu hỏi với Subject, difficulty, tag khác nhau.
2. Lọc theo Subject "Toán" → chỉ thấy câu hỏi thuộc Toán.
3. Lọc theo difficulty "Hard" → đúng kết quả.
4. Tìm kiếm keyword trong content → trả đúng câu hỏi.

**Scenario 3 — Public/Private và duplicate:**
1. Teacher A set Question Public → Teacher B thấy ở trang Public.
2. Teacher B duplicate về bank → bản copy xuất hiện ở Teacher B với visibility Private.
3. Teacher B sửa bản copy → không ảnh hưởng original của Teacher A.

**Scenario 4 — Xoá có ràng buộc:**
1. Tạo Exam từ Question (MVP-4) → quay lại xoá Question → nhận lỗi.
2. Gỡ Question khỏi Exam → xoá Question thành công.

---

## 14. Hook sang MVP-4

MVP-4 (Exam Builder) sẽ:
- Cho Teacher chọn Question từ bank của mình (query `questions` theo `teacher_id`)
- Gán điểm thực tế (override `suggested_point`) khi thêm Question vào Exam
- Tạo Exam với danh sách Question có thứ tự → chuẩn bị cho Snapshot ở MVP-5
