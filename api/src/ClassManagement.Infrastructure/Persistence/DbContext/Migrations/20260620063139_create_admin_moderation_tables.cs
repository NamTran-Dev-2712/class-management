using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_admin_moderation_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    action = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    actor_id = table.Column<long>(type: "bigint", nullable: true),
                    actor_role = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    target_type = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: true
                    ),
                    target_id = table.Column<long>(type: "bigint", nullable: true),
                    target_public_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(
                        type: "character varying(45)",
                        maxLength: 45,
                        nullable: true
                    ),
                    user_agent = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
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
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.CheckConstraint(
                        "chk_audit_logs_action",
                        "action IN ('user.login', 'user.logout', 'user.login_failed', 'user.password_changed', 'user.password_reset_requested', 'user.password_reset_completed', 'user.role_changed', 'user.locked', 'user.unlocked', 'user.deleted', 'question.created', 'question.updated', 'question.correct_answer_changed', 'question.deleted', 'question.visibility_changed', 'exam.created', 'exam.updated', 'exam.deleted', 'exam.visibility_changed', 'assignment.published', 'assignment.closed', 'assignment.archived', 'attempt.started', 'attempt.submitted', 'attempt.auto_submitted', 'grade.manual_graded', 'grade.manual_grade_updated', 'grade.published', 'report.created', 'report.reviewed', 'report.resolved', 'report.rejected', 'admin.system_settings_changed', 'admin.assignment_force_closed')"
                    );
                    table.CheckConstraint(
                        "chk_audit_logs_actor_role",
                        "actor_role IS NULL OR actor_role IN ('Student', 'Teacher', 'Admin', 'System')"
                    );
                    table.CheckConstraint(
                        "chk_audit_logs_target_type",
                        "target_type IS NULL OR target_type IN ('User', 'Class', 'Question', 'Exam', 'Assignment', 'Attempt', 'ManualGrade', 'Report', 'Subscription', 'Payment', 'SystemSetting')"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "notifications",
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
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    event_type = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    title = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    body = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    link = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    reference_id = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "Unread"
                    ),
                    read_at = table.Column<DateTime>(
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
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.CheckConstraint(
                        "chk_notifications_body_length",
                        "body IS NULL OR length(body) <= 500"
                    );
                    table.CheckConstraint(
                        "chk_notifications_status",
                        "status IN ('Unread', 'Read', 'Archived')"
                    );
                    table.CheckConstraint(
                        "chk_notifications_title_length",
                        "length(title) >= 1 AND length(title) <= 200"
                    );
                    table.ForeignKey(
                        name: "fk_notifications_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "reports",
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
                    reporter_id = table.Column<long>(type: "bigint", nullable: false),
                    target_type = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    target_id = table.Column<long>(type: "bigint", nullable: false),
                    target_public_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "Pending"
                    ),
                    admin_id = table.Column<long>(type: "bigint", nullable: true),
                    admin_action = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    admin_note = table.Column<string>(type: "text", nullable: true),
                    resolved_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
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
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.CheckConstraint(
                        "chk_reports_admin_action",
                        "admin_action IS NULL OR admin_action IN ('Dismiss', 'WarnUser', 'HideContent', 'DeleteContent', 'BanUser')"
                    );
                    table.CheckConstraint(
                        "chk_reports_admin_note_length",
                        "admin_note IS NULL OR length(admin_note) <= 1000"
                    );
                    table.CheckConstraint(
                        "chk_reports_description_length",
                        "description IS NULL OR length(description) <= 2000"
                    );
                    table.CheckConstraint(
                        "chk_reports_reason",
                        "reason IN ('InappropriateContent', 'Spam', 'Copyright', 'IncorrectAnswer', 'Other')"
                    );
                    table.CheckConstraint(
                        "chk_reports_status",
                        "status IN ('Pending', 'Reviewing', 'Resolved', 'Rejected')"
                    );
                    table.CheckConstraint(
                        "chk_reports_target_type",
                        "target_type IN ('Question', 'Exam', 'Assignment', 'Class', 'User')"
                    );
                    table.ForeignKey(
                        name: "fk_reports_asp_net_users_admin_id",
                        column: x => x.admin_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_reports_asp_net_users_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    key = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    description = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    value_type = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "string"
                    ),
                    is_public = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_settings", x => x.id);
                    table.CheckConstraint(
                        "chk_system_settings_value_type",
                        "value_type IN ('string', 'integer', 'boolean', 'json')"
                    );
                    table.ForeignKey(
                        name: "fk_system_settings_asp_net_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_action_created",
                table: "audit_logs",
                columns: new[] { "action", "created_at" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_actor_created",
                table: "audit_logs",
                columns: new[] { "actor_id", "created_at" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at",
                descending: new bool[0]
            );

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_target",
                table: "audit_logs",
                columns: new[] { "target_type", "target_id" }
            );

            migrationBuilder.CreateIndex(
                name: "idx_notifications_event_type",
                table: "notifications",
                columns: new[] { "event_type", "created_at" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "idx_notifications_user_status",
                table: "notifications",
                columns: new[] { "user_id", "status", "created_at" },
                descending: new[] { false, false, true }
            );

            migrationBuilder.CreateIndex(
                name: "idx_notifications_user_unread",
                table: "notifications",
                column: "user_id",
                filter: "status = 'Unread'"
            );

            migrationBuilder.CreateIndex(
                name: "uq_notifications_idempotent",
                table: "notifications",
                columns: new[] { "user_id", "event_type", "reference_id" },
                unique: true,
                filter: "reference_id IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "uq_notifications_public_id",
                table: "notifications",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_reports_admin",
                table: "reports",
                column: "admin_id",
                filter: "admin_id IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_reports_reporter",
                table: "reports",
                column: "reporter_id"
            );

            migrationBuilder.CreateIndex(
                name: "idx_reports_status",
                table: "reports",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "idx_reports_target",
                table: "reports",
                columns: new[] { "target_type", "target_id" }
            );

            migrationBuilder.CreateIndex(
                name: "uq_reports_idempotent",
                table: "reports",
                columns: new[] { "reporter_id", "target_type", "target_id" },
                unique: true,
                filter: "status IN ('Pending', 'Reviewing')"
            );

            migrationBuilder.CreateIndex(
                name: "uq_reports_public_id",
                table: "reports",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_updated_by",
                table: "system_settings",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_system_settings_key",
                table: "system_settings",
                column: "key",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "audit_logs");

            migrationBuilder.DropTable(name: "notifications");

            migrationBuilder.DropTable(name: "reports");

            migrationBuilder.DropTable(name: "system_settings");
        }
    }
}
