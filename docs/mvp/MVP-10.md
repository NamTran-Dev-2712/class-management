# MVP-10 — Secure Exam & Anti-Cheat Proctoring (Browser Lockdown)

## 1. Context & Mục tiêu

Đến hết MVP-9, khi Student bấm vào một Assignment để làm bài, trang làm bài (`attempt.page.tsx`) chạy ở **chế độ cửa sổ bình thường, không fullscreen, không hề giám sát hành vi**. Student có thể tự do mở tab mới để tra Google/ChatGPT, copy đề ra ngoài, paste đáp án vào, mở ứng dụng khác — gian lận cực kỳ dễ. Hệ thống hiện chỉ ghi `IpAddress`/`UserAgent` một lần lúc Start và không có bất kỳ tín hiệu liêm chính nào khác.

MVP-10 thêm **lớp khoá trình duyệt (browser lockdown) + nhật ký vi phạm phía server** để việc gian lận khó hơn đáng kể và Teacher có bằng chứng review. Cách tiếp cận là **chỉ dùng trình duyệt** (fullscreen, phát hiện chuyển tab/mất focus, chặn copy/paste/right-click, đếm vi phạm, auto-submit khi vượt ngưỡng) — **không dùng webcam** để tránh gánh nặng quyền riêng tư/pháp lý và ship được ngay. Proctoring bằng camera/AI vẫn nằm ở ROADMAP-FUTURE (11.2).

Quan điểm trung thực xuyên suốt: **proctoring trên trình duyệt là best-effort, không tuyệt đối** — nó nâng rào cản và tạo audit trail, không phải tường lửa chống gian lận hoàn hảo. Server luôn là nguồn sự thật (deadline, đếm vi phạm, quyết định auto-submit); client chỉ phát tín hiệu.

**Kết quả cụ thể:** Teacher bật proctoring cho từng Assignment; Student làm bài trong fullscreen bị giám sát; mỗi hành vi nghi vấn được cảnh báo + ghi log server; vượt ngưỡng vi phạm → auto-submit/khoá theo cấu hình; Teacher xem được số vi phạm + timeline sự kiện của từng Attempt.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-5 (Assignment & Online Testing)** hoàn thành: vòng đời Attempt (`StartAttempt`/`SaveAttemptAnswers`/`SubmitAttempt`), deadline server-authoritative, shared `AttemptGrading.FinalizeAsync`, partial unique index `uq_attempts_one_in_progress`, trang làm bài có countdown + auto-save.
- **MVP-6 (Reports)** hoàn thành: roster Attempt + report cho Teacher để gắn cột liêm chính.
- **MVP-7 (Audit log + Notification)** hoàn thành: ghi audit cho force-submit/flag và notify khi cần.
- (Khuyến nghị) **MVP-9** đã có hạ tầng storage — không bắt buộc cho lockdown nhưng là tiền đề cho proctoring nâng cao về sau.

---

## 3. Trong scope

### Cấu hình Proctoring theo Assignment
- Thêm cấu hình proctoring trên `Assignment` (+ form FE `assignment-form.tsx`/`assignment.schema.ts`):
  - `RequireFullscreen` (bắt buộc fullscreen khi làm bài)
  - `DetectTabSwitch` (ghi vi phạm khi chuyển tab/mất focus)
  - `BlockCopyPaste` (chặn copy/cut/paste/right-click trên vùng làm bài)
  - `MaxViolations` (số vi phạm tối đa; `0 = không giới hạn / chỉ log`)
  - `ViolationAction` (`WarnOnly` / `AutoSubmit` / `LockAttempt`)
- Mặc định **tắt toàn bộ** (giữ nguyên hành vi hiện tại — backward compatible).

### Ghi nhận sự kiện liêm chính (server-authoritative)
- Bảng con append-only `attempt_events` (FK → attempts): `event_type`, `occurred_at`, `metadata` (jsonb tuỳ chọn).
- Cột denormalized trên `attempts`: `violation_count`, `is_flagged` (và optional `last_event_at`).
- Endpoint mới `PATCH /api/student/assignments/.../attempts/{id}/events` — **tái dùng** owner-guard + check `InProgress` + check deadline từ `SaveAttemptAnswersCommandHandler`.
- **Server** quyết định khi nào vượt ngưỡng (client không đáng tin); vượt `MaxViolations` → kích hoạt `ViolationAction` (auto-submit qua shared `AttemptGrading.FinalizeAsync`, hoặc khoá Attempt).
- Loại sự kiện chuẩn (hằng số, tương tự `AuditActions`): `TabSwitch`, `FocusLoss`, `FullscreenExit`, `CopyAttempt`, `PasteAttempt`, `ContextMenu`, `DevToolsOpen`, `ReloadAttempt`, `InactivityTimeout`.

