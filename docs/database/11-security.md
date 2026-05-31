# 11 — Database Security

> Cross-cutting: áp dụng cho toàn bộ schema từ 01–09.

---

## 1. Password & Credential Hashing

### Algorithm

| Algorithm | Config | Dùng cho |
|---|---|---|
| **Argon2id** (recommended) | memory=64MB, iterations=3, parallelism=4 | Password mới (MVP-1+) |
| bcrypt | cost=12 | Fallback nếu Argon2id không available |

**Format lưu trong `users.password_hash`:**
```
{algorithm}${params}${hash}

-- Argon2id example:
$argon2id$v=19$m=65536,t=3,p=4$<salt_base64>$<hash_base64>

-- bcrypt example:
$2b$12$<salt_and_hash>
```

Prefix algorithm cho phép **migrate algorithm** sau này mà không cần force-reset password.

### Token Hashing

| Token Type | Algorithm | Store |
|---|---|---|
| Refresh token | SHA-256(raw_token) | `refresh_tokens.token_hash` |
| Password reset token | SHA-256(raw_token) | `password_reset_tokens.token_hash` |

**Không bao giờ store raw token trong DB.**

```
Flow:
1. Generate random_bytes(32) → raw_token
2. token_hash = SHA256(raw_token)
3. Store token_hash in DB
4. Send raw_token to client (cookie / response)
5. On verify: compute SHA256(received_token), compare with DB
```

---

## 2. PII (Personally Identifiable Information)

### PII Columns Inventory

| Table | Column | PII Level | Notes |
|---|---|---|---|
| `users` | `email` | High | Login identity |
| `users` | `display_name` | Medium | Có thể là tên thật |
| `users` | `avatar_url` | Low | URL ảnh, không phải ảnh |
| `users` | `bio` | Low | Optional, user-entered |
| `refresh_tokens` | `ip_address` | Medium | |
| `refresh_tokens` | `user_agent` | Low | |
| `attempts` | `ip_address` | Medium | Anti-cheating audit |
| `attempts` | `user_agent` | Low | |
| `audit_logs` | `ip_address` | Medium | |
| `payments` | `provider_metadata` | High | Có thể chứa masked card, phone number |

### Sensitive Columns — Không log, không expose

| Column | Rule |
|---|---|
| `users.password_hash` | NEVER log, NEVER include in JSON responses |
| `refresh_tokens.token_hash` | NEVER log |
| `password_reset_tokens.token_hash` | NEVER log |
| `payments.provider_metadata` | Log ở level DEBUG only, mask sensitive fields |

### Column-level Encryption (Optional for MVP-8)

Nếu cần PCI-DSS compliance cho payment data:

```sql
-- Dùng pgcrypto extension
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Encrypt: pgp_sym_encrypt(data::text, key)
-- Decrypt: pgp_sym_decrypt(encrypted, key)

-- payments.provider_metadata_encrypted TEXT
-- Key managed qua application env var (không store key trong DB)
```

> **MVP-8**: Đủ với JSONB unencrypted nếu DB server ở private network và column không store raw card numbers (Momo/VNPay không gửi raw card data trong webhook).

---

## 3. SQL Injection Prevention

### Parameterized Queries

EF Core **mặc định dùng parameterized queries** cho tất cả LINQ queries → safe by default.

```csharp
// Safe (EF Core)
var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

// Safe (raw SQL với parameters)
var result = await _db.Database.SqlQueryRaw<User>(
    "SELECT * FROM users WHERE email = {0}", email);

// UNSAFE — tuyệt đối không làm
var result = await _db.Database.ExecuteSqlRaw(
    $"SELECT * FROM users WHERE email = '{email}'");  // ← SQL INJECTION!
```

### Input Validation Layer

1. **API layer**: Validate length, format, type trước khi xuống DB
2. **DB layer**: CHECK constraints là last resort, không phải primary validation
3. **ORM layer**: EF Core FluentAPI constraints phải match DB constraints

---

## 4. Row-Level Security (RLS)

### Hiện tại (MVP-1 → MVP-7)

RLS **không bật** — app layer enforce access control.

Lý do:
- Complexity với EF Core (cần SET app.current_user_id cho mỗi connection)
- Single-tenant SaaS: simpler app-layer check đủ
- PgBouncer transaction mode: SET LOCAL không persist qua pool

### Tương lai (B2B multi-tenant)

Khi thêm `organization_id`, bật RLS:

```sql
-- Enable RLS
ALTER TABLE classes ENABLE ROW LEVEL SECURITY;

-- Policy: user chỉ thấy data của org mình
CREATE POLICY classes_org_isolation ON classes
  USING (organization_id = current_setting('app.current_org_id')::bigint);

-- Set context mỗi transaction
SET LOCAL app.current_org_id = '123';
```

> Chi tiết ở [13-extensibility.md](./13-extensibility.md)

---

## 5. Database User Permissions

### Principle of Least Privilege

```sql
-- Application user (EF Core runtime)
CREATE USER app_user WITH PASSWORD '...';
GRANT CONNECT ON DATABASE class_mgmt TO app_user;
GRANT USAGE ON SCHEMA public TO app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app_user;
-- KHÔNG GRANT: DROP TABLE, CREATE TABLE, ALTER TABLE, TRUNCATE

-- Migration user (chỉ dùng khi chạy migration)
CREATE USER migration_user WITH PASSWORD '...';
GRANT ALL ON DATABASE class_mgmt TO migration_user;
-- Hoặc dùng superuser chỉ trong migration pipeline

-- Read-only user (analytics, reporting)
CREATE USER readonly_user WITH PASSWORD '...';
GRANT CONNECT ON DATABASE class_mgmt TO readonly_user;
GRANT USAGE ON SCHEMA public TO readonly_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_user;
```

