# MVP-11 — Advanced Question Types (Fill-in-Blank, Matching, Ordering)

## 1. Context & Mục tiêu

Đến hết MVP-10, hệ thống mới có 5 loại Question: `SingleChoice`, `MultipleChoice`, `TrueFalse`, `ShortWriting`, `LongWriting`. Trong đó 3 loại đầu chấm tự động, 2 loại writing phải chấm tay. Khoảng giữa — những loại **chấm tự động được nhưng phong phú hơn trắc nghiệm** — đang trống, khiến đề kiểm tra thiếu chiều sâu (không có điền khuyết, nối đôi, sắp xếp thứ tự).

MVP-11 bổ sung **3 loại câu hỏi nâng cao chấm tự động**: `FillInBlank` (điền khuyết), `Matching` (nối đôi), `Ordering` (sắp xếp thứ tự) — nâng các mục 6.1/6.2/6.3 từ ROADMAP-FUTURE thành MVP thực thi. Các loại mới **tái dùng nguyên** kiến trúc aggregate-có-children sẵn có (Question sở hữu Options/Tags, wholesale-replace khi update), cơ chế snapshot bất biến, và đường grading chung `AttemptGrading` (dùng cho cả submit lẫn auto-submit). Câu hỏi mới cũng hưởng lợi từ media (MVP-9): ảnh trong đề điền khuyết, audio trong nối đôi…

**Kết quả cụ thể:** Teacher tạo được câu điền khuyết / nối đôi / sắp xếp; Student làm được (gõ vào chỗ trống, kéo-thả nối/sắp xếp); hệ thống chấm tự động theo luật rõ ràng; kết quả nằm chung pipeline grade hiện có.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-3 (Question Bank)** hoàn thành: aggregate `Question` sở hữu `QuestionOption`/`Tags`, editor động, `vw_questions`, validator theo loại.
- **MVP-4 (Exam Builder)** hoàn thành: thêm câu hỏi vào exam, snapshot exam_questions.
- **MVP-5 (Assignment & Auto-grade)** hoàn thành: `IAutoGradingService` thuần, snapshot question/option, shared `AttemptGrading.FinalizeAsync`, trang làm bài `QuestionCard`.
- (Khuyến nghị) **MVP-9** để câu hỏi mới có thể nhúng media.
- FE đã có `@dnd-kit` (dùng ở exam builder) — tái dùng cho kéo-thả Matching/Ordering.

---

## 3. Trong scope

### Loại câu hỏi mới
- Mở rộng enum `QuestionType`: `FillInBlank`, `Matching`, `Ordering` (vẫn là enum-as-string, qua `JsonStringEnumConverter` toàn cục).
- Cả 3 đều **chấm tự động** (`auto-gradable`), chạy chung pipeline auto-grade.

### Mô hình dữ liệu (tái dùng/mở rộng `QuestionOption`, hạn chế bảng mới)
- **FillInBlank**: content chứa chỗ trống dạng `{{1}}`, `{{2}}`… ; mỗi blank có danh sách **đáp án chấp nhận** (nhiều biến thể). Lưu qua `QuestionOption` mở rộng (mỗi option = một đáp án chấp nhận cho 1 blank, mang `blank_index`) hoặc cột cấu trúc — quyết định cuối khi implement.
- **Matching**: tập "vế trái" ↔ "vế phải" + cặp đúng. Lưu các phần tử + ánh xạ đúng (vd `match_key` ghép cặp).
- **Ordering**: tập phần tử + thứ tự đúng (`correct_position`).

### Auto-grading (mở rộng `IAutoGradingService` — vẫn thuần, không DB)
- **FillInBlank**: so khớp theo blank; mặc định case-insensitive + trim; tuỳ chọn `exact-match`; mỗi blank có nhiều đáp án chấp nhận; điểm = đúng-tất-cả-blank → full (partial theo số blank đúng là tuỳ chọn, ghi rõ).
- **Matching**: mặc định all-or-nothing (đúng toàn bộ cặp mới có điểm — nhất quán BR-5-12 của MultipleChoice); partial-credit là quyết định tường minh.
- **Ordering**: mặc định all-or-nothing (đúng toàn bộ thứ tự); partial (vd Kendall-tau / số cặp đúng) là tuỳ chọn ghi rõ.