### Browser Lockdown (Frontend)
- Trong `TakingView` của `attempt.page.tsx`:
  - Yêu cầu fullscreen khi Start (nếu `RequireFullscreen`).
  - Gắn handler: `visibilitychange`, `blur`/`focus`, `fullscreenchange`, `copy`/`cut`/`paste`, `contextmenu`, `beforeunload`.
  - Modal cảnh báo mỗi lần vi phạm, hiển thị **số vi phạm đang đếm** và ngưỡng.
  - Gửi event lên server theo batch debounce (giống auto-save).
  - Graceful degradation: nếu trình duyệt chặn fullscreen, bài vẫn làm được nhưng ghi log sự kiện.

### Màn hình review cho Teacher/Admin
- Roster Attempt + chi tiết Attempt hiển thị: `violation_count`, badge `flagged`, và **timeline sự kiện**.
- Dữ liệu liêm chính bổ sung vào report sẵn có (MVP-6) và audit (MVP-7). Admin xem read-only.

### Các kịch bản gian lận thực tế được xử lý
1. **Mở tab/cửa sổ khác để tra cứu** → `visibilitychange` + window `blur` → `TabSwitch`/`FocusLoss`, đếm vi phạm.
2. **Thoát fullscreen để xem app khác** → `fullscreenchange` → `FullscreenExit`, nhắc vào lại fullscreen, đếm.
3. **Copy đề ra ngoài / paste đáp án vào** (vd ChatGPT) → chặn `copy/cut/paste/contextmenu` → ghi `CopyAttempt`/`PasteAttempt`.
4. **Mở DevTools / view-source** → phát hiện best-effort (chặn phím tắt + heuristic) → `DevToolsOpen` (đánh dấu độ tin cậy thấp).
5. **Reload/điều hướng để né timer** → `beforeunload` guard + deadline server-authoritative; reload vào lại lockdown, timer vẫn chạy theo giờ server.
6. **Mở cùng Attempt ở tab/thiết bị thứ 2** → đã chặn sẵn bởi bất biến một-`InProgress`-attempt (`uq_attempts_one_in_progress`).
7. **Ngồi không / mất focus lâu** → log `InactivityTimeout` (tuỳ chọn).
8. **Vượt số vi phạm cho phép** → server vượt `MaxViolations` → auto-submit hoặc khoá theo cấu hình.

---

## 4. Ngoài scope (Out of Scope)

- Webcam / proctoring bằng AI (face detection, phát hiện nhiều người) → ROADMAP-FUTURE (11.2)
- Screen recording / ghi màn hình → ROADMAP-FUTURE
- Lockdown cấp hệ điều hành (chặn alt-tab, app khác) — vượt khả năng trình duyệt → ROADMAP-FUTURE
- Phát hiện màn hình thứ 2 / thiết bị ngoài → ROADMAP-FUTURE
- IP allowlist / geofencing phòng thi → phase sau
- Xác thực danh tính (ID verification) trước khi thi → ROADMAP-FUTURE

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T10-01 | Là Teacher, tôi muốn bật chế độ fullscreen bắt buộc cho Assignment, để hạn chế Student mở app khác. |
| T10-02 | Là Teacher, tôi muốn phát hiện khi Student chuyển tab/mất focus, để biết hành vi nghi vấn. |
| T10-03 | Là Teacher, tôi muốn chặn copy/paste khi làm bài, để giảm gian lận sao chép. |
| T10-04 | Là Teacher, tôi muốn đặt ngưỡng vi phạm và hành động (cảnh báo/tự nộp/khoá), để xử lý tự động. |
| T10-05 | Là Teacher, tôi muốn xem số vi phạm + timeline sự kiện của mỗi Attempt, để đánh giá liêm chính. |
| T10-06 | Là Teacher, tôi muốn Attempt vi phạm nhiều bị đánh dấu (flag), để rà soát nhanh. |

### Student

| ID | Story |
|---|---|
| S10-01 | Là Student, tôi muốn biết Assignment này đang ở chế độ giám sát và những gì bị ghi nhận, để làm bài đúng quy định. |
| S10-02 | Là Student, tôi muốn được cảnh báo khi sắp vượt ngưỡng vi phạm, để không bị tự nộp ngoài ý muốn. |
| S10-03 | Là Student, tôi muốn nếu lỡ thoát fullscreen/reload thì vào lại được mà không mất bài, để không bị thiệt. |

### Admin

