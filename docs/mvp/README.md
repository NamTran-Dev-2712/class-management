# Tài liệu MVP — Class Management System

## Giới thiệu sản phẩm

**Class Management System** là nền tảng quản lý lớp học trực tuyến cho phép giáo viên tạo lớp, quản lý học sinh, xây dựng ngân hàng câu hỏi, tạo đề kiểm tra, giao bài cho lớp, chấm điểm tự động/thủ công, và theo dõi tiến độ học tập. Hệ thống phục vụ 3 role chính: **Student**, **Teacher**, và **Admin**.

---

## Cách đọc bộ tài liệu này

| File | Mục đích |
|---|---|
| [GLOSSARY.md](./GLOSSARY.md) | Định nghĩa chuẩn cho tất cả thuật ngữ — đọc trước khi đọc các MVP |
| [MVP-1.md](./MVP-1.md) → [MVP-7.md](./MVP-7.md) | Các giai đoạn triển khai core production |
| [MVP-8.md](./MVP-8.md) | Mở rộng sau production (Premium & Payment) |
| [MVP-9.md](./MVP-9.md) → [MVP-11.md](./MVP-11.md) | Mở rộng chiều sâu (Media, Proctoring, Advanced Question Types) |
| [ROADMAP-FUTURE.md](./ROADMAP-FUTURE.md) | Tính năng tương lai xa hơn (AI, Mobile, Gamification…) |

> **Quy ước:** Mỗi MVP-N.md chỉ mô tả **scope** (phạm vi) — user stories, business rules, states, permission, acceptance criteria. Không phải specification kỹ thuật hay API docs chi tiết.

---

## Roadmap tổng quan

| # | MVP | Tên | Mục tiêu chính | Phụ thuộc | Status |
|---|---|---|---|---|---|
| 1 | [MVP-1](./MVP-1.md) | Core Foundation | Auth + Role + Profile + Admin cơ bản | — | 🔲 Pending |
| 2 | [MVP-2](./MVP-2.md) | Classroom Management | Tạo lớp, mã invite, duyệt học sinh | MVP-1 | 🔲 Pending |
| 3 | [MVP-3](./MVP-3.md) | Question Bank | CRUD câu hỏi, 5 loại, tags, difficulty | MVP-1 | 🔲 Pending |
| 4 | [MVP-4](./MVP-4.md) | Exam Builder | Tạo đề từ câu hỏi, versioning | MVP-3 | 🔲 Pending |
| 5 | [MVP-5](./MVP-5.md) | Assignment & Online Testing | Giao bài, làm bài, snapshot, auto-grade | MVP-2 + MVP-4 | 🔲 Pending |
| 6 | [MVP-6](./MVP-6.md) | Manual Grading & Reports | Chấm tự luận, grade publishing, báo cáo | MVP-5 | 🔲 Pending |
| 7 | [MVP-7](./MVP-7.md) | Admin & Moderation | Report flow, audit log, notification, production hardening | MVP-6 | 🔲 Pending |
| 8 | [MVP-8](./MVP-8.md) | Premium & Payment | Subscription, Momo/VNPay, resource limits | MVP-7 | 🔲 Pending |
| 9 | [MVP-9](./MVP-9.md) | Media & Rich Content | Upload ảnh/audio/video, storage abstraction (R2), snapshot media | MVP-3 + MVP-5 + MVP-8 | 🔲 Pending |
| 10 | [MVP-10](./MVP-10.md) | Secure Exam Proctoring | Browser lockdown, phát hiện gian lận, log vi phạm | MVP-5 + MVP-7 | 🔲 Pending |
| 11 | [MVP-11](./MVP-11.md) | Advanced Question Types | Fill-in-Blank, Matching, Ordering (auto-grade) | MVP-3 + MVP-4 + MVP-5 | 🔲 Pending |
| — | [ROADMAP-FUTURE](./ROADMAP-FUTURE.md) | Future Extensions | AI, Mobile, Gamification, Contest, LMS… | MVP-11 | 🔲 Future |

---

## Dependency graph

```
MVP-1 (Auth/Role)
  ├── MVP-2 (Classroom)
  │     └── MVP-5 (Assignment) ◄──── MVP-4 (Exam)
  │                                        ▲
  └── MVP-3 (Question Bank) ──────────────┘
              MVP-5
                └── MVP-6 (Grading & Reports)
                      └── MVP-7 (Admin & Moderation) — Production Milestone ✅
                            └── MVP-8 (Premium & Payment)
                                  ├── MVP-9  (Media & Rich Content)      ◄── MVP-3 + MVP-5
                                  ├── MVP-10 (Secure Exam Proctoring)    ◄── MVP-5 + MVP-7
                                  ├── MVP-11 (Advanced Question Types)   ◄── MVP-3 + MVP-4 + MVP-5
                                  └── ROADMAP-FUTURE
```

> MVP-9 → MVP-11 là các nhánh mở-rộng-chiều-sâu sau monetization (MVP-8); chúng độc lập tương đối
> với nhau, có thể triển khai theo thứ tự ưu tiên feedback.

---

## Core value proposition

> **MVP-1 → MVP-7** tạo ra một sản phẩm hoàn chỉnh có thể deploy production:
> giáo viên tạo lớp → xây đề → giao bài → học sinh làm online → chấm điểm → xem báo cáo → admin quản trị.
>
> **MVP-8** thêm khả năng monetization.
>
> **MVP-9 → MVP-11** tăng chiều sâu sản phẩm: media phong phú trong câu hỏi (MVP-9), chống gian lận
> khi thi (MVP-10), và các loại câu hỏi nâng cao chấm tự động (MVP-11).
>
> **ROADMAP-FUTURE** là vision dài hạn, không ảnh hưởng timeline core.

---

## Nguyên tắc thiết kế xuyên suốt

1. **Snapshot bất biến** — Khi bài được giao, nội dung đề được snapshot. Teacher sửa câu hỏi sau không ảnh hưởng kết quả cũ.
2. **Permission theo resource** — Mọi action đều check ownership/membership. Không chỉ check role.
3. **State machine rõ ràng** — Assignment, Attempt, Subscription đều có trạng thái tường minh, không dùng boolean flags chồng chéo.
4. **Audit log cho hành động nhạy cảm** — Sửa điểm, sửa đáp án, khoá tài khoản, thanh toán đều được ghi log.
5. **Auto-save khi làm bài** — Học sinh không bao giờ mất bài do reload hay mất mạng tạm thời.
6. **Thuật ngữ nhất quán** — Luôn dùng theo [GLOSSARY.md](./GLOSSARY.md). Không dùng "bài thi / bài tập / đề thi" lẫn lộn.