### Connection String Security

```
# Sai — hard-code trong config
ConnectionStrings__DefaultConnection=Host=localhost;Database=class_mgmt;Username=app_user;Password=secret123

# Đúng — dùng env var hoặc secret manager
ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}

# Đúng — dùng Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
```

---

## 6. Transport Security

### SSL/TLS for DB Connection

```
# postgresql.conf
ssl = on
ssl_cert_file = 'server.crt'
ssl_key_file = 'server.key'
ssl_ca_file = 'root.crt'  # Optional: client cert auth

# pg_hba.conf
hostssl  all  all  0.0.0.0/0  scram-sha-256
```

### App Connection String SSL

```
# .NET connection string
Host=db.prod.example.com;
Database=class_mgmt;
Username=app_user;
Password=...;
SSL Mode=Require;
Trust Server Certificate=false  -- Verify cert
```

---

## 7. Backup & Recovery

### Backup Strategy

| Tier | Frequency | Retention | Method |
|---|---|---|---|
| **Continuous WAL** | Real-time | 7 ngày | pg_receivewal hoặc cloud-managed |
| **Daily snapshot** | Daily 02:00 UTC | 30 ngày | pg_dump (compressed) |
| **Weekly snapshot** | Sunday 03:00 UTC | 3 tháng | pg_dump |
| **Monthly snapshot** | 1st of month | 1 năm | pg_dump |

### Backup Commands

```bash
# pg_dump compressed
pg_dump -Fc class_mgmt > backup_$(date +%Y%m%d).dump

# Restore
pg_restore -d class_mgmt backup_20260101.dump

# Point-in-time recovery (từ WAL)
# Requires continuous archiving → pg_hba.conf + archive_command setup
```

### Recovery Testing

> **Quan trọng**: Backup vô nghĩa nếu không test restore.

- Quarterly: restore full backup vào test environment, verify data integrity
- Monthly: test restore 1 table cụ thể (pg_restore với `--table`)

---

## 8. Audit Log Retention & Security

### Retention Policy

| Age | Action |
|---|---|
| 0–12 months | Hot storage (main DB) |
| 12–24 months | Cold storage (archive table hoặc external: S3, BigQuery) |
| > 24 months | Có thể delete theo legal/compliance requirement |

### Audit Log Protection

```sql
-- Revoke UPDATE và DELETE quyền của audit_logs cho app_user
REVOKE UPDATE, DELETE ON audit_logs FROM app_user;

-- Chỉ INSERT được phép
-- Verify:
\dp audit_logs  -- Check privileges

-- Hoặc dùng trigger để chặn update/delete
CREATE RULE no_update_audit_logs AS ON UPDATE TO audit_logs DO INSTEAD NOTHING;
CREATE RULE no_delete_audit_logs AS ON DELETE TO audit_logs DO INSTEAD NOTHING;
```

---

## 9. Rate Limiting (DB-side)

Rate limiting chủ yếu làm ở application layer, nhưng có thể thêm DB-side safeguards:

```sql
-- pg_hba.conf: giới hạn connection per user
-- Hoặc pg_bouncer max_client_conn settings

-- Phát hiện brute force: query audit_logs
SELECT COUNT(*) FROM audit_logs
WHERE action = 'user.login_failed'
  AND actor_id = (SELECT id FROM users WHERE email = $1)
  AND created_at > NOW() - INTERVAL '5 minutes';
-- App: nếu > 5 → rate limit 15 phút
```

---

## 10. GDPR & Data Privacy Prep

### User Data Deletion Request

Khi user yêu cầu xóa tài khoản (GDPR "right to be forgotten"):

1. **Soft delete** `users.deleted_at = NOW()`
2. **Anonymize PII** (không xóa hẳn vì ảnh hưởng data integrity):
   ```sql
   UPDATE users SET
     email = CONCAT('deleted_', id, '@removed.invalid'),
     display_name = 'Deleted User',
     avatar_url = NULL,
     bio = NULL,
     password_hash = NULL  -- invalidate login
   WHERE id = $1;
   ```
3. **Revoke sessions**: `UPDATE refresh_tokens SET revoked_at = NOW() WHERE user_id = $1`
4. **Retain** `audit_logs` (legal requirement, không PII nặng)
5. **Retain** `attempts`, `manual_grades` (academic records)
6. **Document** retention policy trong Privacy Policy

### Data Export Request

```sql
-- Export user data
SELECT
  u.email, u.display_name, u.created_at,
  c.name AS class_name,
  a.title AS assignment_title,
  at.total_score, at.submitted_at
FROM users u
LEFT JOIN class_memberships cm ON cm.student_id = u.id AND cm.status = 'Approved'
LEFT JOIN classes c ON c.id = cm.class_id
LEFT JOIN attempts at ON at.student_id = u.id
LEFT JOIN assignments a ON a.id = at.assignment_id
WHERE u.id = $1;
```

---

## 11. Security Checklist (Pre-deployment)

- [ ] Database user `app_user` không có DDL permissions
- [ ] SSL/TLS enabled cho DB connection
- [ ] Connection string trong env var / secret manager
- [ ] `audit_logs` có REVOKE UPDATE/DELETE cho `app_user`
- [ ] `password_hash` không xuất hiện trong logs
- [ ] `refresh_tokens` chỉ store hash
- [ ] Backup script chạy và đã test restore
- [ ] `pg_stat_statements` enabled cho slow query monitoring
- [ ] PgBouncer configured (transaction mode)
- [ ] Database accessible chỉ từ app server IP (firewall/security group)
- [ ] PostgreSQL version update policy documented