| ID | Story |
|---|---|
| A10-01 | Là Admin, tôi muốn xem dữ liệu liêm chính của Attempt (read-only), để hỗ trợ xử lý khiếu nại. |
| A10-02 | Là Admin, tôi muốn các hành động force-submit/flag được audit log, để truy vết. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-10-01 | Cấu hình proctoring nằm trên từng Assignment; mặc định tắt (giữ hành vi cũ). |
| BR-10-02 | `attempt_events` là append-only; không sửa/xoá; là bằng chứng. |
| BR-10-03 | Server là nguồn sự thật về `violation_count` và quyết định vượt ngưỡng; client chỉ phát tín hiệu. |
| BR-10-04 | Endpoint ghi event tái dùng owner-guard + chỉ chấp khi Attempt `InProgress` và chưa quá deadline. |
| BR-10-05 | Khi `violation_count > MaxViolations` (và `MaxViolations > 0`): áp `ViolationAction` (auto-submit/khoá) qua shared `AttemptGrading.FinalizeAsync`. |
| BR-10-06 | Proctoring **không** thêm trạng thái mới cho Attempt; auto-submit do vi phạm dùng đúng đường submit/auto-submit hiện có (đánh dấu `auto_submitted`). |
| BR-10-07 | Mất tín hiệu client (đóng tab, mất mạng) không làm mất bài: auto-save + deadline server giữ nguyên; reload vào lại lockdown. |
| BR-10-08 | Mở cùng Attempt ở tab/thiết bị thứ 2 bị chặn bởi bất biến một-`InProgress`-attempt sẵn có. |
| BR-10-09 | Phát hiện DevTools là best-effort, độ tin cậy thấp — ghi log nhưng không nên dùng làm căn cứ kỷ luật duy nhất. |
| BR-10-10 | Force-submit/flag do Teacher/Admin được ghi audit log (MVP-7). |

---

## 7. States & Workflows

### Proctoring Violation Flow

```
[Client phát hiện hành vi: tab switch / fullscreen exit / copy / paste / ...]
    │
    ▼
[Hiển thị modal cảnh báo + tăng counter hiển thị]
    │
    ▼
[PATCH .../events  (batch debounce)]  ── owner + InProgress + chưa quá deadline ──► (fail → bỏ qua)
    │ (ok)
    ▼
Ghi attempt_events  +  server tăng violation_count
    │
    ├──[MaxViolations = 0 hoặc chưa vượt]──► tiếp tục làm bài (chỉ log)
    │
    └──[violation_count > MaxViolations > 0]──► áp ViolationAction
                                                   │
                                ┌──────────────────┼──────────────────┐
                                │                   │                  │
                            WarnOnly           AutoSubmit          LockAttempt
                          (chỉ cảnh báo)   (FinalizeAsync nộp)   (khoá, chờ Teacher)
```

### Quan hệ với Attempt State Machine (không đổi trạng thái)