### Frontend
- **Editor** (`_shared/question-form.tsx` + options editor động): UI riêng theo loại — quản lý blank + đáp án chấp nhận (FillInBlank); danh sách cặp trái-phải (Matching); danh sách phần tử + thứ tự đúng (Ordering).
- **Taker** (`QuestionCard` trong `attempt.page.tsx`): ô input cho từng blank (FillInBlank); kéo-thả nối cặp (Matching) và sắp xếp (Ordering) qua `@dnd-kit`, có fallback nút lên/xuống/select cho khả dụng.
- **Preview & review**: hiển thị đáp án đúng khi `show_answers_after_grade` và đã release (giống các loại hiện có).

### Snapshot & nhất quán grading
- Loại mới được copy vào `snapshot_questions`/`snapshot_options` lúc publish (cấu trúc blank/cặp/thứ tự được đóng băng).
- Auto-grade chạy đúng shared `AttemptGrading` cho cả submit thủ công lẫn auto-submit lifecycle — không có đường chấm riêng.
- Lưu câu trả lời Student tái dùng `attempt_answers` (`selected_option_ids` jsonb + `text_answer`); với loại mới có thể cần biểu diễn câu trả lời dạng cấu trúc (vd thứ tự đã chọn, cặp đã nối) trong jsonb — quyết định cuối khi implement.

---

## 4. Ngoài scope (Out of Scope)

- Hotspot / image-map (chọn vùng trên ảnh) → ROADMAP-FUTURE
- So khớp công thức Toán / LaTeX equivalence (vd `1/2` = `0.5`) → ROADMAP-FUTURE
- Câu hỏi chạy code (code execution) → ROADMAP-FUTURE
- Partial score nâng cao cho MultipleChoice (mục 7.1 ROADMAP-FUTURE) → giữ riêng
- Rubric-based grading (7.2) → ROADMAP-FUTURE
- Câu hỏi điền khuyết tự luận chấm tay (đã có ShortWriting) — không trùng lặp

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T11-01 | Là Teacher, tôi muốn tạo câu điền khuyết với nhiều chỗ trống và nhiều đáp án chấp nhận, để kiểm tra kiến thức linh hoạt. |
| T11-02 | Là Teacher, tôi muốn tạo câu nối đôi (Matching) trái-phải, để kiểm tra khả năng liên kết khái niệm. |
| T11-03 | Là Teacher, tôi muốn tạo câu sắp xếp thứ tự (Ordering), để kiểm tra trình tự/quy trình. |
| T11-04 | Là Teacher, tôi muốn các loại mới được chấm tự động, để không phải chấm tay. |
| T11-05 | Là Teacher, tôi muốn chọn chế độ chấm (đúng-tất-cả hoặc partial) cho loại mới, để phù hợp mục tiêu đánh giá. |
| T11-06 | Là Teacher, tôi muốn chèn ảnh/audio vào câu hỏi loại mới, để đề phong phú hơn. |

### Student

| ID | Story |
|---|---|
| S11-01 | Là Student, tôi muốn gõ đáp án vào từng chỗ trống một cách rõ ràng, để trả lời câu điền khuyết. |
| S11-02 | Là Student, tôi muốn kéo-thả để nối cặp và sắp xếp thứ tự, để làm bài trực quan. |
| S11-03 | Là Student, tôi muốn xem lại đáp án đúng của loại mới sau khi có điểm (nếu được phép), để học từ lỗi sai. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-11-01 | `FillInBlank`/`Matching`/`Ordering` là loại auto-gradable; chạy chung `IAutoGradingService` + `AttemptGrading`. |
| BR-11-02 | FillInBlank mặc định so khớp case-insensitive + trim; mỗi blank cho phép nhiều đáp án chấp nhận; `exact-match` là tuỳ chọn. |
| BR-11-03 | Matching/Ordering mặc định all-or-nothing (đúng toàn bộ mới có điểm), nhất quán BR-5-12; partial là tuỳ chọn tường minh. |
| BR-11-04 | Loại mới được copy đầy đủ cấu trúc (blank/cặp/thứ tự) vào snapshot lúc publish; chấm dựa trên snapshot, không phải Question gốc. |
| BR-11-05 | Validation theo loại: FillInBlank phải có ≥1 blank và mỗi blank ≥1 đáp án; Matching phải có ánh xạ đầy đủ; Ordering phải có thứ tự đúng cho mọi phần tử. |
| BR-11-06 | Số phần tử/blank/cặp có giới hạn trên (tunable) để tránh câu hỏi quá lớn. |
| BR-11-07 | Câu trả lời Student cho loại mới lưu trong `attempt_answers` (jsonb cấu trúc), tái dùng cơ chế auto-save sẵn có. |
| BR-11-08 | Loại mới tái dùng aggregate wholesale-replace của Question (update thay toàn bộ children, cascade orphan). |
| BR-11-09 | Tag/difficulty/visibility/public-pool/duplicate áp dụng y như các loại Question hiện có. |

