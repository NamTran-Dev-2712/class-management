# MVP-8 — Premium & Payment

## 1. Context & Mục tiêu

MVP-8 thêm **khả năng monetization** cho hệ thống. Đây là extension sau production — hệ thống đã có người dùng thực (post-MVP-7) và cần cơ chế thu phí để duy trì và phát triển.

MVP-8 xây dựng: subscription plans, payment integration (Momo / VNPay), resource limits theo plan, và lifecycle quản lý subscription đầy đủ.

**Kết quả cụ thể:** Teacher có thể nâng lên Pro plan, thanh toán qua cổng nội địa, và được hưởng tài nguyên mở rộng. Admin quản lý được subscription và payment history.

---

## 2. Tiền điều kiện (Dependencies)

- **MVP-7 hoàn thành và stable trong production**: Có người dùng thực, audit log, system settings.
- `system_settings` đã có `max_classes_per_teacher`, `max_questions_per_teacher`, `max_exams_per_teacher`.
- Tài khoản business với Momo Business API và VNPay merchant đã đăng ký.
- HTTPS và domain production đã có (bắt buộc cho payment provider).

---

## 3. Trong scope

### Plan & Pricing
- 2 plan cơ bản: `Free` và `Pro`
- Plan `Free` (mặc định cho tất cả Teacher):
  - Giới hạn theo `system_settings` (vd: max 3 class, max 100 question, max 10 exam)
- Plan `Pro`:
  - Không giới hạn (hoặc giới hạn rất cao theo config)
  - Billing cycle: monthly / annual
  - Pricing được lưu trong DB (không hard-code)
- Admin có thể tạo thêm plan trong tương lai (cấu hình flexible)
- Admin set Pro cho Teacher thủ công (fallback cho trường hợp đặc biệt: giáo viên demo, partnership)

### Subscription Lifecycle
- Subscription states: `Active` → `PastDue` → `Cancelled` / `Expired`
- Teacher mua plan Pro → tạo Subscription mới
- Subscription `Active` khi payment confirmed
- Subscription tự động expire khi đến ngày gia hạn mà không thanh toán thành công → `PastDue` → sau grace period (vd: 3 ngày) → `Expired`
- Teacher huỷ subscription: vẫn `Active` đến hết kỳ, sau đó `Cancelled`
- Teacher nâng từ Monthly → Annual: tính prorata hoặc kích hoạt từ kỳ mới (quyết định khi implement)
- Khi Subscription `Expired`: Teacher tự động về `Free` plan; class/question/exam hiện có không bị xoá, chỉ không tạo thêm được

### Payment Integration
- Tích hợp **Momo Business API** (QR + deep link)
- Tích hợp **VNPay** (chuyển khoản ngân hàng + ATM/Visa)
- Webhook xác nhận thanh toán từ cả 2 provider
- **Idempotency**: mỗi transaction có `idempotency_key`; xử lý webhook nhiều lần cho cùng transaction chỉ apply 1 lần
- Payment states: `Pending` → `Completed` / `Failed` / `Expired`
- Invoice tự động tạo khi payment `Completed`

### Resource Enforcement
- Mỗi lần Teacher tạo resource mới (Class / Question / Exam), kiểm tra limit theo plan
- Nếu vượt limit: trả lỗi rõ ràng với link "Nâng cấp lên Pro"
- Resource hiện có không bị xoá khi downgrade; chỉ không tạo thêm
- Teacher xem trạng thái sử dụng tài nguyên: "X/Y classes used"

### Admin Payment Management
- Admin xem danh sách tất cả Subscription
- Admin xem lịch sử payment (filter theo status, provider, date range)
- Admin set Pro thủ công cho Teacher (với ghi chú lý do)
- Admin xem revenue summary (total, by month, by plan) — basic

---

## 4. Ngoài scope (Out of Scope)

- Refund tự động (Admin xử lý thủ công qua cổng thanh toán, không tự động hoá trong MVP-8)
- Tax / VAT compliance
- Multi-currency (chỉ VND)
- Credit card quốc tế (Stripe) → ROADMAP-FUTURE
- Trial period (7-day free Pro) → có thể thêm sau
- Coupon / discount code → ROADMAP-FUTURE
- Billing per Class / per Student → ROADMAP-FUTURE
- B2B (trường học mua cho cả giáo viên) → ROADMAP-FUTURE
- Payment cho Student (Student cũng có premium) → ROADMAP-FUTURE

---

## 5. User Stories

### Teacher

