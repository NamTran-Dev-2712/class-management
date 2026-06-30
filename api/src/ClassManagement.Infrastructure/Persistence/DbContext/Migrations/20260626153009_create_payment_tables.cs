using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_payment_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs"
            );

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    public_id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "gen_random_uuid()"
                    ),
                    name = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    billing_cycle = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: true
                    ),
                    price_vnd = table.Column<long>(type: "bigint", nullable: false),
                    max_classes = table.Column<int>(type: "integer", nullable: true),
                    max_questions = table.Column<int>(type: "integer", nullable: true),
                    max_exams = table.Column<int>(type: "integer", nullable: true),
                    max_students_per_class = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    display_order = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    features = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plans", x => x.id);
                    table.CheckConstraint(
                        "chk_plans_billing_cycle",
                        "billing_cycle IS NULL OR billing_cycle IN ('Monthly', 'Annual')"
                    );
                    table.CheckConstraint(
                        "chk_plans_max_classes",
                        "max_classes IS NULL OR max_classes > 0"
                    );
                    table.CheckConstraint(
                        "chk_plans_max_exams",
                        "max_exams IS NULL OR max_exams > 0"
                    );
                    table.CheckConstraint(
                        "chk_plans_max_questions",
                        "max_questions IS NULL OR max_questions > 0"
                    );
                    table.CheckConstraint(
                        "chk_plans_max_students",
                        "max_students_per_class IS NULL OR max_students_per_class > 0"
                    );
                    table.CheckConstraint(
                        "chk_plans_name_length",
                        "length(name) >= 2 AND length(name) <= 50"
                    );
                    table.CheckConstraint("chk_plans_price", "price_vnd >= 0");
                }
            );

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    public_id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "gen_random_uuid()"
                    ),
                    teacher_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "Active"
                    ),
                    started_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    expires_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    cancelled_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    grace_period_ends_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    payment_type = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false,
                        defaultValue: "Auto"
                    ),
                    admin_note = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                    table.CheckConstraint(
                        "chk_subscriptions_admin_note_length",
                        "admin_note IS NULL OR length(admin_note) <= 500"
                    );
                    table.CheckConstraint(
                        "chk_subscriptions_payment_type",
                        "payment_type IN ('Auto', 'Manual')"
                    );
                    table.CheckConstraint(
                        "chk_subscriptions_status",
                        "status IN ('Active', 'PastDue', 'Cancelled', 'Expired')"
                    );
                    table.ForeignKey(
                        name: "fk_subscriptions_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_subscriptions_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    public_id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "gen_random_uuid()"
                    ),
                    subscription_id = table.Column<long>(type: "bigint", nullable: true),
                    teacher_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    provider = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    billing_cycle = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    amount_vnd = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "Pending"
                    ),
                    idempotency_key = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    provider_order_id = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    provider_transaction_id = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    provider_metadata = table.Column<string>(type: "jsonb", nullable: true),
                    expires_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    completed_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.CheckConstraint("chk_payments_amount", "amount_vnd > 0");
                    table.CheckConstraint(
                        "chk_payments_billing_cycle",
                        "billing_cycle IN ('Monthly', 'Annual')"
                    );
                    table.CheckConstraint("chk_payments_provider", "provider IN ('Momo', 'VnPay')");
                    table.CheckConstraint(
                        "chk_payments_status",
                        "status IN ('Pending', 'Completed', 'Failed', 'Expired')"
                    );
                    table.ForeignKey(
                        name: "fk_payments_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_payments_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_payments_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    public_id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "gen_random_uuid()"
                    ),
                    payment_id = table.Column<long>(type: "bigint", nullable: false),
                    subscription_id = table.Column<long>(type: "bigint", nullable: true),
                    teacher_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_number = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    amount_vnd = table.Column<long>(type: "bigint", nullable: false),
                    plan_name = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    billing_cycle = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    pdf_url = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    issued_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                    table.CheckConstraint("chk_invoices_amount", "amount_vnd > 0");
                    table.CheckConstraint(
                        "chk_invoices_billing_cycle",
                        "billing_cycle IN ('Monthly', 'Annual')"
                    );
                    table.ForeignKey(
                        name: "fk_invoices_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_invoices_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_invoices_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.AddCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs",
                sql: "action IN ('user.login', 'user.logout', 'user.login_failed', 'user.password_changed', 'user.password_reset_requested', 'user.password_reset_completed', 'user.role_changed', 'user.locked', 'user.unlocked', 'user.deleted', 'question.created', 'question.updated', 'question.correct_answer_changed', 'question.deleted', 'question.visibility_changed', 'exam.created', 'exam.updated', 'exam.deleted', 'exam.visibility_changed', 'assignment.published', 'assignment.closed', 'assignment.archived', 'attempt.started', 'attempt.submitted', 'attempt.auto_submitted', 'grade.manual_graded', 'grade.manual_grade_updated', 'grade.published', 'report.created', 'report.reviewed', 'report.resolved', 'report.rejected', 'admin.system_settings_changed', 'admin.assignment_force_closed', 'subscription.created', 'subscription.cancelled', 'subscription.reactivated', 'subscription.manual_set', 'subscription.expired', 'payment.created', 'payment.completed', 'payment.failed', 'invoice.issued')"
            );

            migrationBuilder.CreateIndex(
                name: "idx_invoices_teacher",
                table: "invoices",
                columns: new[] { "teacher_id", "issued_at" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "ix_invoices_subscription_id",
                table: "invoices",
                column: "subscription_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_invoices_number",
                table: "invoices",
                column: "invoice_number",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_invoices_payment",
                table: "invoices",
                column: "payment_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_invoices_public_id",
                table: "invoices",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_payments_pending_expires",
                table: "payments",
                column: "expires_at",
                filter: "status = 'Pending'"
            );

            migrationBuilder.CreateIndex(
                name: "idx_payments_provider_order",
                table: "payments",
                columns: new[] { "provider", "provider_order_id" }
            );

            migrationBuilder.CreateIndex(
                name: "idx_payments_status",
                table: "payments",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "idx_payments_teacher",
                table: "payments",
                columns: new[] { "teacher_id", "created_at" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "ix_payments_plan_id",
                table: "payments",
                column: "plan_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_payments_subscription_id",
                table: "payments",
                column: "subscription_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_payments_idempotency_key",
                table: "payments",
                column: "idempotency_key",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_payments_public_id",
                table: "payments",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_plans_active_order",
                table: "plans",
                columns: new[] { "is_active", "display_order" }
            );

            migrationBuilder.CreateIndex(
                name: "uq_plans_name_cycle",
                table: "plans",
                columns: new[] { "name", "billing_cycle" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_plans_public_id",
                table: "plans",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_expires",
                table: "subscriptions",
                column: "expires_at",
                filter: "status = 'Active'"
            );

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_past_due",
                table: "subscriptions",
                column: "grace_period_ends_at",
                filter: "status = 'PastDue'"
            );

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_teacher",
                table: "subscriptions",
                columns: new[] { "teacher_id", "status" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_plan_id",
                table: "subscriptions",
                column: "plan_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_subscriptions_public_id",
                table: "subscriptions",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_subscriptions_teacher_active",
                table: "subscriptions",
                column: "teacher_id",
                unique: true,
                filter: "status = 'Active'"
            );

            // Global, race-free source for invoice numbers (INV-YYYYMM-######). Not part of the EF model.
            migrationBuilder.Sql(
                "CREATE SEQUENCE IF NOT EXISTS payment_invoice_number_seq AS bigint START WITH 1 INCREMENT BY 1;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS payment_invoice_number_seq;");

            migrationBuilder.DropTable(name: "invoices");

            migrationBuilder.DropTable(name: "payments");

            migrationBuilder.DropTable(name: "subscriptions");

            migrationBuilder.DropTable(name: "plans");

            migrationBuilder.DropCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs"
            );

            migrationBuilder.AddCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs",
                sql: "action IN ('user.login', 'user.logout', 'user.login_failed', 'user.password_changed', 'user.password_reset_requested', 'user.password_reset_completed', 'user.role_changed', 'user.locked', 'user.unlocked', 'user.deleted', 'question.created', 'question.updated', 'question.correct_answer_changed', 'question.deleted', 'question.visibility_changed', 'exam.created', 'exam.updated', 'exam.deleted', 'exam.visibility_changed', 'assignment.published', 'assignment.closed', 'assignment.archived', 'attempt.started', 'attempt.submitted', 'attempt.auto_submitted', 'grade.manual_graded', 'grade.manual_grade_updated', 'grade.published', 'report.created', 'report.reviewed', 'report.resolved', 'report.rejected', 'admin.system_settings_changed', 'admin.assignment_force_closed')"
            );
        }
    }
}