---

## 7. States & Workflows

### Vòng đời câu hỏi mới (tái dùng Question + Snapshot)

```
Teacher tạo Question (FillInBlank/Matching/Ordering)
    │  (validate theo loại: blank/cặp/thứ tự + đáp án đúng)
    ▼
Lưu vào bank (aggregate + children, wholesale-replace khi update)
    │
    ▼
Thêm vào Exam ──► Publish Assignment ──► copy cấu trúc vào snapshot (bất biến)
    │
    ▼
Student làm bài (gõ blank / kéo-thả nối / sắp xếp)  ──auto-save──► attempt_answers (jsonb)
    │
    ▼
Submit / auto-submit ──► AttemptGrading.FinalizeAsync ──► IAutoGradingService chấm theo loại
```

### Luật chấm theo loại

```
FillInBlank : mỗi blank → so khớp (case-insensitive+trim, nhiều đáp án chấp nhận)
              → mặc định: đúng tất cả blank = full point (partial = tuỳ chọn theo % blank đúng)

Matching    : so toàn bộ cặp đã nối với cặp đúng
              → mặc định: đúng tất cả = full point (partial = tuỳ chọn theo số cặp đúng)

Ordering    : so toàn bộ thứ tự đã chọn với thứ tự đúng
              → mặc định: đúng tất cả = full point (partial = tuỳ chọn theo số vị trí đúng)
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `questions` (mở rộng) | `type` thêm giá trị `FillInBlank`/`Matching`/`Ordering`; content chứa placeholder `{{n}}` cho FillInBlank |
| `question_options` (mở rộng) | tái dùng cho đáp án/phần tử loại mới: thêm `blank_index?` (FillInBlank), `match_key?` (Matching), `correct_position?` (Ordering) — tuỳ loại |
| `snapshot_questions` / `snapshot_options` (mở rộng) | đóng băng cấu trúc loại mới (blank/cặp/thứ tự) lúc publish |
| `attempt_answers` (tái dùng) | `selected_option_ids` (jsonb) + biểu diễn cấu trúc cho thứ tự/cặp đã chọn; `text_answer` cho nội dung blank nếu cần |

> Ghi chú thiết kế: ưu tiên mở rộng `question_options` thay vì tạo bảng mới để tái dùng tối đa aggregate + snapshot. Nếu cấu trúc một loại quá khác (vd Matching cần 2 cột phần tử), cân nhắc cột phân biệt vai trò (`side`/`role`) trong `question_options`. Quyết định cuối khi implement.

---

## 9. Permission Matrix

| Action | Student | Teacher | Admin |
|---|---|---|---|
| Tạo/sửa Question loại mới | Không | Có (của mình) | Không |
| Xem trong public pool / duplicate | Không | Có | Có (tất cả) |
| Làm bài (trả lời loại mới) | Có (Attempt của mình) | Không | Không |
| Xem đáp án đúng sau khi có điểm | Có (nếu được phép) | Có | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| FillInBlank không có blank nào | Validator từ chối: "Cần ít nhất 1 chỗ trống `{{1}}`" |
| Blank trong content không khớp số đáp án khai báo | Từ chối: số blank phải khớp số nhóm đáp án |
| FillInBlank đáp án khác hoa/thường, thừa space | Mặc định chuẩn hoá (case-insensitive + trim) trước khi so |
| Matching số vế trái ≠ số vế phải / thiếu cặp đúng | Từ chối; yêu cầu ánh xạ đầy đủ |
| Ordering có phần tử trùng vị trí / thiếu thứ tự | Từ chối; thứ tự phải là hoán vị đầy đủ |
| Student bỏ trống một phần (blank/cặp/thứ tự) | Chấm theo dữ liệu có; thiếu → coi như sai phần đó (all-or-nothing → 0 nếu không đủ) |
| Câu hỏi loại mới có quá nhiều phần tử | Giới hạn trên (tunable) tại validator |
| Snapshot loại mới khi Question gốc đổi sau publish | Chấm theo snapshot (BR-11-04), không theo bản gốc đã sửa |
| Kéo-thả không khả dụng (mobile/screen reader) | Fallback select/nút lên-xuống cho Ordering/Matching |

---

## 11. Acceptance Criteria

- [ ] Teacher tạo câu FillInBlank nhiều blank, mỗi blank nhiều đáp án chấp nhận → lưu thành công.
- [ ] Teacher tạo câu Matching và Ordering với đáp án đúng → lưu thành công.
- [ ] Student làm bài: gõ blank, kéo-thả nối/sắp xếp; auto-save hoạt động.
- [ ] Submit → auto-grade đúng luật: FillInBlank (case-insensitive/trim, nhiều đáp án), Matching/Ordering all-or-nothing.
- [ ] Loại mới được copy vào snapshot; sửa Question gốc sau publish không đổi điểm Attempt cũ.
- [ ] Auto-submit lifecycle chấm loại mới qua đúng đường `AttemptGrading` (không có pipeline riêng).
- [ ] Tag/difficulty/visibility/duplicate/public-pool hoạt động với loại mới như loại cũ.
- [ ] Student xem lại đáp án đúng của loại mới khi được phép.
- [ ] Câu hỏi loại mới chèn được media (nếu MVP-9 đã có).

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Luật chấm FillInBlank gây tranh cãi (biến thể đáp án) | Trung bình | Cho nhiều đáp án chấp nhận + chuẩn hoá; Teacher có thể bổ sung biến thể; review điểm |
| Partial-credit phức tạp, dễ sai | Trung bình | Mặc định all-or-nothing; partial là tuỳ chọn tường minh, có unit test riêng |
| Biểu diễn câu trả lời cấu trúc trong jsonb phức tạp | Trung bình | Tái dùng value converter jsonb sẵn có; định nghĩa schema câu trả lời rõ ràng |
| UX kéo-thả khó trên mobile/accessibility | Trung bình | `@dnd-kit` + fallback select/nút; test mobile + keyboard |
| Snapshot loại mới sai cấu trúc | Cao | Tái dùng đúng cơ chế snapshot copy; test snapshot cho từng loại |
| Tăng phức tạp validator/grader | Thấp | Tách rõ theo loại; giữ `IAutoGradingService` thuần để unit test |

---

## 13. Verification Plan

**Scenario 1 — FillInBlank:**
1. Teacher tạo câu "Thủ đô VN là {{1}}" với đáp án chấp nhận ["Hà Nội", "Ha Noi", "hanoi"].
2. Student gõ "  ha noi " → auto-grade đúng (case-insensitive + trim).
3. Đáp án sai → 0 điểm.

**Scenario 2 — Matching:**
1. Teacher tạo câu nối 3 cặp khái niệm-định nghĩa.
2. Student nối đúng cả 3 → full; nối sai 1 → 0 (all-or-nothing mặc định).

**Scenario 3 — Ordering:**
1. Teacher tạo câu sắp xếp 4 bước quy trình.
2. Student sắp đúng toàn bộ → full; sai thứ tự → 0 (mặc định).

**Scenario 4 — Snapshot bất biến:**
1. Tạo câu loại mới → publish Assignment → Student Start.
2. Teacher sửa đáp án đúng của Question gốc.
3. Attempt vẫn chấm theo snapshot cũ.

**Scenario 5 — Auto-submit lifecycle:**
1. Assignment có loại mới, hết giờ khi Student chưa nộp.
2. Lifecycle job auto-submit → loại mới được chấm đúng qua `AttemptGrading`.

**Scenario 6 — Reuse Question features:**
1. Đặt tag/difficulty/visibility cho câu loại mới; duplicate từ public pool.
2. Hoạt động như các loại Question hiện có.

---

## 14. Hook sang MVP-12 / ROADMAP-FUTURE

MVP-11 hoàn thiện chiều sâu nội dung đề với các loại câu hỏi nâng cao chấm tự động. Các hướng mở rộng tiếp theo nằm ở [ROADMAP-FUTURE.md](./ROADMAP-FUTURE.md): loại câu hỏi nâng cao hơn (hotspot, LaTeX equivalence, code execution), advanced grading (partial MultipleChoice 7.1, rubric 7.2, peer review 7.3), AI question generation, gamification/leaderboard, contest, và in-class communication. Lựa chọn MVP kế tiếp tuỳ feedback người dùng thực.