| ID | Story |
|---|---|
| T8-01 | Là Teacher, tôi muốn xem thông tin về các plan và giá, để quyết định có nên nâng cấp không. |
| T8-02 | Là Teacher, tôi muốn thanh toán qua Momo hoặc VNPay, để sử dụng phương thức quen thuộc. |
| T8-03 | Là Teacher, tôi muốn nhận xác nhận và invoice sau khi thanh toán, để lưu trữ hồ sơ. |
| T8-04 | Là Teacher, tôi muốn xem trạng thái subscription và ngày gia hạn, để không bị gián đoạn dịch vụ. |
| T8-05 | Là Teacher, tôi muốn huỷ subscription và vẫn dùng được đến hết kỳ, để kiểm soát chi phí. |
| T8-06 | Là Teacher, tôi muốn biết khi nào sắp hết hạn subscription, để chủ động gia hạn. |
| T8-07 | Là Teacher, tôi muốn xem lịch sử thanh toán của mình, để kiểm tra giao dịch. |
| T8-08 | Là Teacher, tôi muốn biết mình đang dùng bao nhiêu tài nguyên so với giới hạn, để lên kế hoạch. |

### Admin

| ID | Story |
|---|---|
| A8-01 | Là Admin, tôi muốn xem tất cả Subscription và trạng thái, để nắm tổng thể người dùng trả phí. |
| A8-02 | Là Admin, tôi muốn set Pro thủ công cho 1 Teacher với ghi chú, để hỗ trợ trường hợp đặc biệt. |
| A8-03 | Là Admin, tôi muốn xem lịch sử payment và lọc theo provider/status, để đối chiếu với cổng thanh toán. |
| A8-04 | Là Admin, tôi muốn xem revenue summary theo tháng, để theo dõi sức khoẻ tài chính. |

---

## 6. Business Rules & Constraints

| # | Rule |
|---|---|
| BR-8-01 | Mỗi Teacher chỉ có 1 Subscription `Active` tại 1 thời điểm. |
| BR-8-02 | Subscription chỉ áp dụng cho Teacher; Student và Admin không có subscription. |
| BR-8-03 | Khi payment webhook đến, kiểm tra `idempotency_key` trước khi process. Cùng key → skip, không apply lại. |
| BR-8-04 | Resource check xảy ra tại thời điểm tạo resource, không retroactively xoá khi downgrade. |
| BR-8-05 | Grace period khi payment thất bại: Subscription vào `PastDue`, giữ nguyên quyền Pro trong 3 ngày. Sau 3 ngày không thanh toán → `Expired`. |
| BR-8-06 | Invoice được tạo tự động khi payment `Completed`, không thể sửa hay xoá. |
| BR-8-07 | Pricing thay đổi không ảnh hưởng Subscription đang `Active` (locked-in price). Subscription mới mới áp dụng giá mới. |
| BR-8-08 | Admin set Pro thủ công: tạo Subscription đặc biệt với `payment_type = manual`, ghi chú admin_note. Cũng được audit log. |

---

## 7. States & Workflows

### Subscription State Machine

```
[Teacher chọn Pro plan]
    │
    ▼
[Tạo PaymentIntent / order]
    │
    ├──[Teacher thanh toán thành công]──► Subscription: Active
    │                                         │
    │                              [Đến ngày gia hạn]
    │                                         │
    │                              ┌──────────┴──────────┐
    │                              │                      │
    │                    [Thanh toán thành công]  [Thanh toán thất bại]
    │                              │                      │
    │                           Active              PastDue (grace 3 ngày)
    │                                                     │
    │                                          ┌──────────┴──────────┐
    │                                          │                      │
    │                                [Thanh toán trong grace]  [Hết grace]
    │                                          │                      │
    │                                       Active                Expired
    │
    └──[Teacher huỷ]──► Active (đến hết kỳ) ──[hết kỳ]──► Cancelled
```

### Payment Flow

```
Teacher chọn plan + provider (Momo/VNPay)
    │
    ▼
Backend tạo Payment record (Pending) + gọi provider API
    │
    ▼
Redirect / QR cho Teacher thanh toán
    │
    ├──[Webhook: payment success]──► Payment = Completed
    │                                    │
    │                                    ▼
    │                              Tạo Invoice + kích hoạt Subscription
    │                                    │
    │                                    ▼
    │                              Notification: "Thanh toán thành công"
    │
    ├──[Webhook: payment failed]──► Payment = Failed
    │                                    │
    │                                    ▼
    │                              Notification: "Thanh toán thất bại, thử lại"
    │
    └──[Không có webhook trong 30 phút]──► Payment = Expired (order timeout)
```

---

## 8. Entities chính

| Entity | Mô tả ngắn |
|---|---|
| `plans` | id, name (Free/Pro), billing_cycle, price_vnd, max_classes, max_questions, max_exams, is_active |
| `subscriptions` | id, teacher_id, plan_id, status, started_at, expires_at, cancelled_at, payment_type (auto/manual), admin_note |
| `payments` | id, subscription_id, teacher_id, provider (momo/vnpay), amount_vnd, status, idempotency_key, provider_transaction_id, created_at, completed_at |
| `invoices` | id, payment_id, teacher_id, amount_vnd, issued_at, pdf_url (optional) |

---

## 9. Permission Matrix