```
InProgress ──(vi phạm vượt ngưỡng + ViolationAction=AutoSubmit)──► Submitted (auto_submitted=true)
InProgress ──(reload / mất focus tạm)──► InProgress (vào lại lockdown, timer server tiếp tục)
InProgress ──(deadline server)──► auto-submit như MVP-5 (độc lập proctoring)
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `attempt_events` | id, attempt_id (FK), event_type (TabSwitch/FocusLoss/FullscreenExit/CopyAttempt/PasteAttempt/ContextMenu/DevToolsOpen/ReloadAttempt/InactivityTimeout), occurred_at, metadata (jsonb), created_at — append-only |
| `attempts` (mở rộng) | thêm `violation_count` (int), `is_flagged` (bool), `last_event_at?` |
| `assignments` (mở rộng) | thêm `require_fullscreen`, `detect_tab_switch`, `block_copy_paste`, `max_violations`, `violation_action` |

---

## 9. Permission Matrix

| Action | Student | Teacher | Admin |
|---|---|---|---|
| Cấu hình proctoring cho Assignment | Không | Có (của mình) | Không |
| Ghi attempt_event (khi đang làm bài) | Có (Attempt của mình) | Không | Không |
| Xem violation_count + timeline của Attempt | Không (chỉ thấy cảnh báo) | Có (Assignment của mình) | Có (tất cả) |
| Force-submit Attempt | Không | Có (của mình) | Có |
| Flag / bỏ flag Attempt | Không | Có (của mình) | Có |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Trình duyệt chặn Fullscreen API | Bài vẫn làm được; ghi log `FullscreenExit`/không vào được fullscreen; không khoá cứng |
| Student mất mạng giữa chừng, không gửi được event | Auto-save + deadline server giữ nguyên; event sẽ gửi lại khi có mạng (best-effort) |
| Client gửi event giả/spam để phá | Server rate-limit per-user + chỉ đếm khi `InProgress`; `violation_count` do server tính |
| Reload trang để reset counter | Counter ở server, reload không reset; reload có thể tự ghi `ReloadAttempt` |
| Quá deadline mới gửi event | Endpoint từ chối (giống save sau deadline) |
| `MaxViolations = 0` | Chỉ log, không bao giờ auto-submit/khoá |
| ViolationAction = LockAttempt rồi Student khiếu nại | Teacher/Admin xem timeline + audit để quyết định mở lại |
| False positive (mất focus do thông báo hệ thống) | Có ngưỡng + Teacher review; không kỷ luật tự động cứng nếu chỉ 1 sự kiện |
| Làm bài trên mobile (fullscreen hạn chế) | Degrade graceful; tài liệu hoá hành vi mobile; ưu tiên tín hiệu visibility thay vì fullscreen |

---

## 11. Acceptance Criteria

- [ ] Teacher bật `RequireFullscreen` → Student vào fullscreen khi Start; thoát fullscreen bị ghi `FullscreenExit` + cảnh báo.
- [ ] Chuyển tab/mất focus → ghi `TabSwitch`/`FocusLoss`, `violation_count` tăng phía server.
- [ ] Bật `BlockCopyPaste` → copy/paste/right-click bị chặn và ghi log.
- [ ] Vượt `MaxViolations` với `ViolationAction=AutoSubmit` → Attempt tự nộp (auto_submitted=true) qua đường grading chung.
- [ ] Reload trang → không mất bài, vào lại lockdown, timer server tiếp tục, counter không reset.
- [ ] Teacher xem được `violation_count` + timeline sự kiện của từng Attempt; Attempt vi phạm nhiều có badge flag.
- [ ] Mặc định (proctoring tắt) → trải nghiệm làm bài y như MVP-5 (backward compatible).
- [ ] Force-submit/flag được ghi audit log.
- [ ] Endpoint event từ chối khi Attempt không `InProgress` hoặc quá deadline.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Proctoring trình duyệt bị bypass (kỹ thuật) | Cao | Tài liệu hoá rõ là best-effort; nhiều tín hiệu kết hợp + audit; camera proctoring để FUTURE |
| False positive gây oan cho Student | Trung bình | Ngưỡng cấu hình + Teacher review timeline; không kỷ luật cứng theo 1 sự kiện |
| Quyền riêng tư khi giám sát hành vi | Trung bình | Không webcam; thông báo rõ cho Student trước khi làm; chỉ log sự kiện kỹ thuật |
| Client spam event làm sai counter | Trung bình | Rate-limit per-user + counter tính ở server, chỉ khi InProgress |
| Phát hiện DevTools không đáng tin | Thấp | Đánh dấu độ tin cậy thấp; không dùng làm căn cứ duy nhất (BR-10-09) |
| Trải nghiệm mobile kém do fullscreen hạn chế | Trung bình | Degrade graceful; ưu tiên visibility signal; test mobile |

---

## 13. Verification Plan

**Scenario 1 — Fullscreen + tab switch:**
1. Teacher bật `RequireFullscreen` + `DetectTabSwitch`, `MaxViolations=3`, `ViolationAction=AutoSubmit`.
2. Student Start → vào fullscreen.
3. Student chuyển tab 3 lần → mỗi lần modal cảnh báo + ghi `TabSwitch`.
4. Lần vượt ngưỡng → server auto-submit Attempt.

**Scenario 2 — Block copy/paste:**
1. Bật `BlockCopyPaste` → Student thử copy đề/paste đáp án → bị chặn + ghi `CopyAttempt`/`PasteAttempt`.

**Scenario 3 — Reload không mất bài, counter không reset:**
1. Student vi phạm 2 lần (counter=2) → reload trang.
2. Vào lại lockdown; counter vẫn = 2 (server); timer tiếp tục theo giờ server; đáp án đã auto-save còn nguyên.

**Scenario 4 — Backward compatible:**
1. Assignment để proctoring tắt → làm bài như MVP-5, không có cảnh báo/log.

**Scenario 5 — Teacher review:**
1. Sau khi nộp, Teacher mở chi tiết Attempt → thấy `violation_count` + timeline + badge flag.
2. Teacher force-submit/flag một Attempt khác → audit log ghi nhận.

**Scenario 6 — Chống spam event:**
1. Gửi nhiều event sau deadline / khi không InProgress → bị từ chối, counter không đổi.

---

## 14. Hook sang MVP-11

MVP-10 nâng độ tin cậy của kết quả thi bằng lockdown + log vi phạm. MVP-11 mở rộng **chiều sâu nội dung đề** với các loại câu hỏi nâng cao (Fill-in-Blank, Matching, Ordering) — chấm tự động, tái dùng snapshot + đường grading chung, và hưởng lợi từ media của MVP-9. Proctoring nâng cao (camera/AI) tiếp tục ở ROADMAP-FUTURE (11.2).