| Action | Student | Teacher (own) | Admin |
|---|---|---|---|
| Xem plan pricing | Có | Có | Có |
| Mua / gia hạn plan | Không | Có | Không |
| Xem subscription của mình | Không | Có | Có (tất cả) |
| Huỷ subscription | Không | Có | Có |
| Xem payment history của mình | Không | Có | Có (tất cả) |
| Set Pro thủ công | Không | Không | Có |
| Xem revenue dashboard | Không | Không | Có |
| Xem invoice | Không | Có (của mình) | Có (tất cả) |

---

## 10. Validation & Edge Cases

| Case | Xử lý |
|---|---|
| Teacher tạo Class vượt limit `Free` | Trả lỗi 403: "Bạn đã đạt giới hạn X lớp. Nâng cấp Pro để tạo thêm." + link upgrade |
| Webhook payment đến 2 lần (retry từ provider) | `idempotency_key` check → skip lần 2, không apply lại |
| Teacher huỷ subscription nhưng vẫn muốn gia hạn trước khi hết kỳ | Cho phép re-activate subscription trước `expires_at` (tạo payment mới) |
| Subscription hết hạn khi Teacher đang có Class vượt limit Free | Không xoá Class; Teacher không tạo thêm được cho đến khi nâng lại Pro |
| Admin set Pro thủ công khi Teacher đang có Subscription Active | Kéo dài `expires_at` hoặc tạo Subscription override — quyết định khi implement |
| Payment webhook đến sau Subscription đã bị cancel | Kiểm tra timestamp; nếu webhook cho payment cũ → apply đúng period, không override cancel |
| VNPay / Momo API down → Teacher không thanh toán được | Timeout 30 phút → Payment = Expired; Teacher thử lại; retry queue ở background |

---

## 11. Acceptance Criteria

- [ ] Teacher `Free` tạo Class vượt limit → nhận lỗi rõ ràng với link upgrade.
- [ ] Teacher chọn Pro monthly → redirect đến payment page Momo hoặc VNPay.
- [ ] Thanh toán thành công → Subscription = `Active`; Teacher tạo Class không bị chặn.
- [ ] Webhook duplicate → chỉ apply 1 lần (idempotency).
- [ ] Invoice được tạo sau payment thành công; Teacher có thể tải về.
- [ ] Teacher huỷ → Subscription vẫn `Active` đến `expires_at` → sau đó `Cancelled`.
- [ ] Sau khi Subscription `Expired`: Teacher về Free, không thể tạo thêm tài nguyên vượt limit, tài nguyên cũ vẫn còn.
- [ ] Admin set Pro thủ công → Teacher có quyền Pro ngay.
- [ ] Admin xem revenue summary theo tháng đúng với payment history.
- [ ] Notification "Subscription sắp hết hạn" gửi trước 3 ngày.

---

## 12. Risks & Mitigations

| Rủi ro | Mức độ | Mitigation |
|---|---|---|
| Webhook không đến (network issue) → Subscription không kích hoạt | Cao | Polling fallback: check payment status sau 5 phút nếu chưa có webhook |
| Idempotency key collision | Thấp | Dùng UUID v4 + payment_id làm key |
| Thanh toán thành công nhưng Subscription không kích hoạt (race condition) | Cao | Transactional: payment update + subscription create trong 1 DB transaction |
| Teacher dispute: "tôi đã trả tiền nhưng không được Pro" | Trung bình | Audit log + payment record + idempotency key làm bằng chứng |
| Provider API thay đổi (Momo, VNPay) | Trung bình | Abstraction layer: `IPaymentProvider` interface, dễ swap provider |

---

## 13. Verification Plan

**Scenario 1 — Free plan limit:**
1. Teacher Free tạo Class đến giới hạn → tạo thêm → nhận lỗi với link upgrade.

**Scenario 2 — Payment happy path:**
1. Teacher chọn Pro monthly → payment page Momo.
2. Thực hiện thanh toán (test env) → webhook đến.
3. Subscription = Active; Invoice được tạo.
4. Teacher tạo Class vượt limit cũ → thành công.

**Scenario 3 — Idempotency:**
1. Gửi webhook payment success 2 lần với cùng `idempotency_key`.
2. Kiểm tra DB: chỉ 1 Subscription được tạo, 1 Invoice được tạo.

**Scenario 4 — Subscription expiry:**
1. Set `expires_at` = quá khứ cho 1 Subscription Active.
2. Background job chạy → Subscription = Expired.
3. Teacher không tạo thêm Class được.
4. Teacher gia hạn → Pro trở lại.

**Scenario 5 — Admin manual set:**
1. Admin set Pro cho Teacher với ghi chú "Partnership".
2. Subscription được tạo với `payment_type = manual`.
3. Audit log ghi nhận action.

---

## 14. Hook sang ROADMAP-FUTURE

MVP-8 hoàn chỉnh production-ready system với monetization cơ bản. Các mở rộng sau MVP-8 được mô tả trong [ROADMAP-FUTURE.md](./ROADMAP-FUTURE.md), bao gồm: OAuth social, AI features, mobile app, gamification, contest, LMS integration, Stripe, trial period, coupon codes, B2B licensing.
